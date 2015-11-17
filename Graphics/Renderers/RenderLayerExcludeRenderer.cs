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
		public int excludeRenderLayer;


		public RenderLayerExcludeRenderer( int excludeRenderLayer, Camera camera = null, int renderOrder = 0 ) : base( camera, renderOrder )
		{
			this.excludeRenderLayer = excludeRenderLayer;
		}


		public override void render( Scene scene )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			foreach( var renderable in scene.renderableComponents )
			{
				if( renderable.renderLayer != excludeRenderLayer && renderable.enabled )
					renderable.render( Graphics.defaultGraphics, cam );
			}

			endRender();
		}

	}
}

