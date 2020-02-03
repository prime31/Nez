using System.Collections.Generic;

namespace Nez.Tiled
{
	public class TmxTileset : TmxDocument, ITmxElement
	{
		public TmxMap Map;
		public int FirstGid;
		public string Name { get; set; }
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
	}

	public class TmxTerrain : ITmxElement
	{
		public string Name { get; set; }
		public int Tile;
		public Dictionary<string, string> Properties;
	}
}
