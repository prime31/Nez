using System;
using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class TiledSprite : Sprite
	{
		public new int width
		{
			get { return _sourceRect.Width; }
			set { _sourceRect.Width = value; }
		}

		public new int height
		{
			get { return _sourceRect.Height; }
			set { _sourceRect.Height = value; }
		}

		/// <summary>
		/// we keep a copy of the sourceRect so that we dont change the Subtexture in case it is used elsewhere
		/// </summary>
		Rectangle _sourceRect;


		public TiledSprite( Subtexture subtexture ) : base( subtexture )
		{
			material = new Material();

			// choose the best fit wrap type based on the defaultSamplerState
			if( Core.defaultSamplerState.Filter == TextureFilter.Point )
				material.samplerState = SamplerState.PointWrap;
			else
				material.samplerState = SamplerState.LinearWrap;
			_sourceRect = subtexture.sourceRect;
		}


		public TiledSprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( subtexture, entity.transform.position + _localOffset, _sourceRect, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, _layerDepth );
		}

	}
}

