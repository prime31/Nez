using System;


namespace Nez
{
	/// <summary>
	/// Renderer that renders using its own Camera which doesnt move.
	/// </summary>
	public class ScreenSpaceRenderer : Renderer
	{
		public int[] renderLayers;


		public ScreenSpaceRenderer( int renderOrder, params int[] renderLayers ) : base( renderOrder, null )
		{
			Array.Sort( renderLayers );
			this.renderLayers = renderLayers;
			camera = new Camera();
		}


		public override void render( Scene scene )
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

			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}

	}
}

