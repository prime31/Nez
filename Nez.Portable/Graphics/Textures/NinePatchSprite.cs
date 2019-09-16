using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Textures
{
	public class NinePatchSprite : Sprite
	{
		public int Left;
		public int Right;
		public int Top;
		public int Bottom;
		public Rectangle[] NinePatchRects = new Rectangle[9];

		/// <summary>
		/// used to indicate if this nine patch has additional padding information
		/// </summary>
		public bool HasPadding;

		public int PadLeft;
		public int PadRight;
		public int PadTop;
		public int PadBottom;


		public NinePatchSprite(Texture2D texture, Rectangle sourceRect, int left, int right, int top, int bottom) :
			base(texture, sourceRect)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;

			GenerateNinePatchRects(sourceRect, NinePatchRects, left, right, top, bottom);
		}


		public NinePatchSprite(Texture2D texture, int left, int right, int top, int bottom) : this(texture,
			texture.Bounds, left, right, top, bottom)
		{ }


		public NinePatchSprite(Sprite sprite, int left, int right, int top, int bottom) : this(sprite,
			sprite.SourceRect, left, right, top, bottom)
		{ }
	}
}