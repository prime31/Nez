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
using Nez.Textures;


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
		/// default SamplerState used by Materials. Note that this must be set at launch! Changing it after that time will result in only
		/// Materials created after it was set having the new SamplerState
		/// </summary>
		public static SamplerState defaultSamplerState = SamplerState.PointClamp;

		/// <summary>
		/// internal flag used to determine if EntitySystems should be used or not
		/// </summary>
		internal static bool entitySystemsEnabled;

		/// <summary>
		/// facilitates easy access to the global Content instance for internal classes
		/// </summary>
		internal static Core _instance;

		#if DEBUG
		internal static ulong drawCalls;
		TimeSpan _frameCounterElapsedTime = TimeSpan.Zero;
		int _frameCounter = 0;
		string _windowTitle;
		#endif

		Scene _scene;
		Scene _nextScene;
		internal SceneTransition _sceneTransition;

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


		public Core( int width = 1280, int height = 720, bool isFullScreen = false, bool enableEntitySystems = true, string windowTitle = "Nez" )
		{
			#if DEBUG
			_windowTitle = windowTitle;
			#endif

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
		}


		protected static void onClientSizeChanged( object sender, EventArgs e )
		{
			_instance.onGraphicsDeviceReset( sender, e );
		}


		/// <summary>
		/// this gets called whenever the screen size changes
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void onGraphicsDeviceReset( object sender, EventArgs e )
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
					( t.context as Core )._graphicsDeviceChangeTimer = null;
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
			var font = Content.Load<BitmapFont>( "nez/NezDefaultBMFont" );
			Graphics.instance = new Graphics( font );
			RenderTarget.instance = new RenderTarget();
		}


		protected override void Update( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
			{
				SuppressDraw();
				return;
			}

			#if DEBUG
			TimeRuler.instance.startFrame();
			TimeRuler.instance.beginMark( "update", Color.Green );
			#endif

			// update all our systems
			Time.update( (float)gameTime.ElapsedGameTime.TotalSeconds );
			Input.update();
			RenderTarget.instance.update();
			_coroutineManager.update();
			_tweenManager.update();
			_timerManager.update();

			if( exitOnEscapeKeypress && Input.isKeyDown( Keys.Escape ) || Input.gamePads[0].isButtonReleased( Buttons.Back ) )
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
				onSceneChanged();

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

			// fps counter
			_frameCounter++;
			_frameCounterElapsedTime += gameTime.ElapsedGameTime;
			if( _frameCounterElapsedTime >= TimeSpan.FromSeconds( 1 ) )
			{
				var totalMemory = ( GC.GetTotalMemory( false ) / 1048576f ).ToString( "F" );
				Window.Title = string.Format( "{0} {1} fps - {2} MB", _windowTitle, _frameCounter, totalMemory );
				_frameCounter = 0;
				_frameCounterElapsedTime -= TimeSpan.FromSeconds( 1 );
			}
			#endif

			if( _sceneTransition != null )
				_sceneTransition.preRender( Graphics.instance );

			if( _scene != null )
			{
				_scene.preRender();
				_scene.render();

				#if DEBUG
				if( debugRenderEnabled )
					Debug.render();
				#endif

				// render as usual if we dont have an active SceneTransition
				if( _sceneTransition == null )
					_scene.postRender();
			}

			// special handling of SceneTransition if we have one
			if( _sceneTransition != null )
			{
				if( _scene != null && _sceneTransition.wantsPreviousSceneRender && !_sceneTransition.hasPreviousSceneRender )
				{
					_scene.postRender( _sceneTransition.previousSceneRender );
					scene = null;
					startCoroutine( _sceneTransition.onBeginTransition() );
				}
				else
				{
					if( _scene != null )
						_scene.postRender();
				}

				_sceneTransition.render( Graphics.instance );
			}

			#if DEBUG
			TimeRuler.instance.endMark( "draw" );

			DebugConsole.instance.render();

			// the TimeRuler only needs to render when the DebugConsole is not open
			if( !DebugConsole.instance.isOpen )
				TimeRuler.instance.render();

			drawCalls = graphicsDevice.Metrics.DrawCount;
			#endif
		}

		#endregion


		/// <summary>
		/// Called after a Scene ends, before the next Scene begins
		/// </summary>
		void onSceneChanged()
		{
			emitter.emit( CoreEvents.SceneChanged );
			Time.sceneChanged();
			GC.Collect();
		}


		/// <summary>
		/// temporarily runs SceneTransition allowing one Scene to transition to another smoothly with custom effects.
		/// </summary>
		/// <param name="sceneTransition">Scene transition.</param>
		public static void startSceneTransition( SceneTransition sceneTransition )
		{
			Assert.isNull( _instance._sceneTransition, "You cannot start a new SceneTransition until the previous one has completed" );
			_instance._sceneTransition = sceneTransition;
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


		/// <summary>
		/// schedules a one-time timer that will call the passed in Action after timeInSeconds
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="context">Context.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer schedule( float timeInSeconds, object context, Action<ITimer> onTime )
		{
			return _instance._timerManager.schedule( timeInSeconds, false, context, onTime );
		}


		/// <summary>
		/// schedules a one-time or repeating timer that will call the passed in Action
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="repeats">If set to <c>true</c> repeats.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer schedule( float timeInSeconds, bool repeats, Action<ITimer> onTime )
		{
			return _instance._timerManager.schedule( timeInSeconds, repeats, null, onTime );
		}


		/// <summary>
		/// schedules a one-time timer that will call the passed in Action after timeInSeconds
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer schedule( float timeInSeconds, Action<ITimer> onTime )
		{
			return _instance._timerManager.schedule( timeInSeconds, false, null, onTime );
		}

		#endregion

	}
}

