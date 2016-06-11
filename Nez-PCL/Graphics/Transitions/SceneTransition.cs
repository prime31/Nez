using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Threading;
using Nez.Tweens;
using System.Threading.Tasks;


namespace Nez
{
	/// <summary>
	/// SceneTransition is used to transition from one Scene to another with an effect
	/// </summary>
	public abstract class SceneTransition
	{
		/// <summary>
		/// contains the last render of the previous Scene. Can be used to obscure the screen while loading a new Scene.
		/// </summary>
		public RenderTarget2D previousSceneRender;

		/// <summary>
		/// if true, Nez will render the previous scene into previousSceneRender so that you can use it with your transition 
		/// </summary>
		public bool wantsPreviousSceneRender;

		/// <summary>
		/// if true, the next Scene will be loaded on a background thread. Note that if raw PNG files are used they cannot be loaded
		/// on a background thread.
		/// </summary>
		public bool loadSceneOnBackgroundThread = false;

		/// <summary>
		/// function that should return the newly loaded scene
		/// </summary>
		protected Func<Scene> sceneLoadAction;

		/// <summary>
		/// used internally to decide if the previous Scene should render into previousSceneRender. Does double duty to ensure that the
		/// render only happens once.
		/// </summary>
		/// <value><c>true</c> if has previous scene render; otherwise, <c>false</c>.</value>
		internal bool hasPreviousSceneRender
		{
			get
			{
				if( !_hasPreviousSceneRender )
				{
					_hasPreviousSceneRender = true;
					return false;
				}

				return true;
			}
		}
		bool _hasPreviousSceneRender;

		protected bool _isNewSceneLoaded;


		public SceneTransition( Func<Scene> sceneLoadAction, bool wantsPreviousSceneRender = true )
		{
			this.sceneLoadAction = sceneLoadAction;
			this.wantsPreviousSceneRender = wantsPreviousSceneRender;

			// create a RenderTarget if we need to for later
			if( wantsPreviousSceneRender )
				previousSceneRender = new RenderTarget2D( Core.graphicsDevice, Screen.width, Screen.height, false, Screen.backBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents );
		}


		protected IEnumerator loadNextScene()
		{
			if( loadSceneOnBackgroundThread )
			{
				// load the Scene on a background thread
				var syncContext = SynchronizationContext.Current;
				Task.Run( () =>
				{
					var scene = sceneLoadAction();

					// get back to the main thread before setting the new Scene active
					syncContext.Post( d =>
					{
						Core.scene = scene;
						_isNewSceneLoaded = true;
					}, null );
				} );
			}
			else
			{
				Core.scene = sceneLoadAction();
				_isNewSceneLoaded = true;
			}

			// wait for the scene to load if it was loaded on a background thread
			while( !_isNewSceneLoaded )
				yield return null;
		}


		/// <summary>
		/// called after the previousSceneRender occurs for the first (and only) time. At this point you can load your new Scene after
		/// yielding one frame (so the first render call happens before scene loading).
		/// </summary>
		public virtual IEnumerator onBeginTransition()
		{
			yield return null;
			Core.scene = sceneLoadAction();
			transitionComplete();
		}


		/// <summary>
		/// called before the Scene is rendered. This allows a transition to render to a RenderTarget if needed and avoids issues with MonoGame
		/// clearing the framebuffer when a RenderTarget is used.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public virtual void preRender( Graphics graphics )
		{}


		/// <summary>
		/// do all of your rendering here
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public virtual void render( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( null );
			graphics.batcher.begin( BlendState.Opaque, Core.defaultSamplerState, DepthStencilState.None, null );
			graphics.batcher.draw( previousSceneRender, Vector2.Zero, Color.White );
			graphics.batcher.end();
		}


		/// <summary>
		/// this should be called when your transition is complete and the new Scene has been set. It will clean up
		/// </summary>
		protected virtual void transitionComplete()
		{
			Core._instance._sceneTransition = null;

			if( previousSceneRender != null )
			{
				previousSceneRender.Dispose();
				previousSceneRender = null;
			}
		}


		/// <summary>
		/// the most common type of transition seems to be one that ticks progress from 0 - 1. This method takes care of that for you
		/// if your transition needs to have a _progress property ticked after the scene loads.
		/// </summary>
		/// <param name="duration">duration</param>
		/// <param name="reverseDirection">if true, _progress will go from 1 to 0. If false, it goes form 0 to 1</param>
		public IEnumerator tickEffectProgressProperty( Effect effect, float duration, EaseType easeType = EaseType.ExpoOut, bool reverseDirection = false )
		{
			var start = reverseDirection ? 1f : 0f;
			var end = reverseDirection ? 0f : 1f;
			var progressParam = effect.Parameters["_progress"];

			var elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				var step = Lerps.ease( easeType, start, end, elapsed, duration );
				progressParam.SetValue( step );

				yield return null;
			}
		}

	}
}

