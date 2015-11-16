using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and Entity.debugRender.
	/// A simple Renderer could just start the Graphics.defaultGraphics spriteBatch or it could create its own local Graphics instance
	/// if it needs it for some kind of custom rendering.
	/// </summary>
	public abstract class Renderer
	{
		static public Comparison<Renderer> compareRenderOrder = ( a, b ) => { return Math.Sign( b.renderOrder - a.renderOrder ); };

		/// <summary>
		/// specifies the order in which the Renderers will be called by the scene
		/// </summary>
		public readonly int renderOrder = 0;

		abstract public void render( Scene scene );


		public virtual void debugRender( Scene scene )
		{
			Graphics.defaultGraphics.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, scene.camera.transformMatrix );

			foreach( var entity in scene.entities )
			{
				if( entity.enabled )
					entity.debugRender( Graphics.defaultGraphics );
			}

			Graphics.defaultGraphics.spriteBatch.End();
		}
	
	}
}

