using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Textures
{
	public class NinePatchSubtexture : Subtexture
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


		public NinePatchSubtexture(Texture2D texture, Rectangle sourceRect, int left, int right, int top, int bottom) :
			base(texture, sourceRect)
		{
			this.Left = left;
			this.Right = right;
			this.Top = top;
			this.Bottom = bottom;

			GenerateNinePatchRects(sourceRect, NinePatchRects, left, right, top, bottom);
		}


		public NinePatchSubtexture(Texture2D texture, int left, int right, int top, int bottom) : this(texture,
			texture.Bounds, left, right, top, bottom)
		{
		}


		public NinePatchSubtexture(Subtexture subtexture, int left, int right, int top, int bottom) : this(subtexture,
			subtexture.SourceRect, left, right, top, bottom)
		{
		}
	}
}