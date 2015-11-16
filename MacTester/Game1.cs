#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Nez;


#endregion


namespace MacTester
{
	public class Game1 : Core
	{
		public Game1()
		{}


		protected override void LoadContent()
		{
			var spriteFont = Content.Load<SpriteFont>( "bin/MacOSX/Fonts/DefaultFont" );
			IMGUI.init( GraphicsDevice, spriteFont );
		}


		protected override void Initialize()
		{
			base.Initialize();

			var scene = new Scene();
			enableDebugRender = true;

			scene.addRenderer( new DefaultRenderer() );


			// simple image with a collider
			var moonTexture = Content.Load<Texture2D>( "Images/moon.png" );
			var entity = scene.createAndAddEntity<Entity>( "first" );
			entity.addComponent( new Image( moonTexture ) );
			entity.collider = new BoxCollider( 0, 0, moonTexture.Width, moonTexture.Height );


			// TiledMap and move it back so is drawn before other entities
			var tiledEntity = scene.createAndAddEntity<Entity>( "tiled-map-entity" );
			tiledEntity.addComponent( new TiledMapComponent( "bin/MacOSX/Tilemap/tilemap", "collision" ) );
			tiledEntity.position.X = 100;
			tiledEntity.position.Y = 100;
			tiledEntity.depth += 5;

			nextScene = scene;
		}


		protected override void Update( GameTime gameTime )
		{
			if( Input.mouseWheelDelta != 0 )
			{
				scene.camera.zoom += Input.mouseWheelDelta * 0.00001f;
			}

			base.Update( gameTime );
		}


		protected override void Draw( GameTime gameTime )
		{
			base.Draw( gameTime );

			IMGUI.beginWindow( 400, 0, 150, 300 );

			if( IMGUI.button( "Click Me" ) )
				Debug.log( "Clicked the button" );

			IMGUI.bar( 0.75f );

			IMGUI.endWindow();			
		}

	}
}

