using System;
using System.Collections.Generic;
using System.Text;


namespace Nez.BitmapFonts
{
	/// <summary>
	/// Represents padding or margin information associated with an element.
	/// </summary>
	public struct Padding
	{
		public int Bottom;
		public int Left;
		public int Right;
		public int Top;

		public Padding(int left, int top, int right, int bottom)
		{
			this.Top = top;
			this.Left = left;
			this.Bottom = bottom;
			this.Right = right;
		}

		public override string ToString() => string.Format("{0}, {1}, {2}, {3}", Left, Top, Right, Bottom);
	}
}