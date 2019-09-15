using System.Runtime.CompilerServices;

namespace Nez.Tiled
{
	public static class TmxLayerTileExt
	{
		/// <summary>
		/// passthrough to TilesetTile
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsSlope(this TmxLayerTile self) => self.TilesetTile != null && self.TilesetTile.IsSlope;

		/// <summary>
		/// passthrough to TilesetTile
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneWayPlatform(this TmxLayerTile self) => self.TilesetTile != null && self.TilesetTile.IsOneWayPlatform;


		/// <summary>
		/// returns the slope top left taking flipping into account. Exceptions if the tile is not a slope.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSlopeTopLeft(this TmxLayerTile self)
		{
			if (self.HorizontalFlip && self.VerticalFlip)
				return self.TilesetTile.SlopeTopLeft;
			if (self.HorizontalFlip)
				return self.TilesetTile.SlopeTopRight;
			if (self.VerticalFlip)
				return self.Tileset.Map.TileWidth - self.TilesetTile.SlopeTopLeft;

			return self.TilesetTile.SlopeTopLeft;
		}

		/// <summary>
		/// returns the slope top right taking flipping into account. Exceptions if the tile is not a slope.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSlopeTopRight(this TmxLayerTile self)
		{
			if (self.HorizontalFlip && self.VerticalFlip)
				return self.TilesetTile.SlopeTopRight;
			if (self.HorizontalFlip)
				return self.TilesetTile.SlopeTopLeft;
			if (self.VerticalFlip)
				return self.Tileset.Map.TileWidth - self.TilesetTile.SlopeTopRight;

			return self.TilesetTile.SlopeTopRight;
		}

		/// <summary>
		/// calculates the slope based on the slope top left/right
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetSlope(this TmxLayerTile self)
		{
			var tileSize = self.Tileset.Map.TileWidth;

			// flip our slopes sign if the tile is flipped horizontally or vertically
			if (self.HorizontalFlip)
				tileSize *= -1;

			if (self.VerticalFlip)
				tileSize *= -1;

			// rise over run
			return (self.TilesetTile.SlopeTopRight - (float)self.TilesetTile.SlopeTopLeft) / tileSize;
		}

		/// <summary>
		/// returns the slope position on the left side of the tile. b in the y = mx + b equation
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetSlopeOffset(this TmxLayerTile self) => self.GetSlopeTopLeft();

		/// <summary>
		/// returns the edge on the side that has the tallest side
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Edge GetHighestSlopeEdge(this TmxLayerTile self)
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Edge GetNearestEdge(this TmxLayerTile self, int worldPosition)
		{
			var tileWidth = self.Tileset.Map.TileWidth;
			var tileMiddleWorldPosition = self.X * tileWidth + tileWidth / 2;
			return worldPosition < tileMiddleWorldPosition ? Edge.Left : Edge.Right;
		}
	}
}
