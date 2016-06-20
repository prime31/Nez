using System.Runtime.CompilerServices;


namespace Nez.Tiled
{
	public static class TiledTileExt
	{
		/// <summary>
		/// passthrough to TilesetTile
		/// </summary>
		/// <returns>The slope.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool isSlope( this TiledTile self )
		{
			return self.tilesetTile != null && self.tilesetTile.isSlope;
		}


		/// <summary>
		/// passthrough to TilesetTile
		/// </summary>
		/// <returns>The one way platform.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool isOneWayPlatform( this TiledTile self )
		{
			return self.tilesetTile != null && self.tilesetTile.isOneWayPlatform;
		}


		/// <summary>
		/// returns the slope top left taking flipping into account
		/// </summary>
		/// <returns>The slope top left.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int getSlopeTopLeft( this TiledTile self )
		{
			if( self.flippedHorizonally && self.flippedVertically )
				return self.tilesetTile.slopeTopLeft;
			if( self.flippedHorizonally )
				return self.tilesetTile.slopeTopRight;
			if( self.flippedVertically )
				return self.tilesetTile.tiledMap.tileWidth - self.tilesetTile.slopeTopLeft;

			return self.tilesetTile.slopeTopLeft;
		}


		/// <summary>
		/// returns the slope top right taking flipping into account
		/// </summary>
		/// <returns>The slope top right.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int getSlopeTopRight( this TiledTile self )
		{
			if( self.flippedHorizonally && self.flippedVertically )
				return self.tilesetTile.slopeTopRight;
			if( self.flippedHorizonally )
				return self.tilesetTile.slopeTopLeft;
			if( self.flippedVertically )
				return self.tilesetTile.tiledMap.tileWidth - self.tilesetTile.slopeTopRight;

			return self.tilesetTile.slopeTopRight;
		}


		/// <summary>
		/// calculates the slope based on the slope top left/right
		/// </summary>
		/// <returns>The slope.</returns>
		/// <param name="self">Self.</param>
		/// <param name="tileSize">Tile width.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float getSlope( this TiledTile self )
		{
			var tileSize = self.tilesetTile.tiledMap.tileWidth;

			// flip our slopes sign if the tile is flipped horizontally or vertically
			if( self.flippedHorizonally )
				tileSize *= -1;

			if( self.flippedVertically )
				tileSize *= -1;

			// rise of run
			return ( (float)self.tilesetTile.slopeTopRight - (float)self.tilesetTile.slopeTopLeft ) / (float)tileSize;
		}


		/// <summary>
		/// returns the slope position on the left side of the tile. b in the y = mx + b equation
		/// </summary>
		/// <returns>The slope offset.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float getSlopeOffset( this TiledTile self )
		{
			return (float)self.getSlopeTopLeft();
		}


		/// <summary>
		/// returns the edge on the side that has the tallest side
		/// </summary>
		/// <returns>The bigest slope edge.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Edge getHighestSlopeEdge( this TiledTile self )
		{
			var left = self.getSlopeTopLeft();
			var right = self.getSlopeTopRight();

			if( self.flippedVertically )
				return right > left ? Edge.Right : Edge.Left;
			return right > left ? Edge.Left : Edge.Right;
		}


		/// <summary>
		/// returns the nearest edge to worldPosition
		/// </summary>
		/// <returns>The nearest edge.</returns>
		/// <param name="self">Self.</param>
		/// <param name="worldPosition">World position.</param>
		/// <param name="tileWidth">Tile width.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Edge getNearestEdge( this TiledTile self, int worldPosition )
		{
			var tileWidth = self.tilesetTile.tiledMap.tileWidth;
			var tileMiddleWorldPosition = self.x * tileWidth + tileWidth / 2;
			return worldPosition < tileMiddleWorldPosition ? Edge.Left : Edge.Right;
		}

	}
}

