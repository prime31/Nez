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
	/// SceneTransition is used to transition from one Scene to another or within a scene with an effect. If sceneLoadAction is null Nez
	/// will perform an in-Scene transition as opposed to loading a new Scene mid transition.
	/// 
	/// The general gist of a transition is the following:
	/// - onBeginTransition will be called allowing you to yield for multipart transitions
	/// - for two part transitions with Effects you can yield on a call to tickEffectProgressProperty for part one to obscure the screen
	/// - next, yield a call to loadNextScene to load up the new Scene
	/// - finally, yield again on tickEffectProgressProperty to un-obscure the screen and show the new Scene
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
		public bool loadSceneOnBackgroundThread;

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

		/// <summary>
		/// called when loadNextScene is executing. This is useful when doing inter-Scene transitions so that you know when you can more the
		/// Camera or reset any Entities
		/// </summary>
		public Action onScreenObscured;

        /// <summary>
        /// called when the Transition has completed it's execution, so that other work can be called, such as Starting another transition.
        /// </summary>
        public Action onTransitionCompleted;

		/// <summary>
		/// flag indicating if this transition will load a new scene or not
		/// </summary>
		internal bool _loadsNewScene;

		bool _hasPreviousSceneRender;

		/// <summary>
		/// use this for two part transitions. For example, a fade would fade to black first then when _isNewSceneLoaded becomes true it would
		/// fade in. For in-Scene transitions _isNewSceneLoaded will be set to true at the midpoint just as if a new Scene was loaded.
		/// </summary>
		protected bool _isNewSceneLoaded;


		protected SceneTransition( bool wantsPreviousSceneRender = true ) : this( null, wantsPreviousSceneRender )
		{}


		protected SceneTransition( Func<Scene> sceneLoadAction, bool wantsPreviousSceneRender = true )
		{
			this.sceneLoadAction = sceneLoadAction;
			this.wantsPreviousSceneRender = wantsPreviousSceneRender;
			_loadsNewScene = sceneLoadAction != null;

			// create a RenderTarget if we need to for later
			if( wantsPreviousSceneRender )
				previousSceneRender = new RenderTarget2D( Core.graphicsDevice, Screen.width, Screen.height, false, Screen.backBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents );
		}


		protected IEnumerator loadNextScene()
		{
			// let the listener know the screen is obscured if we have one
			if( onScreenObscured != null )
				onScreenObscured();
			
			// if we arent loading a new scene we just set the flag as if we did so that 2 phase transitions complete
			if( !_loadsNewScene )
			{
				_isNewSceneLoaded = true;
				yield break;
			}
			
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

            if( onTransitionCompleted != null )
                onTransitionCompleted();
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

