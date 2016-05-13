using System;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and Entity.debugRender.
	/// A simple Renderer could just start the Graphics.instanceGraphics.batcher or it could create its own local Graphics instance
	/// if it needs it for some kind of custom rendering.
	/// 
	/// Note that it is a best practice to ensure all Renderers that render to a RenderTarget have lower renderOrders to avoid issues
	/// with clearing the back buffer (http://gamedev.stackexchange.com/questions/90396/monogame-setrendertarget-is-wiping-the-backbuffer).
	/// Giving them a negative renderOrder is a good strategy to deal with this.
	/// </summary>
	public abstract class Renderer : IComparable<Renderer>
	{
		/// <summary>
		/// Material used by the Batcher. Any RenderableComponent can override this.
		/// </summary>
		public Material material = Material.defaultMaterial;

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
		/// flag for this renderer that decides if it should debug render or not. The render method receives a bool (debugRenderEnabled)
		/// letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide if it
		/// should debug render or not.
		/// </summary>
		public bool shouldDebugRender = true;

		/// <summary>
		/// if true, the Scene will call SetRenderTarget with the scene RenderTarget. The default implementaiton returns true if the Renderer
		/// has a renderTexture
		/// </summary>
		/// <value><c>true</c> if wants to render to scene render target; otherwise, <c>false</c>.</value>
		public virtual bool wantsToRenderToSceneRenderTarget { get { return renderTexture == null; } }

		/// <summary>
		/// if true, the Scene will call the render method AFTER all PostProcessors have finished. This must be set to true BEFORE calling
		/// Scene.addRenderer to take effect and the Renderer should NOT have a renderTexture. The main reason for this type of Renderer
		/// is so that you can render your UI without post processing on top of the rest of your Scene. The ScreenSpaceRenderer is an
		/// example Renderer that sets this to true;
		/// </summary>
		public bool wantsToRenderAfterPostProcessors;

		/// <summary>
		/// holds the current Material of the last rendered Renderable (or the Renderer.material if no changes were made)
		/// </summary>
		protected Material _currentMaterial;


		public Renderer( int renderOrder ) : this( renderOrder, null )
		{}


		public Renderer( int renderOrder, Camera camera )
		{
			this.camera = camera;
			this.renderOrder = renderOrder;
		}


		/// <summary>
		/// if a RenderTarget is used this will set it up. The Batcher is also started. The passed in Camera will be used to set the ViewPort
		/// (if a ViewportAdapter is present) and for the Batcher transform Matrix.
		/// </summary>
		/// <param name="cam">Cam.</param>
		protected virtual void beginRender( Camera cam )
		{
			// if we have a renderTarget render into it
			if( renderTexture != null )
			{
				Core.graphicsDevice.setRenderTarget( renderTexture );
				Core.graphicsDevice.Clear( renderTargetClearColor );
			}

			_currentMaterial = material;
			Graphics.instance.batcher.begin( _currentMaterial, cam.transformMatrix );
		}


		abstract public void render( Scene scene );


		/// <summary>
		/// renders the RenderableComponent flushing the Batcher and resetting current material if necessary
		/// </summary>
		/// <param name="renderable">Renderable.</param>
		/// <param name="cam">Cam.</param>
		protected void renderAfterStateCheck( RenderableComponent renderable, Camera cam )
		{
			// check for Material changes
			if( renderable.material != null && renderable.material != _currentMaterial )
			{
				_currentMaterial = renderable.material;
				if( _currentMaterial.effect != null )
					_currentMaterial.onPreRender( cam );
				flushBatch( cam );
			}
			else if( renderable.material == null && _currentMaterial != material )
			{
				_currentMaterial = material;
				flushBatch( cam );
			}

			renderable.render( Graphics.instance, cam );
		}


		/// <summary>
		/// force flushes the Batcher by calling End then Begin on it.
		/// </summary>
		void flushBatch( Camera cam )
		{
			Graphics.instance.batcher.end();
			Graphics.instance.batcher.begin( _currentMaterial, cam.transformMatrix );
		}


		/// <summary>
		/// ends the Batcher and clears the RenderTarget if it had a RenderTarget
		/// </summary>
		protected virtual void endRender()
		{
			Graphics.instance.batcher.end();
		}


		/// <summary>
		/// default debugRender method just loops through all entities and calls entity.debugRender
		/// </summary>
		/// <param name="scene">Scene.</param>
		protected virtual void debugRender( Scene scene, Camera cam )
		{
			Graphics.instance.batcher.end();
			Graphics.instance.batcher.begin( Core.scene.camera.transformMatrix );

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

