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
		public int[] renderLayers;


		public RenderLayerRenderer( int renderOrder, params int[] renderLayers ) : base( renderOrder, null )
		{
			this.renderLayers = renderLayers;
		}


		public override void render( Scene scene, bool debugRenderEnabled )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			for( var i = 0; i < renderLayers.Length; i++ )
			{
				var renderables = scene.renderableComponents.componentsWithRenderLayer( renderLayers[i] );
				for( var j = 0; j < renderables.Count; j++ )
				{
					var renderable = renderables[j];
					if( renderable.enabled )
						renderAfterStateCheck( renderable, cam );
				}
			}

			if( shouldDebugRender && debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}

	}
}

