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
using System.Diagnostics;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Nez.ImGui" )]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Nez.Persistence" )]

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
		public static NezContentManager content;

		/// <summary>
		/// default SamplerState used by Materials. Note that this must be set at launch! Changing it after that time will result in only
		/// Materials created after it was set having the new SamplerState
		/// </summary>
		public static SamplerState defaultSamplerState = new SamplerState
		{
			Filter = TextureFilter.Point
		};

		/// <summary>
		/// default wrapped SamplerState. Determined by the Filter of the defaultSamplerState.
		/// </summary>
		/// <value>The default state of the wraped sampler.</value>
		public static SamplerState defaultWrappedSamplerState { get { return defaultSamplerState.Filter == TextureFilter.Point ? SamplerState.PointWrap : SamplerState.LinearWrap; } }

		/// <summary>
		/// default GameServiceContainer access
		/// </summary>
		/// <value>The services.</value>
		public static GameServiceContainer services => _instance.Services;

		/// <summary>
		/// provides access to the single Core/Game instance
		/// </summary>
		public static Core instance => _instance;

		/// <summary>
		/// facilitates easy access to the global Content instance for internal classes
		/// </summary>
		internal static Core _instance;

		/// <summary>
		/// internal flag used to determine if EntitySystems should be used or not
		/// </summary>
		internal static bool entitySystemsEnabled;

