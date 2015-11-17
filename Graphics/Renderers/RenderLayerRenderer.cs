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
		public int renderLayer;


		public RenderLayerRenderer( int renderLayer, Camera camera = null, int renderOrder = 0 ) : base( camera, renderOrder )
		{
			this.renderLayer = renderLayer;
		}


		public override void render( Scene scene )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			foreach( var renderable in scene.renderableComponents.componentsWithRenderLayer( renderLayer ) )
			{
				if( renderable.enabled )
					renderable.render( Graphics.defaultGraphics, cam );
			}

			endRender();
		}

	}
}

