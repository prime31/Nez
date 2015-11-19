using System;
using Nez;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.TextureAtlases;


namespace MacTester
{
	public static class Scenes
	{
		public static Scene sceneOne()
		{
			var scene = new Scene();
			scene.clearColor = Color.Black;

			// add a default renderer which will render everything
			scene.addRenderer( new DefaultRenderer() );
			scene.camera._viewportAdapter = new ScalingViewportAdapter( Core.graphicsDevice, 256, 144 );
			Core.instance.setScreenSize( 256 * 4, 144 * 4 );


			// load a TiledMap and move it back so is drawn before other entities
			var tiledEntity = scene.createAndAddEntity<Entity>( "tiled-map-entity" );
			tiledEntity.addComponent( new TiledMapComponent( "bin/MacOSX/Tilemap/tilemap", "collision" ) );
			tiledEntity.depth += 5;

			return scene;
		}


		public static Scene sceneTwo()
		{
			var scene = new Scene();
			scene.clearColor = Color.Coral;

			// setup a renderer that renders everything to a RenderTexture making sure its order is before standard renderers!
			var renderer = new DefaultRenderer( scene.camera, -1 );
			renderer.renderTexture = new RenderTexture( Core.graphicsDevice, 320, 240 );
			renderer.renderTextureClearColor = Color.CornflowerBlue;
			scene.addRenderer( renderer );

			// add a standard renderer that renders to the screen
			scene.addRenderer( new DefaultRenderer() );

			// stick a couple moons on screen
			var entity = scene.createAndAddEntity<Entity>( "moon" );
			var image = new Image( scene.contentManager.Load<Texture2D>( "Images/moon" ) );
			image.originNormalized = new Vector2( 0.5f, 0.5f );
			image.zoom = 2f;
			entity.addComponent( image );
			entity.addComponent( new FramesPerSecondCounter( Graphics.instance.defaultFont, Color.White, FramesPerSecondCounter.FPSDockPosition.TopLeft ) );
			entity.position.Y = 120;


			entity = scene.createAndAddEntity<Entity>( "new-moon" );
			image = new Image( scene.contentManager.Load<Texture2D>( "Images/moon" ) );
			entity.position.X = 130;
			entity.position.Y = 130;
			entity.addComponent( image );
			entity.collider = new BoxCollider( 0, 0, image.width, image.height );

			// add a post processor to display the RenderTexture
			var postProcessor = new SimplePostProcessor( renderer.renderTexture );
			scene.addPostProcessStep( postProcessor );
			scene.enablePostProcessing = true;

			return scene;
		}


		public static Scene sceneThree( bool useBoxColliders = true )
		{
			var scene = new Scene();
			scene.addRenderer( new DefaultRenderer() );
			scene.clearColor = Color.BlanchedAlmond;
			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );


			// create some moons
			Action<Vector2,string> moonMaker = ( Vector2 pos, string name ) =>
			{
				var ent = scene.createAndAddEntity<Entity>( name );
				ent.position = pos;
				ent.addComponent( new Image( moonTexture ) );
				if( useBoxColliders )
					ent.collider = new BoxCollider( moonTexture.Bounds );
				else
					ent.collider = new CircleCollider( moonTexture.Width * 0.5f );
			};

			moonMaker( new Vector2( 400, 10 ), "moon1" );
			moonMaker( new Vector2( 10, 10 ), "moon2" );
			moonMaker( new Vector2( 50, 500 ), "moon3" );
			moonMaker( new Vector2( 500, 250 ), "moon4" );


			// create a player moon
			var entity = scene.createAndAddEntity<Entity>( "player-moon" );
			entity.addComponent( new SimpleMoonMover() );
			var sprite = new Sprite<int>( new Subtexture( moonTexture ) );
			sprite.color = Color.Blue;
			entity.addComponent( sprite );
			if( useBoxColliders )
			{
				entity.collider = new BoxCollider( sprite.bounds );
			}
			else
			{
				// Sprites have a centered origin so we have to center the origin of our CircleCollider as well
				entity.collider = new CircleCollider( moonTexture.Width * 0.5f );
				entity.collider.originNormalized = new Vector2( 0.5f, 0.5f );
			}
			entity.position = new Vector2( 220, 220 );

			return scene;
		}

	}
}

