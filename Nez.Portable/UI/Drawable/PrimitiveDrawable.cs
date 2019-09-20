using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class PrimitiveDrawable : IDrawable
	{
		#region IDrawable implementation

		public float LeftWidth { get; set; }
		public float RightWidth { get; set; }
		public float TopHeight { get; set; }
		public float BottomHeight { get; set; }
		public float MinWidth { get; set; }
		public float MinHeight { get; set; }


		public void SetPadding(float top, float bottom, float left, float right)
		{
			TopHeight = top;
			BottomHeight = bottom;
			LeftWidth = left;
			RightWidth = right;
		}

		#endregion

		public Color? Color;
		public bool UseFilledRect = true;


		public PrimitiveDrawable(Color? color = null)
		{
			Color = color;
		}


		public PrimitiveDrawable(Color color, float horizontalPadding) : this(color)
		{
			LeftWidth = RightWidth = horizontalPadding;
		}


		public PrimitiveDrawable(Color color, float horizontalPadding, float verticalPadding) : this(color)
		{
			LeftWidth = RightWidth = horizontalPadding;
			TopHeight = BottomHeight = verticalPadding;
		}


		public PrimitiveDrawable(float minWidth, float minHeight, Color? color = null) : this(color)
		{
			MinWidth = minWidth;
			MinHeight = minHeight;
		}


		public PrimitiveDrawable(float minSize) : this(minSize, minSize)
		{
		}


		public PrimitiveDrawable(float minSize, Color color) : this(minSize, minSize, color)
		{
		}


		public virtual void Draw(Batcher batcher, float x, float y, float width, float height, Color color)
		{
			var col = Color.HasValue ? Color.Value : color;
			if (color.A != 255)
				col *= (color.A / 255f);
			if (col.A != 255)
				col *= (col.A / 255f);

			if (UseFilledRect)
				batcher.DrawRect(x, y, width, height, col);
			else
				batcher.DrawHollowRect(x, y, width, height, col);
		}
	}
}