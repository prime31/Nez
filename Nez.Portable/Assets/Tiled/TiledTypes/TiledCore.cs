using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Tiled
{
	public class TmxDocument
	{
		public string TmxDirectory { get; protected set; }

		public TmxDocument() => TmxDirectory = string.Empty;

		protected XDocument ReadXml(string filepath)
		{
			using (var stream = TitleContainer.OpenStream(filepath))
			{
				var xDoc = XDocument.Load(stream);
				TmxDirectory = Path.GetDirectoryName(filepath);

				return xDoc;
			}
		}
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


	public static class PropertyDict
	{
		public static Dictionary<string, string> ParsePropertyDict(XContainer xmlProp)
		{
			if (xmlProp == null)
				return null;

			var dict = new Dictionary<string, string>();
			foreach (var p in xmlProp.Elements("property"))
			{
				string pname, pval;

				pname = p.Attribute("name").Value;
				try
				{
					pval = p.Attribute("value").Value;
				}
				catch (NullReferenceException)
				{
					// Fallback to element value if no "value"
					pval = p.Value;
				}

				dict.Add(pname, pval);
			}
			return dict;
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

		public TmxImage(XElement xImage, string tmxDir = "")
		{
			var xSource = xImage.Attribute("source");
			if (xSource != null)
			{
				// Append directory if present
				Source = Path.Combine(tmxDir, (string)xSource);

				using (var stream = TitleContainer.OpenStream(Source))
					Texture = Texture2D.FromStream(Core.GraphicsDevice, stream);
			}
			else
			{
				Format = (string)xImage.Attribute("format");
				var xData = xImage.Element("data");
				var decodedStream = new TmxBase64Data(xData);
				Data = decodedStream.Data;
				throw new NotSupportedException("Stream Data loading is not yet supported");
			}

			Trans = TmxColor.ParseColor(xImage.Attribute("trans"));
			Width = (int?)xImage.Attribute("width") ?? 0;
			Height = (int?)xImage.Attribute("height") ?? 0;
		}

		public void Dispose()
		{
			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}
		}
	}


	public static class TmxColor
	{
		public static Color ParseColor(XAttribute xColor)
		{
			if (xColor == null)
				return Color.White;

			var colorStr = ((string)xColor).TrimStart("#".ToCharArray());
			return ColorExt.HexToColor(colorStr);
		}
	}


	public class TmxBase64Data
	{
		public Stream Data { get; private set; }

		public TmxBase64Data(XElement xData)
		{
			if ((string)xData.Attribute("encoding") != "base64")
				throw new Exception("TmxBase64Data: Only Base64-encoded data is supported.");

			var rawData = Convert.FromBase64String((string)xData.Value);
			Data = new MemoryStream(rawData, false);

			var compression = (string)xData.Attribute("compression");
			if (compression == "gzip")
			{
				Data = new GZipStream(Data, CompressionMode.Decompress);
			}
			else if (compression == "zlib")
			{
				// Strip 2-byte header and 4-byte checksum
				// TODO: Validate header here
				var bodyLength = rawData.Length - 6;
				var bodyData = new byte[bodyLength];
				Array.Copy(rawData, 2, bodyData, 0, bodyLength);

				var bodyStream = new MemoryStream(bodyData, false);
				Data = new DeflateStream(bodyStream, CompressionMode.Decompress);

				// TODO: Validate checksum?
			}
			else if (compression != null)
			{
				throw new Exception("TmxBase64Data: Unknown compression.");
			}
		}
	}

}
