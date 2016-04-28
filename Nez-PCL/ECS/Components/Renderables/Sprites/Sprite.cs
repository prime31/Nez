using System;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Sprites
{
	public class Sprite : RenderableComponent
	{
		public override float width
		{
			get { return subtexture.sourceRect.Width; }
		}

		public override float height
		{
			get { return subtexture.sourceRect.Height; }
		}

		public Subtexture subtexture;


		public Sprite( Subtexture subtexture )
		{
			this.subtexture = subtexture;
			originNormalized = new Vector2( 0.5f, 0.5f );
		}


		public Sprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( subtexture, entity.transform.position + localOffset, subtexture.sourceRect, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, _layerDepth );
		}

	}
}

