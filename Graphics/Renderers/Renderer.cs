using System;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and Entity.debugRender.
	/// A simple Renderer could just start the Graphics.defaultGraphics spriteBatch or it could create its own local Graphics instance
	/// if it needs it for some kind of custom rendering.
	/// 
	/// Note that it is a best practice to ensure all Renderers that render to a RenderTexture have lower renderOrders to avoid issues
	/// with clearing the back buffer (http://gamedev.stackexchange.com/questions/90396/monogame-setrendertarget-is-wiping-the-backbuffer).
	/// Giving them a negative renderOrder is a good strategy to deal with this.
	/// </summary>
	public abstract class Renderer
	{
		/// <summary>
		/// Comparison used to sort renderers
		/// </summary>
		static public Comparison<Renderer> compareRenderOrder = ( a, b ) => { return Math.Sign( a.renderOrder - b.renderOrder ); };

		/// <summary>
		/// specifies the order in which the Renderers will be called by the scene
		/// </summary>
		public readonly int renderOrder = 0;

		/// <summary>
		/// if renderTexture is not null this renderer will render into the RenderTexture instead of to the screen
		/// </summary>
		public RenderTexture renderTexture;

		/// <summary>
		/// if renderTexture is not null this Color will be used to clear the screen
		/// </summary>
		public Color renderTextureClearColor = Color.Transparent;


		public Renderer( int renderOrder )
		{
			this.renderOrder = renderOrder;
		}


		abstract public void render( Scene scene );


		/// <summary>
		/// default debugRender method just loops through all entities and calls entity.debugRender
		/// </summary>
		/// <param name="scene">Scene.</param>
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


		/// <summary>
		/// called when a scene is ended. use this for cleanup.
		/// </summary>
		public virtual void onSceneEnd()
		{
			if( renderTexture != null )
			{
				renderTexture.unload();
				renderTexture = null;
			}
		}
	
	}
}

