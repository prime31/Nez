using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class Core : Game
	{
		/// <summary>
		/// facilitates easy access the GraphicsDevice and global Content instance
		/// </summary>
		public static Core instance;
		public static Emitter<CoreEvents> emitter;
		public static float deltaTime;
		public static float timeScale = 1f;
		public static bool exitOnEscapeKeypress = true;
		public static bool pauseOnFocusLost = true;
		public static bool enableDebugRender = false;
		/// <summary>
		/// total number of frames that have passed
		/// </summary>
		public static uint frameCount;

		public Scene nextScene;
		Scene _scene;
		internal GraphicsDeviceManager _graphics;


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

			_graphics = new GraphicsDeviceManager( this );
			_graphics.PreferredBackBufferWidth = width;
			_graphics.PreferredBackBufferHeight = height;
			_graphics.IsFullScreen = false;
			_graphics.SynchronizeWithVerticalRetrace = true;
			_graphics.DeviceReset += onGraphicsDeviceReset;

			Window.AllowUserResizing = true;

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			IsFixedTimeStep = false;

			// TODO
			this.width = width;
			this.height = height;
		}


		int width;
		int height;
//		void updateView()
//		{
//			var screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
//			var screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
//			int drawWidth;
//			int drawHeight;
//
//			if( screenWidth / width > screenHeight / height )
//			{
//				drawWidth = (int)( screenHeight / height * width );
//				drawHeight = (int)screenHeight;
//			}
//			else
//			{
//				drawWidth = (int)screenWidth;
//				drawHeight = (int)( screenWidth / width * height );
//			}
//
//			Graphics.defaultGraphics.masterRenderMatrix = Matrix.CreateScale( drawWidth / (float)width );
//
//			GraphicsDevice.Viewport = new Viewport
//			{
//				X = (int)( screenWidth / 2 - drawWidth / 2 ),
//				Y = (int)( screenHeight / 2 - drawHeight / 2 ),
//				Width = drawWidth,
//				Height = drawHeight,
//				MinDepth = 0,
//				MaxDepth = 1
//			};
//		}


		/// <summary>
		/// this gets called whenever the screen size changes
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void onGraphicsDeviceReset( object sender, EventArgs e )
		{
			emitter.emit( CoreEvents.GraphicsDeviceReset );
			//updateView();
		}


		protected override void Initialize()
		{
			base.Initialize();

			// prep the default Graphics system
			Graphics.defaultGraphics = new Graphics( GraphicsDevice );
			//Tracker.Initialize();
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


		/// <summary>
		/// Called after a Scene ends, before the next Scene begins
		/// </summary>
		protected virtual void onSceneTransition()
		{
			emitter.emit( CoreEvents.SceneChanged );
			GC.Collect();
		}

	}
}

