using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class DefaultRenderer : Renderer
	{
		public BlendState blendState;
		public SamplerState samplerState;
		public Effect effect;


		public DefaultRenderer( int renderOrder = 0 ) : base( renderOrder )
		{
			blendState = BlendState.AlphaBlend;
			samplerState = SamplerState.PointClamp;
		}


		public override void render( Scene scene )
		{
			// if we have a renderTexture render into it
			if( renderTexture != null )
			{
				Graphics.defaultGraphics.graphicsDevice.SetRenderTarget( renderTexture );
				Graphics.defaultGraphics.graphicsDevice.Clear( renderTextureClearColor );
			}

			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.None, RasterizerState.CullNone, effect, scene.camera.transformMatrix );
			foreach( var renderable in scene.renderableComponents )
			{
				if( renderable.enabled )
					renderable.render( Graphics.defaultGraphics );
			}
			Graphics.defaultGraphics.spriteBatch.End();

			// clear the RenderTarget so that we render to the screen
			Graphics.defaultGraphics.graphicsDevice.SetRenderTarget( null );
		}

	}
}
