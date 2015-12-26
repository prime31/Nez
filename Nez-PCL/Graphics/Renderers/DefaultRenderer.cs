using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class DefaultRenderer : Renderer
	{
		public DefaultRenderer( Camera camera = null, int renderOrder = 0 ) : base( camera, renderOrder )
		{}


		public override void render( Scene scene, bool debugRenderEnabled )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			for( var i = 0; i < scene.renderableComponents.Count; i++ )
			{
				var renderable = scene.renderableComponents[i];
				if( renderable.enabled )
					renderable.render( Graphics.instance, cam );
			}
				
			if( shouldDebugRender && debugRenderEnabled )
				debugRender( scene );

			endRender();
		}

	}
}
