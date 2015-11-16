using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class DefaultRenderer : Renderer
	{
		public BlendState blendState;
		public SamplerState samplerState;
		public Effect effect;


		public DefaultRenderer()
		{
			blendState = BlendState.AlphaBlend;
			samplerState = SamplerState.PointClamp;
		}


		public override void render( Scene scene )
		{
			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.None, RasterizerState.CullNone, effect, scene.camera.transformMatrix );

			foreach( var renderable in scene.renderableComponents )
			{
				if( renderable.enabled )
					renderable.render( Graphics.defaultGraphics );
			}

			Graphics.defaultGraphics.spriteBatch.End();
		}

	}
}
