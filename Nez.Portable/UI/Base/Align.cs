using System;


namespace Nez.UI
{
	[Flags]
	public enum Align
	{
		Center = 1 << 0,
		Top = 1 << 1,
		Bottom = 1 << 2,
		Left = 1 << 3,
		Right = 1 << 4,

		TopLeft = Top | Left,
		TopRight = Top | Right,
		BottomLeft = Bottom | Left,
		BottomRight = Bottom | Right
	}


	/// <summary>
	/// used internally so that alignment can be stored as an int and can have an unlimited number of options by just setting it outside
	/// the bounds of the flags
	/// </summary>
	internal class AlignInternal
	{
		public const int Center = 1 << 0;
		public const int Top = 1 << 1;
		public const int Bottom = 1 << 2;
		public const int Left = 1 << 3;
		public const int Right = 1 << 4;

		public const int TopLeft = Top | Left;
		public const int TopRight = Top | Right;
		public const int BottomLeft = Bottom | Left;
		public const int BottomRight = Bottom | Right;
	}
}