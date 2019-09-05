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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsSlope(this TiledTile self)
		{
			return self.TilesetTile != null && self.TilesetTile.IsSlope;
		}


		/// <summary>
		/// passthrough to TilesetTile
		/// </summary>
		/// <returns>The one way platform.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneWayPlatform(this TiledTile self)
		{
			return self.TilesetTile != null && self.TilesetTile.IsOneWayPlatform;
		}


		/// <summary>
		/// returns the slope top left taking flipping into account
		/// </summary>
		/// <returns>The slope top left.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSlopeTopLeft(this TiledTile self)
		{
			if (self.FlippedHorizonally && self.FlippedVertically)
				return self.TilesetTile.SlopeTopLeft;
			if (self.FlippedHorizonally)
				return self.TilesetTile.SlopeTopRight;
			if (self.FlippedVertically)
				return self.TilesetTile.TiledMap.TileWidth - self.TilesetTile.SlopeTopLeft;

			return self.TilesetTile.SlopeTopLeft;
		}


		/// <summary>
		/// returns the slope top right taking flipping into account
		/// </summary>
		/// <returns>The slope top right.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSlopeTopRight(this TiledTile self)
		{
			if (self.FlippedHorizonally && self.FlippedVertically)
				return self.TilesetTile.SlopeTopRight;
			if (self.FlippedHorizonally)
				return self.TilesetTile.SlopeTopLeft;
			if (self.FlippedVertically)
				return self.TilesetTile.TiledMap.TileWidth - self.TilesetTile.SlopeTopRight;

			return self.TilesetTile.SlopeTopRight;
		}


		/// <summary>
		/// calculates the slope based on the slope top left/right
		/// </summary>
		/// <returns>The slope.</returns>
		/// <param name="self">Self.</param>
		/// <param name="tileSize">Tile width.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetSlope(this TiledTile self)
		{
			var tileSize = self.TilesetTile.TiledMap.TileWidth;

			// flip our slopes sign if the tile is flipped horizontally or vertically
			if (self.FlippedHorizonally)
				tileSize *= -1;

			if (self.FlippedVertically)
				tileSize *= -1;

			// rise over run
			return ((float) self.TilesetTile.SlopeTopRight - (float) self.TilesetTile.SlopeTopLeft) / (float) tileSize;
		}


		/// <summary>
		/// returns the slope position on the left side of the tile. b in the y = mx + b equation
		/// </summary>
		/// <returns>The slope offset.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetSlopeOffset(this TiledTile self)
		{
			return (float) self.GetSlopeTopLeft();
		}


		/// <summary>
		/// returns the edge on the side that has the tallest side
		/// </summary>
		/// <returns>The bigest slope edge.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Edge GetHighestSlopeEdge(this TiledTile self)
		{
			// left and right already have flipping taken into account. Also remember a lower value means a taller slope since the slope values
			// are in pixels from the top!
			var left = self.GetSlopeTopLeft();
			var right = self.GetSlopeTopRight();
			return right > left ? Edge.Left : Edge.Right;
		}


		/// <summary>
		/// returns the nearest edge to worldPosition
		/// </summary>
		/// <returns>The nearest edge.</returns>
		/// <param name="self">Self.</param>
		/// <param name="worldPosition">World position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Edge GetNearestEdge(this TiledTile self, int worldPosition)
		{
			var tileWidth = self.TilesetTile.TiledMap.TileWidth;
			var tileMiddleWorldPosition = self.X * tileWidth + tileWidth / 2;
			return worldPosition < tileMiddleWorldPosition ? Edge.Left : Edge.Right;
		}
	}
}