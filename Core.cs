using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class Core : Game
	{
		/// <summary>
		/// facilitates easy access to the global Content instance
		/// </summary>
		public static Core instance;
		public static Emitter<CoreEvents> emitter;
		public static float deltaTime;
		public static float timeScale = 1f;
		public static bool exitOnEscapeKeypress = true;
		public static bool pauseOnFocusLost = true;
		public static bool enableDebugRender = false;
		public static GraphicsDevice graphicsDevice;
		/// <summary>
		/// total number of frames that have passed
		/// </summary>
		public static uint frameCount;

		public Scene nextScene;
		Scene _scene;
		GraphicsDeviceManager _graphicsManager;


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
			_graphicsManager.PreferredBackBufferWidth = width;
			_graphicsManager.PreferredBackBufferHeight = height;
			_graphicsManager.IsFullScreen = false;
			_graphicsManager.SynchronizeWithVerticalRetrace = true;
			_graphicsManager.DeviceReset += onGraphicsDeviceReset;

			Window.AllowUserResizing = true;

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
			emitter.emit( CoreEvents.GraphicsDeviceReset );
		}


		#region Game overides

		protected override void Initialize()
		{
			base.Initialize();

			// prep the default Graphics system
			Graphics.defaultGraphics = new Graphics( GraphicsDevice );
			graphicsDevice = GraphicsDevice;
		}


		protected override void LoadContent()
		{}


		protected override void Update( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;
			
			deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
			Input.update();

			if( exitOnEscapeKeypress && Input.getKeyDown( Keys.Escape ) )
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

			frameCount++;
		}


		protected override void Draw( GameTime gameTime )
		{
			if( pauseOnFocusLost && !IsActive )
				return;
			
			if( _scene != null )
			{
				_scene.preRender();
				_scene.render();

				if( enableDebugRender )
					_scene.debugRender();
				
				_scene.postRender();
			}
		}

		#endregion


		/// <summary>
		/// Called after a Scene ends, before the next Scene begins
		/// </summary>
		protected virtual void onSceneTransition()
		{
			emitter.emit( CoreEvents.SceneChanged );
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

	}
}

