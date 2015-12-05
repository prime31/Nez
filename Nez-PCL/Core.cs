using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Nez.Systems;
using Nez.Console;
using Nez.Tweens;
using Nez.Timers;
using Nez.BitmapFonts;


namespace Nez
{
	public class Core : Game
	{
		/// <summary>
		/// core emitter. emits only Core level events.
		/// </summary>
		public static Emitter<CoreEvents> emitter;
		public static bool exitOnEscapeKeypress = true;
		public static bool pauseOnFocusLost = true;
		public static bool debugRenderEnabled = false;
		public static GraphicsDevice graphicsDevice;

		/// <summary>
		/// global content manager for loading any assets that should stick around between scenes
		/// </summary>
		public static NezContentManager contentManager;

		/// <summary>
		/// facilitates easy access to the global Content instance for internal classes
		/// </summary>
		internal static Core _instance;

		internal GraphicsDeviceManager _graphicsManager;
		Scene _scene;
		Scene _nextScene;

		// globally accessible systems
		CoroutineManager _coroutineManager = new CoroutineManager();
		TweenManager _tweenManager = new TweenManager();
		TimerManager _timerManager = new TimerManager();


		/// <summary>
		/// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update
		/// </summary>
		public static Scene scene
		{
			get { return _instance._scene; }
			set { _instance._nextScene = value; }
		}


		public Core( int width = 1280, int height = 720, bool isFullScreen = false )
		{
			_instance = this;
			emitter = new Emitter<CoreEvents>( new CoreEventsComparer() );

			_graphicsManager = new GraphicsDeviceManager( this );
			//_graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8; // defaults to Depth24
			_graphicsManager.PreferredBackBufferWidth = width;
			_graphicsManager.PreferredBackBufferHeight = height;
			_graphicsManager.IsFullScreen = isFullScreen;
			_graphicsManager.SynchronizeWithVerticalRetrace = true;
			_graphicsManager.DeviceReset += onGraphicsDeviceReset;

			// HACK: not sure how to do this the PCL way
			//Window.AllowUserResizing = true;

			Content.RootDirectory = "Content";
			contentManager = new NezContentManager( Services, Content.RootDirectory );
			IsMouseVisible = true;
			IsFixedTimeStep = false;
		}


		/// <summary>
		/// this gets called whenever the screen size changes
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void onGraphicsDeviceReset( object sender, EventArgs e )
		{
			// TODO: coalese these to avoid spamming once we have a scheduler/timer
			emitter.emit( CoreEvents.GraphicsDeviceReset );
		}


		#region Passthroughs to Game

		public static void exit()
		{
			_instance.Exit();
		}

		#endregion


		#region Game overides

		protected override void Initialize()
		{
			base.Initialize();

			// prep the default Graphics system
			graphicsDevice = GraphicsDevice;
			var font = Content.Load<BitmapFont>( "NezDefaultBMFont" );
			Graphics.instance = new Graphics( font );
		}


		protected override void Update( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;

			// update all our systems
			Time.update( (float)gameTime.ElapsedGameTime.TotalSeconds );
			Input.update();
			_coroutineManager.update();
			_tweenManager.update();
			_timerManager.update();

			if( exitOnEscapeKeypress && Input.isKeyDown( Keys.Escape ) )
			{
				Exit();
				return;
			}

			if( _scene != null )
				_scene.update();

			if( _scene != _nextScene )
			{
				if( _scene != null )
					_scene.end();

				_scene = _nextScene;
				onSceneTransition();

				if( _scene != null )
					_scene.begin();
			}

			#if DEBUG
			DebugConsole.instance.update();
			#endif
		}


		protected override void Draw( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;

			if( _scene != null )
			{
				_scene.preRender();
				_scene.render( debugRenderEnabled );
				_scene.postRender();
			}

			#if DEBUG
			DebugConsole.instance.render();
			if( debugRenderEnabled )
				Debug.render();
			#endif
		}

		#endregion


		/// <summary>
		/// Called after a Scene ends, before the next Scene begins
		/// </summary>
		protected virtual void onSceneTransition()
		{
			emitter.emit( CoreEvents.SceneChanged );
			Time.sceneChanged();
			GC.Collect();
		}


		/// <summary>
		/// sets the screen size and applies the changes
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="isFullScreen">If set to <c>true</c> is full screen.</param>
		public static void setScreenSize( int width, int height, bool isFullScreen = false )
		{
			_instance._graphicsManager.PreferredBackBufferWidth = width;
			_instance._graphicsManager.PreferredBackBufferHeight = height;
			_instance._graphicsManager.IsFullScreen = isFullScreen;
			//instance._graphicsManager.ApplyChanges();
			Debug.warn( "setScreenSize doesnt work properly on OS X. It causes a crash with no stack trace" );
		}


		#region Systems access

		/// <summary>
		/// starts a coroutine. Coroutines can yeild ints/floats to delay for seconds or yeild to other calls to startCoroutine.
		/// Yielding null will make the coroutine get ticked the next frame.
		/// </summary>
		/// <returns>The coroutine.</returns>
		/// <param name="enumerator">Enumerator.</param>
		public static ICoroutine startCoroutine( IEnumerator enumerator )
		{
			return _instance._coroutineManager.startCoroutine( enumerator );
		}


		/// <summary>
		/// schedules a one-time or repeating timer that will call the passed in Action
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="repeats">If set to <c>true</c> repeats.</param>
		/// <param name="context">Context.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer schedule( float timeInSeconds, bool repeats, object context, Action<ITimer> onTime )
		{
			return _instance._timerManager.schedule( timeInSeconds, repeats, context, onTime );
		}

		#endregion

	}
}

