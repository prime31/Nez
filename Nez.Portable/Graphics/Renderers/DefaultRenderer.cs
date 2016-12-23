namespace Nez
{
	public class DefaultRenderer : Renderer
	{
		/// <summary>
		/// renders all renderLayers
		/// </summary>
		/// <param name="renderOrder">Render order.</param>
		/// <param name="camera">Camera.</param>
		public DefaultRenderer( int renderOrder = 0, Camera camera = null ) : base( renderOrder, camera )
		{}


		public override void render( Scene scene )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			for( var i = 0; i < scene.renderableComponents.count; i++ )
			{
				var renderable = scene.renderableComponents[i];
				if( renderable.enabled && renderable.isVisibleFromCamera( cam ) )
					renderAfterStateCheck( renderable, cam );
			}
				
			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}
	}
}
