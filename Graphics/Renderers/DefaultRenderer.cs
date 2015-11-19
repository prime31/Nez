using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class DefaultRenderer : Renderer
	{
		public DefaultRenderer( Camera camera = null, int renderOrder = 0 ) : base( camera, renderOrder )
		{}


		public override void render( Scene scene, bool shouldDebugRender )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			foreach( var renderable in scene.renderableComponents )
			{
				if( renderable.enabled )
					renderable.render( Graphics.instance, cam );
			}

			if( shouldDebugRender )
				debugRender( scene );

			endRender();
		}

	}
}
