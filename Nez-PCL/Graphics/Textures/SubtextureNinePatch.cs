using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Textures
{
	public class SubtextureNinePatch : Subtexture
	{
		public readonly int left;
		public readonly int right;
		public readonly int top;
		public readonly int bottom;
		public Rectangle[] ninePatchRects = new Rectangle[9];


		public SubtextureNinePatch( Texture2D texture, Rectangle sourceRect, int left, int right, int top, int bottom ) : base( texture, sourceRect )
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;

			generateNinePatchRects( sourceRect, ninePatchRects, left, right, top, bottom );
		}


		public SubtextureNinePatch( Texture2D texture, int left, int right, int top, int bottom ) : this( texture, texture.Bounds, left, right, top, bottom )
		{}

	}
}

