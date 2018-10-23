using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledIsometricTiledLayer : TiledTileLayer
	{
		public readonly TiledMap tiledMap;
		public int width;
		public int height;
		public readonly TiledTile[] tiles;
		public Color color = Color.White;


		public int tileWidth { get { return tiledMap.tileWidth; } }

		public int tileHeight { get { return tiledMap.tileHeight; } }


		public TiledIsometricTiledLayer( TiledMap map, string name, int width, int height, TiledTile[] tiles ) : base(map, name, width, height)
		{
			this.width = width;
			this.height = height;
			this.tiles = tiles;

			tiledMap = map;
			tiles = populateTilePositions();
		}


		public TiledIsometricTiledLayer( TiledMap map, string name, int width, int height ) : this( map, name, width, height, new TiledTile[width * height] )
		{ }

		/// <summary>
		/// loops through the tiles and sets each tiles x/y value
		/// </summary>
		/// <returns>The tile positions.</returns>
		TiledTile[] populateTilePositions()
		{
			for( var y = 0; y < height; y++ )
			{
				for( var x = 0; x < width; x++ )
				{
					if( tiles[x + y * width] != null )
					{
						tiles[x + y * width].x = x;
						tiles[x + y * width].y = y;
					}
				}
			}

			return tiles;
		}


		public override void draw( Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds )
		{
			// offset render position by the layer offset, used for isometric layer depth
			position += offset;

			// offset it by the entity position since the tilemap will always expect positions in its own coordinate space
			cameraClipBounds.location -= position;
			if( tiledMap.requiresLargeTileCulling )
			{
				// we expand our cameraClipBounds by the excess tile width/height of the largest tiles to ensure we include tiles whose
				// origin might be outside of the cameraClipBounds
				cameraClipBounds.location -= new Vector2( tiledMap.largestTileWidth, tiledMap.largestTileHeight - tiledMap.tileHeight );
				cameraClipBounds.size += new Vector2( tiledMap.largestTileWidth, tiledMap.largestTileHeight - tiledMap.tileHeight );
			}

			Point min, max;
			min = new Point( 0, 0 );
			max = new Point( tiledMap.width - 1, tiledMap.height - 1 );

			// loop through and draw all the non-culled tiles
			for( var y = min.Y; y <= max.Y; y++ )
			{
				for( var x = min.X; x <= max.X; x++ )
				{
					var tile = getTile( x, y );
					if( tile == null )
						continue;

					var tileworldpos = tiledMap.isometricTileToWorldPosition( x, y );
					if( tileworldpos.X < cameraClipBounds.left || tileworldpos.Y < cameraClipBounds.top || tileworldpos.X > cameraClipBounds.right || tileworldpos.Y > cameraClipBounds.bottom )
						continue;

					var tileRegion = tile.textureRegion;

					// culling for arbitrary size tiles if necessary
					if( tiledMap.requiresLargeTileCulling )
					{
						// TODO: this only checks left and bottom. we should check top and right as well to deal with rotated, odd-sized tiles
						if( tileworldpos.X + tileRegion.sourceRect.Width < cameraClipBounds.left || tileworldpos.Y - tileRegion.sourceRect.Height > cameraClipBounds.bottom )
							continue;
					}

					drawTile( batcher, position, layerDepth, x, y, 1 );
				}
			}
		}


		public void drawTile( Batcher batcher, Vector2 position, float layerDepth, int x, int y, float scale )
		{
			var tile = getTile( x, y );
			if( tile == null )
				return;

			var t = tiledMap.isometricTileToWorldPosition( x, y );
			var tileRegion = tile.textureRegion;

			// for the y position, we need to take into account if the tile is larger than the tileHeight and shift. Tiled uses
			// a bottom-left coordinate system and MonoGame a top-left
			var rotation = 0f;

			var spriteEffects = SpriteEffects.None;
			if( tile.flippedHorizonally )
				spriteEffects |= SpriteEffects.FlipHorizontally;
			if( tile.flippedVertically )
				spriteEffects |= SpriteEffects.FlipVertically;
			if( tile.flippedDiagonally )
			{
				if( tile.flippedHorizonally && tile.flippedVertically )
				{
					spriteEffects ^= SpriteEffects.FlipVertically;
					rotation = MathHelper.PiOver2;
					t.X += tiledMap.tileHeight + ( tileRegion.sourceRect.Height - tiledMap.tileHeight );
					t.Y -= ( tileRegion.sourceRect.Width - tiledMap.tileWidth );
				}
				else if( tile.flippedHorizonally )
				{
					spriteEffects ^= SpriteEffects.FlipVertically;
					rotation = -MathHelper.PiOver2;
					t.Y += tiledMap.tileHeight;
				}
				else if( tile.flippedVertically )
				{
					spriteEffects ^= SpriteEffects.FlipHorizontally;
					rotation = MathHelper.PiOver2;
					t.X += tiledMap.tileWidth + ( tileRegion.sourceRect.Height - tiledMap.tileHeight );
					t.Y += ( tiledMap.tileWidth - tileRegion.sourceRect.Width );
				}
				else
				{
					spriteEffects ^= SpriteEffects.FlipHorizontally;
					rotation = -MathHelper.PiOver2;
					t.Y += tiledMap.tileHeight;
				}
			}

			// if we had no rotations (diagonal flipping) shift our y-coord to account for any non-tileSized tiles to account for
			// Tiled being bottom-left origin
			if( rotation == 0 )
				t.Y += ( tiledMap.tileHeight - tileRegion.sourceRect.Height );

            // Scale the tile's relative position, but not the origin
            t = t * new Vector2(scale) + position;

			batcher.draw( tileRegion.texture2D, t, tileRegion.sourceRect, color, rotation, Vector2.Zero, scale, spriteEffects, layerDepth );
		}


		#region Tile management

		/// <summary>
		/// gets the TiledTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public TiledTile getTile( int x, int y )
		{
			return tiles[x + y * width];
		}


		/// <summary>
		/// gets the TiledTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getTile<T>( int x, int y ) where T : TiledTile
		{
			return tiles[x + y * width] as T;
		}


		/// <summary>
		/// sets the tile and updates its tileset
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="tile">Tile.</param>
		public TiledTile setTile( TiledTile tile )
		{
			tiles[tile.x + tile.y * width] = tile;
			tile.tileset = tiledMap.getTilesetForTileId( tile.id );

			return tile;
		}


		/// <summary>
		/// nulls out the tile at the x/y coordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void removeTile( int x, int y )
		{
			tiles[x + y * width] = null;
		}

		#endregion


		/// <summary>
		/// returns the bounds Rectangle of the passed in tile
		/// </summary>
		/// <returns>The bounds for tile.</returns>
		/// <param name="tile">Tile.</param>
		/// <param name="tilemap">Tilemap.</param>
		public static Rectangle getBoundsForTile( TiledTile tile, TiledMap tilemap )
		{
			return new Rectangle( tile.x * tilemap.tileWidth, tile.y * tilemap.tileHeight, tilemap.tileWidth, tilemap.tileHeight );
		}


		/// <summary>
		/// note that world position assumes that the Vector2 was normalized to be in the tilemaps coordinates. i.e. if the tilemap
		/// is not at 0,0 then the world position should be moved so that it takes into consideration the tilemap offset from 0,0.
		/// Example: if the tilemap is at 300,300 then the passed in value should be worldPos - (300,300)
		/// </summary>
		/// <returns>The tile at world position.</returns>
		/// <param name="pos">Position.</param>
		public TiledTile getTileAtWorldPosition( Vector2 pos )
		{
			Point tilePos = tiledMap.isometricWorldToTilePosition( pos );
			return getTile( tilePos.X, tilePos.Y );
		}


		/// <summary>
		/// Returns a list of rectangles in tile space, where any non-null tile is combined into bounding regions
		/// </summary>
		/// <param name="layer">Layer.</param>
		public List<Rectangle> getCollisionRectangles()
		{
			var checkedIndexes = new bool?[tiles.Length];
			var rectangles = new List<Rectangle>();
			var startCol = -1;
			var index = -1;

			for( var y = 0; y < tiledMap.height; y++ )
			{
				for( var x = 0; x < tiledMap.width; x++ )
				{
					index = y * tiledMap.width + x;
					var tile = getTile( x, y );

					if( tile != null && ( checkedIndexes[index] == false || checkedIndexes[index] == null ) )
					{
						if( startCol < 0 )
							startCol = x;

						checkedIndexes[index] = true;
					}
					else if( tile == null || checkedIndexes[index] == true )
					{
						if( startCol >= 0 )
						{
							rectangles.Add( findBoundsRect( startCol, x, y, checkedIndexes ) );
							startCol = -1;
						}
					}
				} // end for x

				if( startCol >= 0 )
				{
					rectangles.Add( findBoundsRect( startCol, tiledMap.width, y, checkedIndexes ) );
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
		public Rectangle findBoundsRect( int startX, int endX, int startY, bool?[] checkedIndexes )
		{
			var index = -1;

			for( var y = startY + 1; y < tiledMap.height; y++ )
			{
				for( var x = startX; x < endX; x++ )
				{
					index = y * tiledMap.width + x;
					var tile = getTile( x, y );

					if( tile == null || checkedIndexes[index] == true )
					{
						// Set everything we've visited so far in this row to false again because it won't be included in the rectangle and should be checked again
						for( var _x = startX; _x < x; _x++ )
						{
							index = y * tiledMap.width + _x;
							checkedIndexes[index] = false;
						}

						return new Rectangle( startX * tiledMap.tileWidth, startY * tiledMap.tileHeight, ( endX - startX ) * tiledMap.tileWidth, ( y - startY ) * tiledMap.tileHeight );
					}

					checkedIndexes[index] = true;
				}
			}

			return new Rectangle( startX * tiledMap.tileWidth, startY * tiledMap.tileHeight, ( endX - startX ) * tiledMap.tileWidth, ( tiledMap.height - startY ) * tiledMap.tileHeight );
		}


		/// <summary>
		/// gets a List of all the TiledTiles that intersect the passed in Rectangle. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The tiles intersecting bounds.</returns>
		/// <param name="bounds">Bounds.</param>
		public List<TiledTile> getTilesIntersectingBounds( Rectangle bounds )
		{
			// Note that after transforming the bounds to isometric, top left is the left most tile, bottom right is the right most, top right is top most, bottom left is bottom most
			var topLeft = tiledMap.isometricWorldToTilePosition( bounds.X, bounds.Y );
			var bottomLeft = tiledMap.isometricWorldToTilePosition( bounds.X, bounds.Bottom );
			var topRight = tiledMap.isometricWorldToTilePosition( bounds.Right, bounds.Y );
			var bottomRight = tiledMap.isometricWorldToTilePosition( bounds.Right, bounds.Bottom );

			var tilelist = ListPool<TiledTile>.obtain();
			// Iterate in larger isometric rectangle that definitely includes our smaller bounding rectangle
			for( var x = topLeft.X; x <= bottomRight.X; x++ )
			{
				for( var y = topRight.Y; y <= bottomLeft.Y; y++ )
				{
					var tile = getTile( x, y );
					// Check if tile collides with our bounds
					if( tile != null && bounds.Contains( tiledMap.isometricTileToWorldPosition( x, y ) ) )
						tilelist.Add( tile );
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
		public TiledTile linecast( Vector2 start, Vector2 end )
		{
			var direction = end - start;

			// worldToTilePosition clamps to the tilemaps bounds so no need to worry about overlow
			var startCell = tiledMap.isometricWorldToTilePosition( start );
			var endCell = tiledMap.isometricWorldToTilePosition( end );

			start.X /= tiledMap.tileWidth;
			start.Y /= tiledMap.tileHeight;

			// what tile are we on
			var intX = startCell.X;
			var intY = startCell.Y;

			// ensure our start cell exists
			if( intX < 0 || intX >= tiledMap.width || intY < 0 || intY >= tiledMap.height )
				return null;

			// which way we go
			var stepX = Math.Sign( direction.X );
			var stepY = Math.Sign( direction.Y );

			// Calculate cell boundaries. when the step is positive, the next cell is after this one meaning we add 1.
			// If negative, cell is before this one in which case dont add to boundary
			var boundaryX = intX + ( stepX > 0 ? 1 : 0 );
			var boundaryY = intY + ( stepY > 0 ? 1 : 0 );

			// determine the value of t at which the ray crosses the first vertical tile boundary. same for y/horizontal.
			// The minimum of these two values will indicate how much we can travel along the ray and still remain in the current tile
			// may be infinite for near vertical/horizontal rays
			var tMaxX = ( boundaryX - start.X ) / direction.X;
			var tMaxY = ( boundaryY - start.Y ) / direction.Y;
			if( direction.X == 0f )
				tMaxX = float.PositiveInfinity;
			if( direction.Y == 0f )
				tMaxY = float.PositiveInfinity;

			// how far do we have to walk before crossing a cell from a cell boundary. may be infinite for near vertical/horizontal rays
			var tDeltaX = stepX / direction.X;
			var tDeltaY = stepY / direction.Y;

			// start walking and returning the intersecting tiles
			var tile = tiles[intX + intY * width];
			if( tile != null )
				return tile;

			while( intX != endCell.X || intY != endCell.Y )
			{
				if( tMaxX < tMaxY )
				{
					intX += stepX;
					tMaxX += tDeltaX;
				}
				else
				{
					intY += stepY;
					tMaxY += tDeltaY;
				}

				tile = tiles[intX + intY * width];
				if( tile != null )
					return tile;
			}

			return null;
		}

	}
}
