using System;
using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Sprites
{
	public class ScrollingSprite : Sprite
	{
		public float scrollSpeedX = 100f;
		public float scrollSpeedY = 100f;

		/// <summary>
		/// we keep a copy of the sourceRect so that we dont change the Subtexture in case it is used elsewhere
		/// </summary>
		Rectangle _sourceRect;


		public ScrollingSprite( Subtexture subtexture ) : base( subtexture )
		{
			renderState = new RenderState();
			renderState.samplerState = SamplerState.PointWrap;
			_sourceRect = subtexture.sourceRect;
		}


		public ScrollingSprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}


		public override void update()
		{
			_sourceRect.X += (int)( scrollSpeedX * Time.deltaTime );
			_sourceRect.Y += (int)( scrollSpeedY * Time.deltaTime );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( isVisibleFromCamera( camera ) )
				graphics.spriteBatch.Draw( subtexture, renderPosition, _sourceRect, color, rotation, origin, scale, spriteEffects, _layerDepth );
		}

	}
}

