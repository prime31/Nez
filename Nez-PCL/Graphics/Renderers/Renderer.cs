using System;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and Entity.debugRender.
	/// A simple Renderer could just start the Graphics.instanceGraphics spriteBatch or it could create its own local Graphics instance
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
		static internal Comparison<Renderer> compareRenderOrder = ( a, b ) => { return Math.Sign( a.renderOrder - b.renderOrder ); };

		/// <summary>
		/// SpriteSortMode used by the SpriteBatch. Use BackToFront when drawing transparent sprites and FrontToBack when drawing opaque sprites
		/// if you want the depth value to be taken into account.
		/// </summary>
		public SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;

		/// <summary>
		/// BlendState used by the SpriteBatch
		/// </summary>
		public BlendState blendState = BlendState.AlphaBlend;

		/// <summary>
		/// SamplerState used by the SpriteBatch
		/// </summary>
		public SamplerState samplerState = SamplerState.PointClamp;

		/// <summary>
		/// DepthStencilState used by the SpriteBatch
		/// </summary>
		public DepthStencilState depthStencilState = DepthStencilState.None;

		/// <summary>
		/// RasterizerState used by the SpriteBatch
		/// </summary>
		public RasterizerState rasterizerState = RasterizerState.CullNone;

		/// <summary>
		/// Effect used by the SpriteBatch
		/// </summary>
		public Effect effect;

		/// <summary>
		/// the Camera this renderer uses for rendering (really its transformMatrix and bounds for culling). This is a convenience field and isnt
		/// required. Renderer subclasses can pick the camera used when calling beginRender.
		/// </summary>
		public Camera camera;

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

		/// <summary>
		/// if true and a renderTexture is present the RenderTarget will not be reset to null. This is useful when you have multiple
		/// renderers that should all be rendering into the same RenderTarget.
		/// </summary>
		public bool clearRenderTargetAfterRender = true;

		/// <summary>
		/// flag for this renderer that decides if it should debug render or not. The render method receives a bool (debugRenderEnabled)
		/// letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide if it
		/// should debug render or not.
		/// </summary>
		public bool shouldDebugRender = true;


		public Renderer( Camera camera, int renderOrder )
		{
			this.camera = camera;
			this.renderOrder = renderOrder;
		}


		/// <summary>
		/// if a RenderTexture is used this will set it up. The SpriteBatch is also started. The passed in Camera will be used to set the ViewPort
		/// (if a ViewportAdapter is present) and for the SpriteBatch transform Matrix.
		/// </summary>
		/// <param name="cam">Cam.</param>
		protected virtual void beginRender( Camera cam )
		{
			// if we have a renderTexture render into it
			if( renderTexture != null )
			{
				Core.graphicsDevice.SetRenderTarget( renderTexture );
				Core.graphicsDevice.Clear( renderTextureClearColor );
			}

			// Sets the current camera viewport if the camera has one
			if( cam.viewportAdapter != null )
			{
				Core.graphicsDevice.Viewport = cam.viewportAdapter.viewport;
			}

			// MonoGame resets the Viewport to the RT size without asking so we have to let the Camera know to update itself
			cam.forceMatrixUpdate();

			Graphics.instance.spriteBatch.Begin( spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, cam.transformMatrix );
		}


		abstract public void render( Scene scene, bool debugRenderEnabled );


		/// <summary>
		/// force flushes the SpriteBatch by calling End then Begin on it.
		/// </summary>
		protected void flushSpriteBatch( Camera cam )
		{
			Graphics.instance.spriteBatch.End();
			Graphics.instance.spriteBatch.Begin( spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, cam.transformMatrix );
		}


		/// <summary>
		/// ends the SpriteBatch and clears the RenderTarget if it had a RenderTexture
		/// </summary>
		protected virtual void endRender()
		{
			Graphics.instance.spriteBatch.End();

			// clear the RenderTarget so that we render to the screen if we were using a RenderTexture
			if( renderTexture != null && clearRenderTargetAfterRender )
				Core.graphicsDevice.SetRenderTarget( null );
		}


		/// <summary>
		/// default debugRender method just loops through all entities and calls entity.debugRender
		/// </summary>
		/// <param name="scene">Scene.</param>
		protected virtual void debugRender( Scene scene, Camera cam )
		{
			Graphics.instance.spriteBatch.End();
			Graphics.instance.spriteBatch.Begin( transformMatrix: cam.transformMatrix );

			for( var i = 0; i < scene.entities.Count; i++ )
			{
				var entity = scene.entities[i];
				if( entity.enabled )
					entity.debugRender( Graphics.instance );
			}
		}


		/// <summary>
		/// called when a scene is ended. use this for cleanup.
		/// </summary>
		public virtual void unload()
		{
			if( renderTexture != null )
			{
				renderTexture.unload();
				renderTexture = null;
			}
		}
	
	}
}

