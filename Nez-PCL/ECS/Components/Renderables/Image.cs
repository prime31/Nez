using System;
using Nez.TextureAtlases;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class Image : RenderableComponent
	{
		public Subtexture subtexture;


		public override float width
		{
			get { return subtexture.sourceRect.Width; }
		}

		public override float height
		{
			get { return subtexture.sourceRect.Height; }
		}


		public Image()
		{}


		public Image( Subtexture texture )
		{
			this.subtexture = texture;
		}


		public Image( Texture2D tex2d ) : this( new Subtexture( tex2d ) )
		{}


		public override void render( Graphics graphics, Camera camera )
		{
			if( camera.bounds.Intersects( bounds ) )
				graphics.spriteBatch.Draw( subtexture, renderPosition, subtexture.sourceRect, color, rotation, origin, scale * zoom, spriteEffects, layerDepth );
		}
	}
}

