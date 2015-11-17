using System;
using Nez.TextureAtlases;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Sprite<TEnum> : RenderableComponent where TEnum : struct, IConvertible, IComparable, IFormattable
	{
		public override float width
		{
			get { return _subtexture.sourceRect.Width; }
		}

		public override float height
		{
			get { return _subtexture.sourceRect.Height; }
		}

		Subtexture _subtexture;
		public Subtexture subtexture
		{
			get { return _subtexture; }
			set
			{
				// preserve the origin if the texture size changes
				var oldOriginNormalized = originNormalized;
				_subtexture = value;
				originNormalized = oldOriginNormalized;
			}
		}


		public Sprite( Subtexture subtexture ) : base()
		{
			_subtexture = subtexture;
			originNormalized = new Vector2( 0.5f, 0.5f );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( camera.bounds.Intersects( bounds ) )
				graphics.spriteBatch.Draw( _subtexture, renderPosition, _subtexture.sourceRect, color, rotation, origin, scale * zoom, spriteEffects, layerDepth );
		}

	}
}

