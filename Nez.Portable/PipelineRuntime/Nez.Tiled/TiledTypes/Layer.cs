using System;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public partial class TmxLayer : ITmxLayer
	{
		public readonly TmxMap Map;
		public string Name { get; private set; }
		public float Opacity { get; private set; }
		public bool Visible { get; private set; }
		public float OffsetX { get; private set; }
		public float OffsetY { get; private set; }
		public Vector2 Offset => new Vector2(OffsetX, OffsetY);
		public Dictionary<string, string> Properties { get; private set; }

		/// <summary>
		/// width in tiles for this layer. Always the same as the map width for fixed-size maps.
		/// </summary>
		public int Width;

		/// <summary>
		/// height in tiles for this layer. Always the same as the map height for fixed-size maps.
		/// </summary>
		public int Height;
		public TmxLayerTile[] Tiles;

		public TmxLayer(TmxMap map, XElement xLayer, int width, int height)
		{
			Map = map;
			Name = (string)xLayer.Attribute("name");
			Opacity = (float?)xLayer.Attribute("opacity") ?? 1.0f;
			Visible = (bool?)xLayer.Attribute("visible") ?? true;
			OffsetX = (float?)xLayer.Attribute("offsetx") ?? 0.0f;
			OffsetY = (float?)xLayer.Attribute("offsety") ?? 0.0f;
			// TODO: does the width/height passed in ever differ from the TMX layer XML?
			Width = (int)xLayer.Attribute("width");
			Height = (int)xLayer.Attribute("height");

			var xData = xLayer.Element("data");
			var encoding = (string)xData.Attribute("encoding");

			Tiles = new TmxLayerTile[width * height];
			if (encoding == "base64")
			{
				var decodedStream = new TmxBase64Data(xData);
				var stream = decodedStream.Data;

				var index = 0;
				using (var br = new BinaryReader(stream))
					for (int j = 0; j < height; j++)
						for (int i = 0; i < width; i++)
						{
							var gid = br.ReadUInt32();
							Tiles[index++] = gid != 0 ? new TmxLayerTile(map, gid, i, j) : null;
						}
			}
			else if (encoding == "csv")
			{
				var csvData = (string)xData.Value;
				int k = 0;
				foreach (var s in csvData.Split(','))
				{
					var gid = uint.Parse(s.Trim());
					var x = k % width;
					var y = k / width;

					Tiles[k++] = gid != 0 ? new TmxLayerTile(map, gid, x, y) : null;
				}
			}
			else if (encoding == null)
			{
				int k = 0;
				foreach (var e in xData.Elements("tile"))
				{
					var gid = (uint?)e.Attribute("gid") ?? 0;

					var x = k % width;
					var y = k / width;

					Tiles[k++] = gid != 0 ? new TmxLayerTile(map, gid, x, y) : null;
				}
			}
			else throw new Exception("TmxLayer: Unknown encoding.");

			Properties = PropertyDict.ParsePropertyDict(xLayer.Element("properties"));
		}

		/// <summary>
		/// returns the TmxLayerTile with gid. This is a slow lookup so cache it!
		/// </summary>
		/// <param name="gid"></param>
		/// <returns></returns>
		public TmxLayerTile GetTileWithGid(int gid)
		{
			for (var i = 0; i < Tiles.Length; i++)
			{
				if (Tiles[i] != null && Tiles[i].Gid == gid)
					return Tiles[i];
			}
			return null;
		}
	}


	public class TmxLayerTile
	{
		const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
		const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
		const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

		public TmxTileset Tileset;
		public int Gid;
		public int X;
		public int Y;
		public Vector2 Position => new Vector2(X, Y);
		public bool HorizontalFlip;
		public bool VerticalFlip;
		public bool DiagonalFlip;

		int? _tilesetTileIndex;

		/// <summary>
		/// gets the TmxTilesetTile for this TmxLayerTile if it exists. TmxTilesetTile only exist for animated tiles and tiles with attached
		/// properties.
		/// </summary>
		public TmxTilesetTile TilesetTile
		{
			get
			{
				if (!_tilesetTileIndex.HasValue)
				{
					_tilesetTileIndex = -1;
					if (Tileset.FirstGid <= Gid)
					{
						if (Tileset.Tiles.TryGetValue(Gid - Tileset.FirstGid, out var tilesetTile))
						{
							_tilesetTileIndex = Gid - Tileset.FirstGid;
						}
					}
				}

				if (_tilesetTileIndex.Value < 0)
					return null;

				return Tileset.Tiles[_tilesetTileIndex.Value];
			}
		}

		public TmxLayerTile(TmxMap map, uint id, int x, int y)
		{
			X = x;
			Y = y;
			var rawGid = id;

			// Scan for tile flip bit flags
			bool flip;
			flip = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
			HorizontalFlip = flip;

			flip = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
			VerticalFlip = flip;

			flip = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;
			DiagonalFlip = flip;

			// Zero the bit flags
			rawGid &= ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

			// Save GID remainder to int
			Gid = (int)rawGid;
			Tileset = map.GetTilesetForTileGid(Gid);
		}
	}

}
