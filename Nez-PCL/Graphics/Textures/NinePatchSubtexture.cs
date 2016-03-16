using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Textures
{
	public class NinePatchSubtexture : Subtexture
	{
		public int left;
		public int right;
		public int top;
		public int bottom;
		public Rectangle[] ninePatchRects = new Rectangle[9];


		public NinePatchSubtexture( Texture2D texture, Rectangle sourceRect, int left, int right, int top, int bottom ) : base( texture, sourceRect )
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;

			generateNinePatchRects( sourceRect, ninePatchRects, left, right, top, bottom );
		}


		public NinePatchSubtexture( Texture2D texture, int left, int right, int top, int bottom ) : this( texture, texture.Bounds, left, right, top, bottom )
		{}


		public NinePatchSubtexture( Subtexture subtexture, int left, int right, int top, int bottom ) : this( subtexture, subtexture.sourceRect, left, right, top, bottom )
		{}

	}
}

