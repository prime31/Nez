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
        public int bottom;
        public int left;
        public int right;
        public int top;

        public Padding(int left, int top, int right, int bottom)
        {
            this.top = top;
            this.left = left;
            this.bottom = bottom;
            this.right = right;
        }

        public override string ToString() => string.Format("{0}, {1}, {2}, {3}", left, top, right, bottom);
    }
}
