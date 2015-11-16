using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Renderer that only renders all but one renderLayer. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with a RenderLayerRenderer. Note that UI would most likely want to be rendered in screen space so the camera matrix shouldn't be passed to
	/// spriteBatch.Begin.
	/// </summary>
	public class RenderLayerExcludeRenderer : Renderer
	{
		public BlendState blendState;
		public SamplerState samplerState;
		public Effect effect;
		public int excludeRenderLayer;


		public RenderLayerExcludeRenderer( int excludeRenderLayer, int renderOrder = 0 ) : base( renderOrder )
		{
			blendState = BlendState.AlphaBlend;
			samplerState = SamplerState.PointClamp;
			this.excludeRenderLayer = excludeRenderLayer;
		}


		public override void render( Scene scene )
		{
			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.None, RasterizerState.CullNone, effect, scene.camera.transformMatrix );

			foreach( var renderable in scene.renderableComponents )
			{
				if( renderable.renderLayer != excludeRenderLayer && renderable.enabled )
					renderable.render( Graphics.defaultGraphics );
			}

			Graphics.defaultGraphics.spriteBatch.End();
		}

	}
}

