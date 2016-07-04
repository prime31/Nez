using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Tiled
{
	public class TiledTileLayer : TiledLayer
	{
		public readonly TiledMap tiledMap;
		public int width;
		public int height;
		public readonly TiledTile[] tiles;
		public Color color = Color.White;


		public int tileWidth
		{
			get { return tiledMap.tileWidth; }
		}

		public int tileHeight
		{
			get { return tiledMap.tileHeight; }
		}


		public TiledTileLayer( TiledMap map, string name, int width, int height, TiledTile[] tiles ) : base( name )
		{
			this.width = width;
			this.height = height;
			this.tiles = tiles;

			tiledMap = map;
			tiles = populateTilePositions();
		}


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
			// offset it by the entity position since the tilemap will always expect positions in its own coordinate space
			cameraClipBounds.location -= position;

			var minX = tiledMap.worldToTilePositionX( cameraClipBounds.left );
			var minY = tiledMap.worldToTilePositionY( cameraClipBounds.top );
			var maxX = tiledMap.worldToTilePositionX( cameraClipBounds.right );
			var maxY = tiledMap.worldToTilePositionY( cameraClipBounds.bottom );

			// loop through and draw all the non-culled tiles
			for( var y = minY; y <= maxY; y++ )
			{
				for( var x = minX; x <= maxX; x++ )
				{
					var tile = getTile( x, y );
					if( tile == null )
						continue;

					var tileRegion = tile.textureRegion;

					// for the y position, we need to take into account if the tile is larger than the tiledHeight and shift. Tiled uses
					// a bottom-left coordinate system and MonoGame a top-left
					var tx = tile.x * tiledMap.tileWidth + (int)position.X;
					var ty = tile.y * tiledMap.tileHeight + (int)position.Y;
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
							tx += tiledMap.tileHeight + ( tileRegion.sourceRect.Height - tiledMap.tileHeight );
							ty -= ( tileRegion.sourceRect.Width - tiledMap.tileWidth );
						}
						else if( tile.flippedHorizonally )
						{
							spriteEffects ^= SpriteEffects.FlipVertically;
							rotation = -MathHelper.PiOver2;
							ty += tiledMap.tileHeight;
						}
						else if( tile.flippedVertically )
						{
							spriteEffects ^= SpriteEffects.FlipHorizontally;
							rotation = MathHelper.PiOver2;
							tx += tiledMap.tileWidth + ( tileRegion.sourceRect.Height - tiledMap.tileHeight );
							ty += ( tiledMap.tileWidth - tileRegion.sourceRect.Width );
						}
						else
						{
							spriteEffects ^= SpriteEffects.FlipHorizontally;
							rotation = -MathHelper.PiOver2;
							ty += tiledMap.tileHeight;
						}
					}

					// if we had no rotations (diagonal flipping) shift our y-coord to account for any non-tileSized tiles to account for
					// Tiled being bottom-left origin
					if( rotation == 0 )
						ty += ( tiledMap.tileHeight - tileRegion.sourceRect.Height );

					batcher.draw( tileRegion.texture2D, new Vector2( tx, ty ), tileRegion.sourceRect, color, rotation, Vector2.Zero, 1, spriteEffects, layerDepth );
				}
			}
		}


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
		/// nulls out the tile at the x/y coordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void removeTile( int x, int y )
		{
			tiles[x + y * width] = null;
		}


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
			return getTile( tiledMap.worldToTilePositionX( pos.X ), tiledMap.worldToTilePositionY( pos.Y ) );
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
		/// <param name="layer">Layer.</param>
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
		/// <param name="layer">Layer.</param>
		/// <param name="bounds">Bounds.</param>
		public List<TiledTile> getTilesIntersectingBounds( Rectangle bounds )
		{
			var minX = tiledMap.worldToTilePositionX( bounds.X );
			var minY = tiledMap.worldToTilePositionY( bounds.Y );
			var maxX = tiledMap.worldToTilePositionX( bounds.Right );
			var maxY = tiledMap.worldToTilePositionY( bounds.Bottom );

			var tilelist = ListPool<TiledTile>.obtain();

			for( var x = minX; x <= maxX; x++ )
			{
				for( var y = minY; y <= maxY; y++ )
				{
					var tile = getTile( x, y );
					if( tile != null )
						tilelist.Add( tile );
				}
			}

			return tilelist;
		}

	}
}