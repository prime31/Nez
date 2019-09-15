using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public class TmxTileset : TmxDocument, ITmxElement
	{
		public readonly TmxMap Map;
		public int FirstGid;
		public string Name { get; private set; }
		public int TileWidth;
		public int TileHeight;
		public int Spacing;
		public int Margin;
		public int? Columns;
		public int? TileCount;

		public Dictionary<int, TmxTilesetTile> Tiles;
		public TmxTileOffset TileOffset;
		public Dictionary<string, string> Properties;
		public TmxImage Image;
		public TmxList<TmxTerrain> Terrains;

		/// <summary>
		/// cache of the source rectangles for each tile
		/// </summary>
		public Dictionary<int, RectangleF> TileRegions;


		public static TmxTileset ParseTmxTileset(TmxMap map, XElement xTileset, string tmxDir)
		{
			// firstgid is always in TMX, but not TSX
			var xFirstGid = xTileset.Attribute("firstgid");
			var firstGid = (int)xFirstGid;
			var source = (string)xTileset.Attribute("source");

			// source will be null if this is an embedded TmxTileset, i.e. not external
			if (source != null)
			{
				// Prepend the parent TMX directory
				source = Path.Combine(tmxDir, source);

				// Everything else is in the TSX file
				using (var stream = TitleContainer.OpenStream(source))
				{
					var xDocTileset = XDocument.Load(stream);

					var tileset = new TmxTileset(map, xDocTileset.Element("tileset"), firstGid, tmxDir)
					{
						TmxDirectory = Path.GetDirectoryName(source)
					};

					return tileset;
				}
			}

			return new TmxTileset(map, xTileset, firstGid, tmxDir);
		}

		public TmxTileset(TmxMap map, XElement xTileset, int firstGid, string tmxDir)
		{
			Map = map;
			FirstGid = firstGid;

			Name = (string)xTileset.Attribute("name");
			TileWidth = (int)xTileset.Attribute("tilewidth");
			TileHeight = (int)xTileset.Attribute("tileheight");
			Spacing = (int?)xTileset.Attribute("spacing") ?? 0;
			Margin = (int?)xTileset.Attribute("margin") ?? 0;
			Columns = (int?)xTileset.Attribute("columns");
			TileCount = (int?)xTileset.Attribute("tilecount");
			TileOffset = new TmxTileOffset(xTileset.Element("tileoffset"));

			var xImage = xTileset.Element("image");
			if (xImage != null)
				Image = new TmxImage(xImage, tmxDir);

			var xTerrainType = xTileset.Element("terraintypes");
			if (xTerrainType != null)
			{
				Terrains = new TmxList<TmxTerrain>();
				foreach (var e in xTerrainType.Elements("terrain"))
					Terrains.Add(new TmxTerrain(e));
			}

			Tiles = new Dictionary<int, TmxTilesetTile>();
			foreach (var xTile in xTileset.Elements("tile"))
			{
				var tile = new TmxTilesetTile(this, xTile, Terrains, tmxDir);
				Tiles[tile.Id] = tile;
			}

			Properties = PropertyDict.ParsePropertyDict(xTileset.Element("properties"));

			// cache our source rects for each tile so we dont have to calculate them every time we render. If we have
			// an image this is a normal tileset, else its an image tileset
			TileRegions = new Dictionary<int, RectangleF>();
			if (Image != null)
			{
				var id = firstGid;
				for (var y = Margin; y < Image.Height - Margin; y += TileHeight + Spacing)
				{
					var column = 0;
					for (var x = Margin; x < Image.Width - Margin; x += TileWidth + Spacing)
					{
						TileRegions.Add(id++, new RectangleF(x, y, TileWidth, TileHeight));

						if (++column >= Columns)
							break;
					}
				}
			}
			else
			{
				// it seems that firstGid is always 0 for image tilesets so we can access them like an array here
				var id = firstGid;
				for (var i = 0; i < Tiles.Count; i++)
				{
					var tile = Tiles[i];
					TileRegions.Add(id++, new RectangleF(0, 0, tile.Image.Width, tile.Image.Height));
				}
			}
		}

		public void Update()
		{
			foreach (var kvPair in Tiles)
				kvPair.Value.UpdateAnimatedTiles();
		}

	}

	public class TmxTileOffset
	{
		public int X;
		public int Y;

		public TmxTileOffset(XElement xTileOffset)
		{
			if (xTileOffset == null)
			{
				X = 0;
				Y = 0;
			}
			else
			{
				X = (int)xTileOffset.Attribute("x");
				Y = (int)xTileOffset.Attribute("y");
			}
		}
	}


	public class TmxTerrain : ITmxElement
	{
		public string Name { get; private set; }
		public int Tile;
		public Dictionary<string, string> Properties;

		public TmxTerrain(XElement xTerrain)
		{
			Name = (string)xTerrain.Attribute("name");
			Tile = (int)xTerrain.Attribute("tile");
			Properties = PropertyDict.ParsePropertyDict(xTerrain.Element("properties"));
		}
	}


	public class TmxTilesetTile
	{
		public readonly TmxTileset Tileset;

		public int Id;
		public TmxTerrain[] TerrainEdges;
		public double Probability;
		public string Type;

		public Dictionary<string, string> Properties;
		public TmxImage Image;
		public TmxList<TmxObjectGroup> ObjectGroups;
		public List<TmxAnimationFrame> AnimationFrames;

		// HACK: why do animated tiles need to add the firstGid?
		public int currentAnimationFrameGid => AnimationFrames[_animationCurrentFrame].Gid + Tileset.FirstGid;
		float _animationElapsedTime;
		int _animationCurrentFrame;

		/// <summary>
		/// returns the value of an "nez:isDestructable" property if present in the properties dictionary
		/// </summary>
		/// <value><c>true</c> if is destructable; otherwise, <c>false</c>.</value>
		public bool IsDestructable;

		/// <summary>
		/// returns the value of a "nez:isSlope" property if present in the properties dictionary
		/// </summary>
		/// <value>The is slope.</value>
		public bool IsSlope;

		/// <summary>
		/// returns the value of a "nez:isOneWayPlatform" property if present in the properties dictionary
		/// </summary>
		public bool IsOneWayPlatform;

		/// <summary>
		/// returns the value of a "nez:slopeTopLeft" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top left.</value>
		public int SlopeTopLeft;

		/// <summary>
		/// returns the value of a "nez:slopeTopRight" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top right.</value>
		public int SlopeTopRight;


		public TmxTilesetTile(TmxTileset tileset, XElement xTile, TmxList<TmxTerrain> Terrains, string tmxDir = "")
		{
			Tileset = tileset;
			Id = (int)xTile.Attribute("id");

			var strTerrain = (string)xTile.Attribute("terrain");
			if (strTerrain != null)
			{
				TerrainEdges = new TmxTerrain[4];
				var index = 0;
				foreach (var v in strTerrain.Split(','))
				{
					var success = int.TryParse(v, out int result);

					TmxTerrain edge;
					if (success)
						edge = Terrains[result];
					else
						edge = null;
					TerrainEdges[index++] = edge;
				}
			}

			Probability = (double?)xTile.Attribute("probability") ?? 1.0;
			Type = (string)xTile.Attribute("type");
			var xImage = xTile.Element("image");
			if (xImage != null)
				Image = new TmxImage(xImage, tmxDir);

			ObjectGroups = new TmxList<TmxObjectGroup>();
			foreach (var e in xTile.Elements("objectgroup"))
				ObjectGroups.Add(new TmxObjectGroup(tileset.Map, e));

			AnimationFrames = new List<TmxAnimationFrame>();
			if (xTile.Element("animation") != null)
			{
				foreach (var e in xTile.Element("animation").Elements("frame"))
					AnimationFrames.Add(new TmxAnimationFrame(e));
			}

			Properties = PropertyDict.ParsePropertyDict(xTile.Element("properties"));

			if (Properties != null)
				ProcessProperties();
		}

		void ProcessProperties()
		{
			string value;
			if (Properties.TryGetValue("nez:isDestructable", out value))
				IsDestructable = bool.Parse(value);

			if (Properties.TryGetValue("nez:isSlope", out value))
				IsSlope = bool.Parse(value);

			if (Properties.TryGetValue("nez:isOneWayPlatform", out value))
				IsOneWayPlatform = bool.Parse(value);

			if (Properties.TryGetValue("nez:slopeTopLeft", out value))
				SlopeTopLeft = int.Parse(value);

			if (Properties.TryGetValue("nez:slopeTopRight", out value))
				SlopeTopRight = int.Parse(value);
		}

		public void UpdateAnimatedTiles()
		{
			if (AnimationFrames.Count == 0)
				return;

			_animationElapsedTime += Time.DeltaTime;

			if (_animationElapsedTime > AnimationFrames[_animationCurrentFrame].Duration)
			{
				_animationCurrentFrame = Mathf.IncrementWithWrap(_animationCurrentFrame, AnimationFrames.Count);
				_animationElapsedTime = 0;
			}
		}
	}


	public class TmxAnimationFrame
	{
		public int Gid;
		public float Duration;

		public TmxAnimationFrame(XElement xFrame)
		{
			Gid = (int)xFrame.Attribute("tileid");
			Duration = (float)xFrame.Attribute("duration") / 1000f;
		}
	}

}
