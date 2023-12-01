using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteSliceKey
	{
		public readonly AsepriteSlice Slice;
		public readonly int FrameIndex;
		public readonly Rectangle Bounds;
		public readonly Rectangle? CenterBounds;
		public Point? Pivot;

		internal AsepriteSliceKey(AsepriteSlice slice, int frameIndex, Rectangle bounds, Rectangle? centerBounds, Point? pivot)
		{
			Slice = slice;
			slice.Keys.Add(this);
			Bounds = bounds;
			CenterBounds = centerBounds;
			Pivot = pivot;
		}
	}
}