using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Nez.Systems;
using Nez.Console;


namespace Nez
{
	public class Core : Game
	{
		/// <summary>
		/// facilitates easy access to the global Content instance
		/// </summary>
		public static Core instance;
		/// <summary>
		/// core emitter. emits only Core level events.
		/// </summary>
		public static Emitter<CoreEvents> emitter;
		public static bool exitOnEscapeKeypress = true;
		public static bool pauseOnFocusLost = true;
		public static bool enableDebugRender = false;
		public static GraphicsDevice graphicsDevice;

		public Scene nextScene;
		Scene _scene;
		CoroutineManager _coroutineManager = new CoroutineManager();
		internal GraphicsDeviceManager _graphicsManager;


		/// <summary>
		/// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update
		/// </summary>
		public static Scene scene
		{
			get { return instance._scene; }
			set { instance.nextScene = value; }
		}


		public Core( int width = 1280, int height = 720 )
		{
			instance = this;
			emitter = new Emitter<CoreEvents>( new CoreEventsComparer() );

			_graphicsManager = new GraphicsDeviceManager( this );
			//_graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8; // defaults to Depth24
			_graphicsManager.PreferredBackBufferWidth = width;
			_graphicsManager.PreferredBackBufferHeight = height;
			_graphicsManager.IsFullScreen = false;
			_graphicsManager.SynchronizeWithVerticalRetrace = true;
			_graphicsManager.DeviceReset += onGraphicsDeviceReset;

			// HACK: not sure how to do this the PCL way
			//Window.AllowUserResizing = true;

			Content.RootDirectory = "Content";
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


		#region Game overides

		protected override void Initialize()
		{
			base.Initialize();

			// prep the default Graphics system
			var font = Content.Load<SpriteFont>( "NezFont" );
			Graphics.instance = new Graphics( GraphicsDevice, font );
			graphicsDevice = GraphicsDevice;
		}


		protected override void Update( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;

			// update all our systems
			Time.update( (float)gameTime.ElapsedGameTime.TotalSeconds );
			Input.update();
			_coroutineManager.update();

			if( exitOnEscapeKeypress && Input.isKeyDown( Keys.Escape ) )
			{
				Exit();
				return;
			}

			if( _scene != null )
				_scene.update();

			if( _scene != nextScene )
			{
				if( _scene != null )
					_scene.end();

				_scene = nextScene;
				onSceneTransition();

				if( _scene != null )
					_scene.begin();
			}

			DebugConsole.instance.update();
		}


		protected override void Draw( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;

			if( _scene != null )
			{
				_scene.preRender();
				_scene.render( enableDebugRender );
				_scene.postRender();
			}

			DebugConsole.instance.render();
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
		public void setScreenSize( int width, int height, bool isFullScreen = false )
		{
			_graphicsManager.PreferredBackBufferWidth = width;
			_graphicsManager.PreferredBackBufferHeight = height;
			_graphicsManager.IsFullScreen = isFullScreen;
			//graphicsManager.ApplyChanges();
			Debug.warn( "setScreenSize doesnt work properly on OS X. It causes a crash with no stack trace" );
		}


		/// <summary>
		/// starts a coroutine. Coroutines can yeild ints/floats to delay for seconds or yeild to other calls to startCoroutine.
		/// Yielding null will make the coroutine get ticked the next frame.
		/// </summary>
		/// <returns>The coroutine.</returns>
		/// <param name="enumerator">Enumerator.</param>
		public ICoroutine startCoroutine( IEnumerator enumerator )
		{
			return _coroutineManager.startCoroutine( enumerator );
		}

	}
}

