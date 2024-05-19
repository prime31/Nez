using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public partial class TmxLayer : ITmxLayer
	{
		public TmxMap Map;
		public string Name { get; set; }
		public float Opacity { get; set; }
		public bool Visible { get; set; }
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		public Vector2 Offset => new Vector2(OffsetX, OffsetY);
		public float ParallaxFactorX { get; set; }
		public float ParallaxFactorY { get; set; }
		public Vector2 ParallaxFactor => new Vector2(ParallaxFactorX, ParallaxFactorY);

		public Dictionary<string, string> Properties { get; set; }

		/// <summary>
		/// width in tiles for this layer. Always the same as the map width for fixed-size maps.
		/// </summary>
		public int Width;

		/// <summary>
		/// height in tiles for this layer. Always the same as the map height for fixed-size maps.
		/// </summary>
		public int Height;
		public uint[] Grid;
		public Dictionary<uint, TmxLayerTile> Tiles;
		
		/// <summary>
		/// returns the TmxLayerTile with gid. This is a slow lookup so cache it!
		/// </summary>
		/// <param name="gid"></param>
		/// <returns></returns>
		public TmxLayerTile GetTileWithGid(uint gid) {
			Tiles.TryGetValue(gid, out var result);
			return result;
		}
	}

	public class TmxLayerTile
	{
		const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
		const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
		const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

		public readonly TmxTileset Tileset;
		public readonly int Gid;
		public readonly bool HorizontalFlip;
		public readonly bool VerticalFlip;
		public readonly bool DiagonalFlip;

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
		
		public TmxLayerTile(TmxMap map, uint rawGid)
		{
			// Scan for tile flip bit flags
			bool flip;
			flip = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
			HorizontalFlip = flip;

			flip = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
			VerticalFlip = flip;

			flip = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;
			DiagonalFlip = flip;

			Gid = ClearFlipFlags(rawGid);
			Tileset = map.GetTilesetForTileGid(Gid);
		}

		/// <summary>
		/// Applies the given flip flags to the given Global Tile ID.
		/// </summary>
		/// <param name="gid">Global Tile ID (without the flip flags)</param>
		/// <param name="flipHorizontally">Horizontally flipped?</param>
		/// <param name="flipVertically">Vertically flipped?</param>
		/// <param name="flipDiagonally">Diagonally flipped?</param>
		/// <returns>Global Tile ID (with the flip flags)</returns>
		public static uint GetRawGid(int gid, bool flipHorizontally, bool flipVertically, bool flipDiagonally) 
		{
			if (gid == 0) return 0;

			var rawGid = (uint)gid;

			if (flipHorizontally) rawGid |= FLIPPED_HORIZONTALLY_FLAG;
			if (flipVertically) rawGid |= FLIPPED_VERTICALLY_FLAG;
			if (flipDiagonally) rawGid |= FLIPPED_DIAGONALLY_FLAG;

			return rawGid;
		}
		
		/// <summary>
		/// Clears flip flags from the given Global Tile Id.
		/// </summary>
		/// <param name="rawGid">Global Tile ID (with the flip flags)</param>
		/// <returns>Global Tile ID (without the flip flags)</returns>
		public static int ClearFlipFlags(uint rawGid)
		{
			return (int)(rawGid & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
		}
	}

}
