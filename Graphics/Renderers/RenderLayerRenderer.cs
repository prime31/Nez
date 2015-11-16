using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Renderer that only renders a single renderLayer. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with a RenderLayerRenderer
	/// </summary>
	public class RenderLayerRenderer : Renderer
	{
		public BlendState blendState;
		public SamplerState samplerState;
		public Effect effect;
		public int renderLayer;


		public RenderLayerRenderer( int renderLayer )
		{
			blendState = BlendState.AlphaBlend;
			samplerState = SamplerState.PointClamp;
			this.renderLayer = renderLayer;
		}


		public override void render( Scene scene )
		{
			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.None, RasterizerState.CullNone, effect, scene.camera.transformMatrix );

			foreach( var renderable in scene.renderableComponents.componentsWithRenderLayer( renderLayer ) )
			{
				if( renderable.enabled )
					renderable.render( Graphics.defaultGraphics );
			}

			Graphics.defaultGraphics.spriteBatch.End();
		}

	}
}