#if DEBUG
		internal static long drawCalls;
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
		FastList<GlobalManager> _globalManagers = new FastList<GlobalManager>();
		CoroutineManager _coroutineManager = new CoroutineManager();
		TimerManager _timerManager = new TimerManager();


		/// <summary>
		/// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update
		/// </summary>
		public static Scene scene
		{
			get => _instance._scene;
			set
			{
				Insist.isNotNull( value, "Scene cannot be null!" );

				// handle our initial Scene. If we have no Scene and one is assigned directly wire it up
				if( _instance._scene == null )
				{
					_instance._scene = value;
					_instance._scene.begin();
					_instance.onSceneChanged();
				}
				else
				{
					_instance._nextScene = value;
				}
			}
		}


		public Core( int width = 1280, int height = 720, bool isFullScreen = false, bool enableEntitySystems = true, string windowTitle = "Nez", string contentDirectory = "Content" )
		{
#if DEBUG
			_windowTitle = windowTitle;
#endif

			_instance = this;
			emitter = new Emitter<CoreEvents>( new CoreEventsComparer() );

			var graphicsManager = new GraphicsDeviceManager( this )
			{
				PreferredBackBufferWidth = width,
				PreferredBackBufferHeight = height,
				IsFullScreen = isFullScreen,
				SynchronizeWithVerticalRetrace = true
			};
			graphicsManager.DeviceReset += onGraphicsDeviceReset;
			graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

			Screen.initialize( graphicsManager );
			Window.ClientSizeChanged += onGraphicsDeviceReset;
			Window.OrientationChanged += onOrientationChanged;

			Content.RootDirectory = contentDirectory;
			content = new NezGlobalContentManager( Services, Content.RootDirectory );
			IsMouseVisible = true;
			IsFixedTimeStep = false;

			entitySystemsEnabled = enableEntitySystems;

			// setup systems
			registerGlobalManager( _coroutineManager );
			registerGlobalManager( new TweenManager() );
			registerGlobalManager( _timerManager );
			registerGlobalManager( new RenderTarget() );
		}

		void onOrientationChanged( object sender, EventArgs e )
		{
			emitter.emit( CoreEvents.OrientationChanged );
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
			var font = content.Load<BitmapFont>( "nez://Nez.Content.NezDefaultBMFont.xnb" );
			Graphics.instance = new Graphics( font );
		}

		protected override void Update( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
			{
				SuppressDraw();
				return;
			}

			startDebugUpdate();

			// update all our systems and global managers
			Time.update( (float)gameTime.ElapsedGameTime.TotalSeconds );
			Input.update();

			if( exitOnEscapeKeypress && ( Input.isKeyDown( Keys.Escape ) || Input.gamePads[0].isButtonReleased( Buttons.Back ) ) )
			{
				Exit();
				return;
			}

			if( _scene != null )
			{
				for( var i = _globalManagers.length - 1; i >= 0; i-- )
				{
					if( _globalManagers.buffer[i].enabled )
						_globalManagers.buffer[i].update();
				}

				// read carefully:
				// - we do not update the Scene while a SceneTransition is happening
				// 		- unless it is SceneTransition that doesn't change Scenes (no reason not to update)
				//		- or it is a SceneTransition that has already switched to the new Scene (the new Scene needs to do its thing)
				if( _sceneTransition == null ||
					( _sceneTransition != null && ( !_sceneTransition._loadsNewScene || _sceneTransition._isNewSceneLoaded ) ) )
				{
					_scene.update();
				}

				if( _nextScene != null )
				{
					_scene.end();

					_scene = _nextScene;
					_nextScene = null;
					onSceneChanged();

					_scene.begin();
				}
			}

			endDebugUpdate();

#if FNA
			// MonoGame only updates old-school XNA Components in Update which we dont care about. FNA's core FrameworkDispatcher needs
			// Update called though so we do so here.
			FrameworkDispatcher.Update();
#endif
		}

		protected override void Draw( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;

			startDebugDraw( gameTime.ElapsedGameTime );

			if( _sceneTransition != null )
				_sceneTransition.preRender( Graphics.instance );

			// special handling of SceneTransition if we have one. We either render the SceneTransition or the Scene
			if( _sceneTransition != null )
			{
				if( _scene != null && _sceneTransition.wantsPreviousSceneRender && !_sceneTransition.hasPreviousSceneRender )
				{
					_scene.render();
					_scene.postRender( _sceneTransition.previousSceneRender );
					startCoroutine( _sceneTransition.onBeginTransition() );
				}
				else if( _scene != null && _sceneTransition._isNewSceneLoaded )
				{
					_scene.render();
					_scene.postRender();
				}

				_sceneTransition.render( Graphics.instance );
			}
			else if( _scene != null )
			{
				_scene.render();

#if DEBUG
				if( debugRenderEnabled )
					Debug.render();
#endif

				// render as usual if we dont have an active SceneTransition
				_scene.postRender();
			}

			endDebugDraw();
		}

		protected override void OnExiting( object sender, EventArgs args )
		{
			base.OnExiting( sender, args );
			emitter.emit( CoreEvents.Exiting );
		}

		#endregion

		#region Debug Injection

		[Conditional( "DEBUG" )]
		void startDebugUpdate()
		{
#if DEBUG
			TimeRuler.instance.startFrame();
			TimeRuler.instance.beginMark( "update", Color.Green );
#endif
		}

		[Conditional( "DEBUG" )]
		void endDebugUpdate()
		{
#if DEBUG
			TimeRuler.instance.endMark( "update" );
			DebugConsole.instance.update();
			drawCalls = 0;
#endif
		}

		[Conditional( "DEBUG" )]
		void startDebugDraw( TimeSpan elapsedGameTime )
		{
#if DEBUG
			TimeRuler.instance.beginMark( "draw", Color.Gold );

			// fps counter
			_frameCounter++;
			_frameCounterElapsedTime += elapsedGameTime;
			if( _frameCounterElapsedTime >= TimeSpan.FromSeconds( 1 ) )
			{
				var totalMemory = ( GC.GetTotalMemory( false ) / 1048576f ).ToString( "F" );
				Window.Title = string.Format( "{0} {1} fps - {2} MB", _windowTitle, _frameCounter, totalMemory );
				_frameCounter = 0;
				_frameCounterElapsedTime -= TimeSpan.FromSeconds( 1 );
			}
#endif
		}

		[Conditional( "DEBUG" )]
		void endDebugDraw()
		{
#if DEBUG
			TimeRuler.instance.endMark( "draw" );
			DebugConsole.instance.render();

			// the TimeRuler only needs to render when the DebugConsole is not open
			if( !DebugConsole.instance.isOpen )
				TimeRuler.instance.render();

#if !FNA
			drawCalls = graphicsDevice.Metrics.DrawCount;
#endif
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
		public static T startSceneTransition<T>( T sceneTransition ) where T : SceneTransition
		{
			Insist.isNull( _instance._sceneTransition, "You cannot start a new SceneTransition until the previous one has completed" );
			_instance._sceneTransition = sceneTransition;
			return sceneTransition;
		}


		#region Global Managers

		/// <summary>
		/// adds a global manager object that will have its update method called each frame before Scene.update is called
		/// </summary>
		/// <returns>The global manager.</returns>
		/// <param name="manager">Manager.</param>
		public static void registerGlobalManager( GlobalManager manager )
		{
			_instance._globalManagers.add( manager );
			manager.enabled = true;
		}

		/// <summary>
		/// removes the global manager object
		/// </summary>
		/// <returns>The global manager.</returns>
		/// <param name="manager">Manager.</param>
		public static void unregisterGlobalManager( GlobalManager manager )
		{
			_instance._globalManagers.remove( manager );
			manager.enabled = false;
		}

		/// <summary>
		/// gets the global manager of type T
		/// </summary>
		/// <returns>The global manager.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T getGlobalManager<T>() where T : GlobalManager
		{
			for( var i = 0; i < _instance._globalManagers.length; i++ )
			{
				if( _instance._globalManagers.buffer[i] is T )
					return _instance._globalManagers.buffer[i] as T;
			}
			return null;
		}

		#endregion


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

