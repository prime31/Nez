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


		public RenderLayerRenderer( int renderLayer, Camera camera = null, int renderOrder = 0 ) : base( renderOrder, camera )
		{
			this.renderLayer = renderLayer;
		}


		public override void render( Scene scene, bool debugRenderEnabled )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			var renderables = scene.renderableComponents.componentsWithRenderLayer( renderLayer );
			for( var i = 0; i < renderables.Count; i++ )
			{
				var renderable = renderables[i];
				if( renderable.enabled )
					renderAfterStateCheck( renderable, cam );
			}

			if( shouldDebugRender && debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}

	}
}

