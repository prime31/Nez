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
	/// Note that it is a best practice to ensure all Renderers that render to a RenderTarget have lower renderOrders to avoid issues
	/// with clearing the back buffer (http://gamedev.stackexchange.com/questions/90396/monogame-setrendertarget-is-wiping-the-backbuffer).
	/// Giving them a negative renderOrder is a good strategy to deal with this.
	/// </summary>
	public abstract class Renderer : IComparable<Renderer>
	{
		/// <summary>
		/// SpriteSortMode used by the SpriteBatch. Use BackToFront when drawing transparent sprites and FrontToBack when drawing opaque sprites
		/// if you want the depth value to be taken into account.
		/// </summary>
		public SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;

		/// <summary>
		/// RasterizerState used by the SpriteBatch
		/// </summary>
		public RasterizerState rasterizerState;

		/// <summary>
		/// RenderState used by the SpriteBatch. Any RenderableComponent can override this.
		/// </summary>
		public RenderState renderState = new RenderState();

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
		/// if renderTarget is not null this renderer will render into the RenderTarget instead of to the screen
		/// </summary>
		public RenderTexture renderTexture;

		/// <summary>
		/// if renderTarget is not null this Color will be used to clear the screen
		/// </summary>
		public Color renderTargetClearColor = Color.Transparent;

		/// <summary>
		/// if true and a renderTarget is present the RenderTarget will be reset to null. This is useful when you have multiple
		/// renderers that should all be rendering into the same RenderTarget.
		/// </summary>
		public bool clearRenderTargetAfterRender = true;

		/// <summary>
		/// flag for this renderer that decides if it should debug render or not. The render method receives a bool (debugRenderEnabled)
		/// letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide if it
		/// should debug render or not.
		/// </summary>
		public bool shouldDebugRender = true;

		/// <summary>
		/// holds the current RenderState of the last rendered Renderable (or the Renderer.renderState if no changes were made)
		/// </summary>
		RenderState _currentRenderState;


		public Renderer( int renderOrder, Camera camera )
		{
			this.camera = camera;
			this.renderOrder = renderOrder;
		}


		/// <summary>
		/// if a RenderTarget is used this will set it up. The SpriteBatch is also started. The passed in Camera will be used to set the ViewPort
		/// (if a ViewportAdapter is present) and for the SpriteBatch transform Matrix.
		/// </summary>
		/// <param name="cam">Cam.</param>
		protected virtual void beginRender( Camera cam )
		{
			// if we have a renderTarget render into it
			if( renderTexture != null )
			{
				Core.graphicsDevice.SetRenderTarget( renderTexture );
				Core.graphicsDevice.Clear( renderTargetClearColor );
			}

			_currentRenderState = renderState;
			Graphics.instance.spriteBatch.Begin( spriteSortMode, _currentRenderState.blendState, _currentRenderState.samplerState, _currentRenderState.depthStencilState, rasterizerState, _currentRenderState.effect, cam.transformMatrix );
		}


		abstract public void render( Scene scene );


		/// <summary>
		/// renders the RenderableComponent flushing the SpriteBatch and resetting RenderState if necessary
		/// </summary>
		/// <param name="renderable">Renderable.</param>
		/// <param name="cam">Cam.</param>
		protected void renderAfterStateCheck( RenderableComponent renderable, Camera cam )
		{
			// check for RenderState changes
			if( renderable.renderState != null && renderable.renderState != _currentRenderState )
			{
				_currentRenderState = renderable.renderState;
				if( _currentRenderState.effect != null )
					_currentRenderState.onPreRender( cam );
				flushSpriteBatch( cam );
			}
			else if( renderable.renderState == null && _currentRenderState != renderState )
			{
				_currentRenderState = renderState;
				flushSpriteBatch( cam );
			}

			renderable.render( Graphics.instance, cam );
		}


		/// <summary>
		/// force flushes the SpriteBatch by calling End then Begin on it.
		/// </summary>
		void flushSpriteBatch( Camera cam )
		{
			Graphics.instance.spriteBatch.End();
			Graphics.instance.spriteBatch.Begin( spriteSortMode, _currentRenderState.blendState, _currentRenderState.samplerState, _currentRenderState.depthStencilState, rasterizerState, _currentRenderState.effect, cam.transformMatrix );
		}


		/// <summary>
		/// ends the SpriteBatch and clears the RenderTarget if it had a RenderTarget
		/// </summary>
		protected virtual void endRender()
		{
			Graphics.instance.spriteBatch.End();

			// clear the RenderTarget so that we render to the screen if we were using a RenderTarget
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
			Graphics.instance.spriteBatch.Begin( SpriteSortMode.Deferred, null, null, null, null, null, Core.scene.camera.transformMatrix );

			for( var i = 0; i < scene.entities.Count; i++ )
			{
				var entity = scene.entities[i];
				if( entity.enabled )
					entity.debugRender( Graphics.instance );
			}
		}


		/// <summary>
		/// called when the default scene RenderTarget is resized and when adding a Renderer if the scene has already began. default implementation
		/// calls through to RenderTexture.onSceneBackBufferSizeChanged
		/// so that it can size itself appropriately if necessary.
		/// </summary>
		/// <param name="newWidth">New width.</param>
		/// <param name="newHeight">New height.</param>
		public virtual void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			if( renderTexture != null )
				renderTexture.onSceneBackBufferSizeChanged( newWidth, newHeight );
		}


		/// <summary>
		/// called when a scene is ended. use this for cleanup.
		/// </summary>
		public virtual void unload()
		{
			if( renderTexture != null )
			{
				renderTexture.Dispose();
				renderTexture = null;
			}
		}


		public int CompareTo( Renderer other )
		{
			return renderOrder.CompareTo( other.renderOrder );
		}
	}
}

