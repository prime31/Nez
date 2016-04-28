using System;
using Nez.Sprites;
using Microsoft.Xna.Framework;


namespace Nez.Sprites
{
	/// <summary>
	/// this component will draw the same frame of of spriteToMime every frame. The only difference in rendering is that SpriteMime uses its own
	/// localPosition and color. This allows you to use it for the purpose of shadows (by offsetting via localPosition) or silhouettes (with a
	/// Material that has a stencil read).
	/// </summary>
	public class SpriteMime : RenderableComponent
	{
		public override float width { get { return _spriteToMime.width; } }

		public override float height { get { return _spriteToMime.height; } }


		Sprite _spriteToMime;


		public SpriteMime( Sprite spriteToMime )
		{
			_spriteToMime = spriteToMime;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( _spriteToMime.subtexture, entity.transform.position + _localOffset, _spriteToMime.subtexture.sourceRect, color, entity.transform.rotation, _spriteToMime.origin, entity.transform.scale, _spriteToMime.spriteEffects, _layerDepth );
		}
	}
}

