using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Renderer that only renders the specified renderLayers. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with other RenderLayerRenderers rendering different renderLayers.
	/// </summary>
	public class RenderLayerRenderer : Renderer
	{
		/// <summary>
		/// the renderLayers this Renderer will render
		/// </summary>
		public int[] renderLayers;


		public RenderLayerRenderer( int renderOrder, params int[] renderLayers ) : base( renderOrder, null )
		{
			Array.Sort( renderLayers );
			Array.Reverse( renderLayers );
			this.renderLayers = renderLayers;
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
					if( renderable.enabled && renderable.isVisibleFromCamera( cam ) )
						renderAfterStateCheck( renderable, cam );
				}
			}

			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}


		protected override void debugRender( Scene scene, Camera cam )
		{
			for( var i = 0; i < renderLayers.Length; i++ )
			{
				var renderables = scene.renderableComponents.componentsWithRenderLayer( renderLayers[i] );
				for( var j = 0; j < renderables.Count; j++ )
				{
					var renderable = renderables[j];
					if( renderable.enabled && renderable.isVisibleFromCamera( cam ) )
						renderable.debugRender( Graphics.instance );
				}
			}
		}

	}
}

