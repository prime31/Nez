using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledTileLayer : TiledLayer
	{
		public readonly TiledMap TiledMap;
		public int Width;
		public int Height;
		public readonly TiledTile[] Tiles;
		public Color Color = Color.White;


		public int TileWidth
		{
			get { return TiledMap.TileWidth; }
		}

		public int TileHeight
		{
			get { return TiledMap.TileHeight; }
		}


		public TiledTileLayer(TiledMap map, string name, int width, int height, TiledTile[] tiles) : base(name)
		{
			this.Width = width;
			this.Height = height;
			this.Tiles = tiles;

			TiledMap = map;
			tiles = PopulateTilePositions();
		}


		public TiledTileLayer(TiledMap map, string name, int width, int height) : this(map, name, width, height,
			new TiledTile[width * height])
		{
		}


		/// <summary>
		/// loops through the tiles and sets each tiles x/y value
		/// </summary>
		/// <returns>The tile positions.</returns>
		TiledTile[] PopulateTilePositions()
		{
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					if (Tiles[x + y * Width] != null)
					{
						Tiles[x + y * Width].X = x;
						Tiles[x + y * Width].Y = y;
					}
				}
			}

			return Tiles;
		}


		public override void Draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds)
		{
			Draw(batcher, position, Vector2.One, layerDepth, cameraClipBounds);
		}


		public override void Draw(Batcher batcher, Vector2 position, Vector2 scale, float layerDepth,
		                          RectangleF cameraClipBounds)
		{
			// offset it by the entity position since the tilemap will always expect positions in its own coordinate space
			cameraClipBounds.Location -= (position + Offset);

			int minX, minY, maxX, maxY;
			if (TiledMap.requiresLargeTileCulling)
			{
				// we expand our cameraClipBounds by the excess tile width/height of the largest tiles to ensure we include tiles whose
				// origin might be outside of the cameraClipBounds
				minX = TiledMap.WorldToTilePositionX(cameraClipBounds.Left -
				                                     (TiledMap.LargestTileWidth - TiledMap.TileWidth));
				minY = TiledMap.WorldToTilePositionY(cameraClipBounds.Top -
				                                     (TiledMap.LargestTileHeight - TiledMap.TileHeight));
				maxX = TiledMap.WorldToTilePositionX(cameraClipBounds.Right +
				                                     (TiledMap.LargestTileWidth - TiledMap.TileWidth));
				maxY = TiledMap.WorldToTilePositionY(cameraClipBounds.Bottom +
				                                     (TiledMap.LargestTileHeight - TiledMap.TileHeight));
			}
			else
			{
				minX = TiledMap.WorldToTilePositionX(cameraClipBounds.Left);
				minY = TiledMap.WorldToTilePositionY(cameraClipBounds.Top);
				maxX = TiledMap.WorldToTilePositionX(cameraClipBounds.Right);
				maxY = TiledMap.WorldToTilePositionY(cameraClipBounds.Bottom);
			}

			// loop through and draw all the non-culled tiles
			for (var y = minY; y <= maxY; y++)
			{
				for (var x = minX; x <= maxX; x++)
				{
					var tile = GetTile(x, y);
					if (tile == null)
						continue;

					var tileRegion = tile.TextureRegion;

					// culling for arbitrary size tiles if necessary
					if (TiledMap.requiresLargeTileCulling)
					{
						// TODO: this only checks left and bottom. we should check top and right as well to deal with rotated, odd-sized tiles
						var tileworldpos = TiledMap.TileToWorldPosition(new Point(x, y));
						if (tileworldpos.X + tileRegion.SourceRect.Width < cameraClipBounds.Left ||
						    tileworldpos.Y - tileRegion.SourceRect.Height > cameraClipBounds.Bottom)
							continue;
					}

					// for the y position, we need to take into account if the tile is larger than the tileHeight and shift. Tiled uses
					// a bottom-left coordinate system and MonoGame a top-left
					var tx = tile.X * TiledMap.TileWidth * scale.X + (int) position.X;
					var ty = tile.Y * TiledMap.TileHeight * scale.Y + (int) position.Y;
					var rotation = 0f;

					var spriteEffects = SpriteEffects.None;
					if (tile.FlippedHorizonally)
						spriteEffects |= SpriteEffects.FlipHorizontally;
					if (tile.FlippedVertically)
						spriteEffects |= SpriteEffects.FlipVertically;
					if (tile.FlippedDiagonally)
					{
						if (tile.FlippedHorizonally && tile.FlippedVertically)
						{
							spriteEffects ^= SpriteEffects.FlipVertically;
							rotation = MathHelper.PiOver2;
							tx += TiledMap.TileHeight + (tileRegion.SourceRect.Height - TiledMap.TileHeight);
							ty -= (tileRegion.SourceRect.Width - TiledMap.TileWidth);
						}
						else if (tile.FlippedHorizonally)
						{
							spriteEffects ^= SpriteEffects.FlipVertically;
							rotation = -MathHelper.PiOver2;
							ty += TiledMap.TileHeight;
						}
						else if (tile.FlippedVertically)
						{
							spriteEffects ^= SpriteEffects.FlipHorizontally;
							rotation = MathHelper.PiOver2;
							tx += TiledMap.TileWidth + (tileRegion.SourceRect.Height - TiledMap.TileHeight);
							ty += (TiledMap.TileWidth - tileRegion.SourceRect.Width);
						}
						else
						{
							spriteEffects ^= SpriteEffects.FlipHorizontally;
							rotation = -MathHelper.PiOver2;
							ty += TiledMap.TileHeight;
						}
					}

					// if we had no rotations (diagonal flipping) shift our y-coord to account for any non-tileSized tiles to account for
					// Tiled being bottom-left origin
					if (rotation == 0)
						ty += (TiledMap.TileHeight - tileRegion.SourceRect.Height);

					batcher.Draw(tileRegion, new Vector2(tx, ty) + Offset, Color, rotation, Vector2.Zero, scale,
						spriteEffects, layerDepth);
				}
			}
		}


		#region Tile management

		/// <summary>
		/// gets the TiledTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public TiledTile GetTile(int x, int y)
		{
			return Tiles[x + y * Width];
		}


		/// <summary>
		/// gets the TiledTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetTile<T>(int x, int y) where T : TiledTile
		{
			return Tiles[x + y * Width] as T;
		}


		/// <summary>
		/// sets the tile and updates its tileset
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="tile">Tile.</param>
		public TiledTile SetTile(TiledTile tile)
		{
			Tiles[tile.X + tile.Y * Width] = tile;
			tile.Tileset = TiledMap.GetTilesetForTileId(tile.Id);

			return tile;
		}


		/// <summary>
		/// nulls out the tile at the x/y coordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void RemoveTile(int x, int y)
		{
			Tiles[x + y * Width] = null;
		}

		#endregion


		/// <summary>
		/// returns the bounds Rectangle of the passed in tile
		/// </summary>
		/// <returns>The bounds for tile.</returns>
		/// <param name="tile">Tile.</param>
		/// <param name="tilemap">Tilemap.</param>
		public static Rectangle GetBoundsForTile(TiledTile tile, TiledMap tilemap)
		{
			return new Rectangle(tile.X * tilemap.TileWidth, tile.Y * tilemap.TileHeight, tilemap.TileWidth,
				tilemap.TileHeight);
		}


		/// <summary>
		/// note that world position assumes that the Vector2 was normalized to be in the tilemaps coordinates. i.e. if the tilemap
		/// is not at 0,0 then the world position should be moved so that it takes into consideration the tilemap offset from 0,0.
		/// Example: if the tilemap is at 300,300 then the passed in value should be worldPos - (300,300)
		/// </summary>
		/// <returns>The tile at world position.</returns>
		/// <param name="pos">Position.</param>
		public TiledTile GetTileAtWorldPosition(Vector2 pos)
		{
			return GetTile(TiledMap.WorldToTilePositionX(pos.X), TiledMap.WorldToTilePositionY(pos.Y));
		}


		/// <summary>
		/// Returns a list of rectangles in tile space, where any non-null tile is combined into bounding regions
		/// </summary>
		/// <param name="layer">Layer.</param>
		public List<Rectangle> GetCollisionRectangles()
		{
			var checkedIndexes = new bool?[Tiles.Length];
			var rectangles = new List<Rectangle>();
			var startCol = -1;
			var index = -1;

			for (var y = 0; y < TiledMap.Height; y++)
			{
				for (var x = 0; x < TiledMap.Width; x++)
				{
					index = y * TiledMap.Width + x;
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
					rectangles.Add(FindBoundsRect(startCol, TiledMap.Width, y, checkedIndexes));
					startCol = -1;
				}
			}

			return rectangles;
		}


		/// <summary>
		/// Finds the largest bounding rect around tiles between startX and endX, starting at startY and going
		/// down as far as possible
		/// </summary>
		/// <returns>The bounds rect.</returns>
		/// <param name="startX">Start x.</param>
		/// <param name="endX">End x.</param>
		/// <param name="startY">Start y.</param>
		/// <param name="checkedIndexes">Checked indexes.</param>
		public Rectangle FindBoundsRect(int startX, int endX, int startY, bool?[] checkedIndexes)
		{
			var index = -1;

			for (var y = startY + 1; y < TiledMap.Height; y++)
			{
				for (var x = startX; x < endX; x++)
				{
					index = y * TiledMap.Width + x;
					var tile = GetTile(x, y);

					if (tile == null || checkedIndexes[index] == true)
					{
						// Set everything we've visited so far in this row to false again because it won't be included in the rectangle and should be checked again
						for (var _x = startX; _x < x; _x++)
						{
							index = y * TiledMap.Width + _x;
							checkedIndexes[index] = false;
						}

						return new Rectangle(startX * TiledMap.TileWidth, startY * TiledMap.TileHeight,
							(endX - startX) * TiledMap.TileWidth, (y - startY) * TiledMap.TileHeight);
					}

					checkedIndexes[index] = true;
				}
			}

			return new Rectangle(startX * TiledMap.TileWidth, startY * TiledMap.TileHeight,
				(endX - startX) * TiledMap.TileWidth, (TiledMap.Height - startY) * TiledMap.TileHeight);
		}


		/// <summary>
		/// gets a List of all the TiledTiles that intersect the passed in Rectangle. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The tiles intersecting bounds.</returns>
		/// <param name="layer">Layer.</param>
		/// <param name="bounds">Bounds.</param>
		public List<TiledTile> GetTilesIntersectingBounds(Rectangle bounds)
		{
			var minX = TiledMap.WorldToTilePositionX(bounds.X);
			var minY = TiledMap.WorldToTilePositionY(bounds.Y);
			var maxX = TiledMap.WorldToTilePositionX(bounds.Right);
			var maxY = TiledMap.WorldToTilePositionY(bounds.Bottom);

			var tilelist = ListPool<TiledTile>.Obtain();

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
		/// casts a line from start to end returning the first solid tile it intersects. Note that start and end and clamped to the tilemap
		/// bounds so make sure you pass in valid positions else you may get odd results!
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public TiledTile Linecast(Vector2 start, Vector2 end)
		{
			var direction = end - start;

			// worldToTilePosition clamps to the tilemaps bounds so no need to worry about overlow
			var startCell = TiledMap.WorldToTilePosition(start);
			var endCell = TiledMap.WorldToTilePosition(end);

			start.X /= TiledMap.TileWidth;
			start.Y /= TiledMap.TileHeight;

			// what tile are we on
			var intX = startCell.X;
			var intY = startCell.Y;

			// ensure our start cell exists
			if (intX < 0 || intX >= TiledMap.Width || intY < 0 || intY >= TiledMap.Height)
				return null;

			// which way we go
			var stepX = Math.Sign(direction.X);
			var stepY = Math.Sign(direction.Y);

			// Calculate cell boundaries. when the step is positive, the next cell is after this one meaning we add 1.
			// If negative, cell is before this one in which case dont add to boundary
			var boundaryX = intX + (stepX > 0 ? 1 : 0);
			var boundaryY = intY + (stepY > 0 ? 1 : 0);

			// determine the value of t at which the ray crosses the first vertical tile boundary. same for y/horizontal.
			// The minimum of these two values will indicate how much we can travel along the ray and still remain in the current tile
			// may be infinite for near vertical/horizontal rays
			var tMaxX = (boundaryX - start.X) / direction.X;
			var tMaxY = (boundaryY - start.Y) / direction.Y;
			if (direction.X == 0f)
				tMaxX = float.PositiveInfinity;
			if (direction.Y == 0f)
				tMaxY = float.PositiveInfinity;

			// how far do we have to walk before crossing a cell from a cell boundary. may be infinite for near vertical/horizontal rays
			var tDeltaX = stepX / direction.X;
			var tDeltaY = stepY / direction.Y;

			// start walking and returning the intersecting tiles
			var tile = Tiles[intX + intY * Width];
			if (tile != null)
				return tile;

			while (intX != endCell.X || intY != endCell.Y)
			{
				if (tMaxX < tMaxY)
				{
					intX += stepX;
					tMaxX += tDeltaX;
				}
				else
				{
					intY += stepY;
					tMaxY += tDeltaY;
				}

				tile = Tiles[intX + intY * Width];
				if (tile != null)
					return tile;
			}

			return null;
		}
	}
}