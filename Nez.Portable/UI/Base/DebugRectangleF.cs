using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public struct DebugRectangleF
	{
		public RectangleF rect;
		public Color color;


		public DebugRectangleF( float x, float y, float width, float height, Color color )
		{
			rect = new RectangleF( x, y, width, height );
			this.color = color;
		}


		public void set( float x, float y, float width, float height, Color color )
		{
			rect.x = x;
			rect.y = y;
			rect.width = width;
			rect.height = height;
			this.color = color;
		}
	}
}

