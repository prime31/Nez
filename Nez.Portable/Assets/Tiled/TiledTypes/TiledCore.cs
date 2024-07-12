using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Nez.Tiled
{
	public class TmxDocument
	{
		public string TmxDirectory;

		public TmxDocument() => TmxDirectory = string.Empty;
	}


	public interface ITmxElement
	{
		string Name { get; }
	}


	public class TmxList<T> : KeyedCollection<string, T> where T : ITmxElement
	{
		Dictionary<string, int> _nameCount = new Dictionary<string, int>();

		public new void Add(T t)
		{
			var tName = t.Name;

			// Rename duplicate entries by appending a number
			if (Contains(tName))
				_nameCount[tName] += 1;
			else
				_nameCount.Add(tName, 0);
			base.Add(t);
		}

		protected override string GetKeyForItem(T item)
		{
			var name = item.Name;
			var count = _nameCount[name];

			var dupes = 0;

			// For duplicate keys, append a counter. For pathological cases, insert underscores to ensure uniqueness
			while (Contains(name))
			{
				name = name + String.Concat(Enumerable.Repeat("_", dupes)) + count.ToString();
				dupes++;
			}

			return name;
		}
	}

	public class TmxImage : IDisposable
	{
		public Texture2D Texture;
		public string Source;
		public string Format;
		public Stream Data;
		public Color Trans;
		public int Width;
		public int Height;

		public void Dispose()
		{
			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}
		}
	}


	public class TmxBase64Data
	{
		public Stream Data { get; private set; }

		public TmxBase64Data(XElement xData)
		{
			if ((string)xData.Attribute("encoding") != "base64")
				throw new Exception("TmxBase64Data: Only Base64-encoded data is supported.");

			var rawData = Convert.FromBase64String(xData.Value);
			Data = new MemoryStream(rawData, false);

			var compression = (string)xData.Attribute("compression");
			if (compression == "gzip")
			{
				using (MemoryStream compressedStream = new MemoryStream(rawData))
				{
					MemoryStream decompressedStream = new MemoryStream();

					using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
					{
						gzipStream.CopyTo(decompressedStream);
					}

					// Reset the position of the decompressed stream to the beginning
					decompressedStream.Position = 0;
					Data = decompressedStream;
				}
			}
			else if (compression == "zlib")
			{
				// Strip 2-byte zlib header
				using (MemoryStream bodyStream = new MemoryStream(rawData, 2, rawData.Length - 2, false))
				{

					MemoryStream decompressedStream = new MemoryStream();
					using (DeflateStream deflateStream = new DeflateStream(bodyStream, CompressionMode.Decompress))
					{
						deflateStream.CopyTo(decompressedStream);
					}

					// Reset the position of the decompressed stream to the beginning
					decompressedStream.Position = 0;
					Data = decompressedStream;
				}
			}
			else if (compression != null)
			{
				throw new Exception("TmxBase64Data: Unknown compression.");
			}
		}
	}

}
