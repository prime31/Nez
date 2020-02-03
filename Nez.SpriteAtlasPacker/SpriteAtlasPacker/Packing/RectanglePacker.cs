using System.Drawing;

namespace Nez.Tools.Atlases
{
	/// <summary>Base class for rectangle packing algorithms</summary>
	/// <remarks>
	///   <para>
	///     By uniting all rectangle packers under this common base class, you can
	///     easily switch between different algorithms to find the most efficient or
	///     performant one for a given job.
	///   </para>
	///   <para>
	///     An almost exhaustive list of packing algorithms can be found here:
	///     http://www.csc.liv.ac.uk/~epa/surveyhtml.html
	///   </para>
	/// </remarks>
	internal abstract class RectanglePacker
	{
		/// <summary>Maximum width the packing area is allowed to have</summary>
		protected int PackingAreaWidth { get; private set; }

		/// <summary>Maximum height the packing area is allowed to have</summary>
		protected int PackingAreaHeight { get; private set; }

		/// <summary>Initializes a new rectangle packer</summary>
		/// <param name="packingAreaWidth">Width of the packing area</param>
		/// <param name="packingAreaHeight">Height of the packing area</param>
		protected RectanglePacker(int packingAreaWidth, int packingAreaHeight)
		{
			PackingAreaWidth = packingAreaWidth;
			PackingAreaHeight = packingAreaHeight;
		}

		/// <summary>Allocates space for a rectangle in the packing area</summary>
		/// <param name="rectangleWidth">Width of the rectangle to allocate</param>
		/// <param name="rectangleHeight">Height of the rectangle to allocate</param>
		/// <returns>The location at which the rectangle has been placed</returns>
		public virtual Point Pack(int rectangleWidth, int rectangleHeight)
		{
			Point point;

			if (!TryPack(rectangleWidth, rectangleHeight, out point))
				throw new OutOfSpaceException("Rectangle does not fit in packing area");

			return point;
		}

		/// <summary>Tries to allocate space for a rectangle in the packing area</summary>
		/// <param name="rectangleWidth">Width of the rectangle to allocate</param>
		/// <param name="rectangleHeight">Height of the rectangle to allocate</param>
		/// <param name="placement">Output parameter receiving the rectangle's placement</param>
		/// <returns>True if space for the rectangle could be allocated</returns>
		public abstract bool TryPack(int rectangleWidth, int rectangleHeight, out Point placement);
	}
}