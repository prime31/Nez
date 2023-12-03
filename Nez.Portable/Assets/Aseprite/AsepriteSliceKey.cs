using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a key for a slice in Aseprite. 
	/// </summary>
	/// <remarks>
	/// A slice key can be thought of like frame keys in animations.  They contain the properties of the slice on a
	/// specific frame.
	/// </remarks>
	public sealed class AsepriteSliceKey
	{
		/// <summary>
		/// The slice this key is for.
		/// </summary>
		public readonly AsepriteSlice Slice;

		/// <summary>
		/// The index of the frame in the Aseprite file that the properties of this key are applied to the slice on.
		/// </summary>
		public readonly int FrameIndex;

		/// <summary>
		/// A rectangle value that represents the location and size of the slice during this key.
		/// </summary>
		public readonly Rectangle Bounds;

		/// <summary>
		/// A rectangle value that represents the location and size of the center area of the slice during this key, if
		/// the slice is a nine patch slice; otherwise, this will be null.
		/// </summary>
		public readonly Rectangle? CenterBounds;

		/// <summary>
		/// A point value that indicates the x- and y-coordinate location relative to the bounds of the slice during
		/// this key tha the slice should pivot from, if the slice was marked to have a pivot point in Aseprite; 
		/// otherwise, this will be null.
		/// </summary>
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