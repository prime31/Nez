using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
    public partial class TmxLayer : ITmxLayer
    {

	    /// <summary>
	    /// gets the TmxLayerTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
	    /// </summary>
	    /// <returns>The tile.</returns>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    public TmxLayerTile GetTile(int x, int y) 
	    {
		    return GetTile(x + y * Width);
	    }
	    
	    public TmxLayerTile GetTile(int index) 
	    {
		    Tiles.TryGetValue(Grid[index], out var tmxLayerTile);

		    return tmxLayerTile;
	    }

		/// <summary>
		/// gets the TmxLayerTile at the given world position
		/// </summary>
		public TmxLayerTile GetTileAtWorldPosition(Vector2 pos)
		{
			var worldPoint = Map.WorldToTilePosition(pos);
			return GetTile(worldPoint.X, worldPoint.Y);
		}

		/// <summary>
		/// Returns a list of rectangles in tile space, where any non-null tile is combined into bounding regions
		/// </summary>
		public List<Rectangle> GetCollisionRectangles()
		{
			var checkedIndexes = new bool?[Grid.Length];
			var rectangles = new List<Rectangle>();
			var startCol = -1;
			var index = -1;

			for (var y = 0; y < Map.Height; y++)
			{
				for (var x = 0; x < Map.Width; x++)
				{
					index = y * Map.Width + x;
					var tile = GetTile(x, y);

					if (tile != null && (checkedIndexes[index] == false || checkedIndexes[index] == null))
					{
						if (startCol < 0)
							startCol = x;

						checkedIndexes[index] = true;
					}
					else if (tile == null || checkedIndexes[index] == true)
					{
						if (startCol >= 0)
						{
							rectangles.Add(FindBoundsRect(startCol, x, y, checkedIndexes));
							startCol = -1;
						}
					}
				} // end for x

				if (startCol >= 0)
				{
					rectangles.Add(FindBoundsRect(startCol, Map.Width, y, checkedIndexes));
					startCol = -1;
				}
			}

			return rectangles;
		}

		/// <summary>
		/// Finds the largest bounding rect around tiles between startX and endX, starting at startY and going
		/// down as far as possible
		/// </summary>
		public Rectangle FindBoundsRect(int startX, int endX, int startY, bool?[] checkedIndexes)
		{
			var index = -1;

			for (var y = startY + 1; y < Map.Height; y++)
			{
				for (var x = startX; x < endX; x++)
				{
					index = y * Map.Width + x;
					var tile = GetTile(x, y);

					if (tile == null || checkedIndexes[index] == true)
					{
						// Set everything we've visited so far in this row to false again because it won't be included in the rectangle and should be checked again
						for (var _x = startX; _x < x; _x++)
						{
							index = y * Map.Width + _x;
							checkedIndexes[index] = false;
						}

						return new Rectangle(startX * Map.TileWidth, startY * Map.TileHeight,
							(endX - startX) * Map.TileWidth, (y - startY) * Map.TileHeight);
					}

					checkedIndexes[index] = true;
				}
			}

			return new Rectangle(startX * Map.TileWidth, startY * Map.TileHeight,
				(endX - startX) * Map.TileWidth, (Map.Height - startY) * Map.TileHeight);
		}

		/// <summary>
		/// gets a List of all the TiledTiles that intersect the passed in Rectangle. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		public List<TmxLayerTile> GetTilesIntersectingBounds(Rectangle bounds)
		{
			var minX = Map.WorldToTilePositionX(bounds.X);
			var minY = Map.WorldToTilePositionY(bounds.Y);
			var maxX = Map.WorldToTilePositionX(bounds.Right);
			var maxY = Map.WorldToTilePositionY(bounds.Bottom);

			var tilelist = ListPool<TmxLayerTile>.Obtain();

			for (var x = minX; x <= maxX; x++)
			{
				for (var y = minY; y <= maxY; y++)
				{
					var tile = GetTile(x, y);
					if (tile != null)
						tilelist.Add(tile);
				}
			}

			return tilelist;
		}

		/// <summary>
		/// sets the tile.
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="tile">Tile.</param>
		[Obsolete("Please use SetTile with gid instead.")]
		public TmxLayerTile SetTile(int x, int y, TmxLayerTile tile)
		{
			return SetTile(x, y, tile.Gid, tile.HorizontalFlip, tile.VerticalFlip, tile.DiagonalFlip);
		}
		
		/// <summary>
		/// Sets the tile at position x, y.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="gid">Global Tile ID (without the flip flags)</param>
		/// <param name="flipHorizontally">Should the tile be flipped horizontally?</param>
		/// <param name="flipVertically">Should the tile be flipped vertically?</param>
		/// <param name="flipDiagonally">Should the tile be flipped diagonally?</param>
		/// <returns>The tile.</returns>
		public TmxLayerTile SetTile(int x, int y, int gid, bool flipHorizontally = false, bool flipVertically = false, bool flipDiagonally = false)
		{
			if (gid == 0) return null;
			
			uint rawGid = TmxLayerTile.GetRawGid(gid, flipHorizontally, flipVertically, flipDiagonally);
			
			Grid[x + y * Width] = rawGid;

			TmxLayerTile tileToSet;

			if (!Tiles.TryGetValue(rawGid, out var tmxLayerTile)) 
			{
				tileToSet = new TmxLayerTile(Map, rawGid);
				Tiles.Add(rawGid, tileToSet);
			}
			else
				tileToSet = tmxLayerTile;
			
			return tileToSet;
		}

		/// <summary>
		/// nulls out the tile at the x/y coordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void RemoveTile(int x, int y) {
			Grid[x + y * Width] = 0;
		}
    }
}