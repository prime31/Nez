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
using Nez.Analysis;


namespace Nez
{
	public class Core : Game
	{
		/// <summary>
		/// core emitter. emits only Core level events.
		/// </summary>
		public static Emitter<CoreEvents> emitter;

		/// <summary>
		/// enables/disables if we should quit the app when escape is pressed
		/// </summary>
		public static bool exitOnEscapeKeypress = true;

		/// <summary>
		/// enables/disables pausing when focus is lost. No update or render methods will be called if true when not in focus.
		/// </summary>
		public static bool pauseOnFocusLost = true;

		/// <summary>
		/// enables/disables debug rendering
		/// </summary>
		public static bool debugRenderEnabled = false;

		/// <summary>
		/// global access to the graphicsDevice
		/// </summary>
		public static GraphicsDevice graphicsDevice;

		/// <summary>
		/// global content manager for loading any assets that should stick around between scenes
		/// </summary>
		public static NezContentManager contentManager;

		/// <summary>
		/// internal flag used to determine if EntitySystems should be used or not
		/// </summary>
		internal static bool entitySystemsEnabled;

		/// <summary>
		/// facilitates easy access to the global Content instance for internal classes
		/// </summary>
		internal static Core _instance;

		Scene _scene;
		Scene _nextScene;

		/// <summary>
		/// used to coalesce GraphicsDeviceReset events
		/// </summary>
		ITimer _graphicsDeviceChangeTimer;

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


		public Core( int width = 1280, int height = 720, bool isFullScreen = false, bool enableEntitySystems = true )
		{
			_instance = this;
			emitter = new Emitter<CoreEvents>( new CoreEventsComparer() );

			var graphicsManager = new GraphicsDeviceManager( this );
			graphicsManager.PreferredBackBufferWidth = width;
			graphicsManager.PreferredBackBufferHeight = height;
			graphicsManager.IsFullScreen = isFullScreen;
			graphicsManager.SynchronizeWithVerticalRetrace = true;
			graphicsManager.DeviceReset += onGraphicsDeviceReset;
			graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
			Screen.initialize( graphicsManager );

			Content.RootDirectory = "Content";
			contentManager = new NezContentManager( Services, Content.RootDirectory );
			IsMouseVisible = true;
			IsFixedTimeStep = false;

			entitySystemsEnabled = enableEntitySystems;
			if( enableEntitySystems )
				ComponentTypeManager.initialize();
		}


		/// <summary>
		/// this gets called whenever the screen size changes
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void onGraphicsDeviceReset( object sender, EventArgs e )
		{
			// we coalese these to avoid spamming events
			if( _graphicsDeviceChangeTimer != null )
			{
				_graphicsDeviceChangeTimer.reset();
			}
			else
			{
				_graphicsDeviceChangeTimer = schedule( 0.05f, false, this, t =>
				{
					( this as Core )._graphicsDeviceChangeTimer = null;
					emitter.emit( CoreEvents.GraphicsDeviceReset );
				} );
			}
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

			#if DEBUG
			TimeRuler.instance.startFrame();
			TimeRuler.instance.beginMark( "update", Color.Green );
			#endif

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
			TimeRuler.instance.endMark( "update" );
			DebugConsole.instance.update();
			#endif
		}


		protected override void Draw( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;

			#if DEBUG
			TimeRuler.instance.beginMark( "draw", Color.Gold );
			#endif

			if( _scene != null )
			{
				_scene.preRender();
				_scene.render( debugRenderEnabled );
				_scene.postRender();
			}

			#if DEBUG
			TimeRuler.instance.endMark( "draw" );

			if( DebugConsole.instance.isOpen )
				DebugConsole.instance.render();
			else
				TimeRuler.instance.render();
			
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

