using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public struct DebugRectangleF
	{
		public RectangleF Rect;
		public Color Color;


		public DebugRectangleF(float x, float y, float width, float height, Color color)
		{
			Rect = new RectangleF(x, y, width, height);
			Color = color;
		}


		public void Set(float x, float y, float width, float height, Color color)
		{
			Rect.X = x;
			Rect.Y = y;
			Rect.Width = width;
			Rect.Height = height;
			Color = color;
		}
	}
}