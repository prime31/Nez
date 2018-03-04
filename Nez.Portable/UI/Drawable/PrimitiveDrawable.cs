using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class PrimitiveDrawable : IDrawable
	{
		#region IDrawable implementation

		public float leftWidth { get; set; }
		public float rightWidth { get; set; }
		public float topHeight { get; set; }
		public float bottomHeight { get; set; }
		public float minWidth { get; set; }
		public float minHeight { get; set; }


		public void setPadding( float top, float bottom, float left, float right )
		{
			topHeight = top;
			bottomHeight = bottom;
			leftWidth = left;
			rightWidth = right;
		}

		#endregion

		public Color? color;
		public bool useFilledRect = true;


		public PrimitiveDrawable( Color? color = null )
		{
			this.color = color;
		}


		public PrimitiveDrawable( Color color, float horizontalPadding ) : this( color )
		{
			leftWidth = rightWidth = horizontalPadding;
		}


		public PrimitiveDrawable( Color color, float horizontalPadding, float verticalPadding ) : this( color )
		{
			leftWidth = rightWidth = horizontalPadding;
			topHeight = bottomHeight = verticalPadding;
		}


		public PrimitiveDrawable( float minWidth, float minHeight, Color? color = null ) : this( color )
		{
			this.minWidth = minWidth;
			this.minHeight = minHeight;
		}


		public PrimitiveDrawable( float minSize ) : this( minSize, minSize )
		{}


		public PrimitiveDrawable( float minSize, Color color ) : this( minSize, minSize, color )
		{}


		public virtual void draw( Graphics graphics, float x, float y, float width, float height, Color color )
		{
			var col = this.color.HasValue ? this.color.Value : color;
			if( color.A != 255 )
				col *= ( color.A / 255f );
			if (col.A != 255)
				col *= ( col.A / 255f );

			if( useFilledRect )
				graphics.batcher.drawRect( x, y, width, height, col );
			else
				graphics.batcher.drawHollowRect( x, y, width, height, col );
		}
	}
}

