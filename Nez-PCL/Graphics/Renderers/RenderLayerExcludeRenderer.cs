using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Renderer that only renders all but one renderLayer. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with a RenderLayerRenderer. Note that UI would most likely want to be rendered in screen space so the camera matrix shouldn't be passed to
	/// Batcher.Begin.
	/// </summary>
	public class RenderLayerExcludeRenderer : Renderer
	{
		public int[] excludedRenderLayers;


		public RenderLayerExcludeRenderer( int renderOrder, params int[] excludedRenderLayers ) : base( renderOrder, null )
		{
			this.excludedRenderLayers = excludedRenderLayers;
		}


		public override void render( Scene scene )
		{
			var cam = camera ?? scene.camera;
			beginRender( cam );

			for( var i = 0; i < scene.renderableComponents.Count; i++ )
			{
				var renderable = scene.renderableComponents[i];
				if( !excludedRenderLayers.contains( renderable.renderLayer ) && renderable.enabled && renderable.isVisibleFromCamera( cam ) )
					renderAfterStateCheck( renderable, cam );
			}

			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}


		protected override void debugRender( Scene scene, Camera cam )
		{
			for( var i = 0; i < scene.renderableComponents.Count; i++ )
			{
				var renderable = scene.renderableComponents[i];
				if( !excludedRenderLayers.contains( renderable.renderLayer ) && renderable.enabled && renderable.isVisibleFromCamera( cam ) )
					renderable.debugRender( Graphics.instance );
			}
		}

	}
}

