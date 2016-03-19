using System;


namespace Nez.UI
{
	[Flags]
	public enum Align
	{
		center = 1 << 0,
		top = 1 << 1,
		bottom = 1 << 2,
		left = 1 << 3,
		right = 1 << 4,

		topLeft = top | left,
		topRight = top | right,
		bottomLeft = bottom | left,
		bottomRight = bottom | right
	}


	/// <summary>
	/// used internally so that alignment can be stored as an int and can have an unlimited numver of options by just setting it outside
	/// the bounds of the flags
	/// </summary>
	internal class AlignInternal
	{
		public const int center = 1 << 0;
		public const int top = 1 << 1;
		public const int bottom = 1 << 2;
		public const int left = 1 << 3;
		public const int right = 1 << 4;

		public const int topLeft = top | left;
		public const int topRight = top | right;
		public const int bottomLeft = bottom | left;
		public const int bottomRight = bottom | right;
	}
}

