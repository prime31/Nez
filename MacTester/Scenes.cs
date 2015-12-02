using System;
using Nez;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Textures;
using Nez.Tiled;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Overlap2D;
using Nez.LibGdxAtlases;
using Nez.BitmapFonts;


namespace MacTester
{
	public static class Scenes
	{
		public static Scene sceneOne( bool useScalingViewportAdapter = true )
		{
			var scene = Scene.createWithDefaultRenderer( Color.Black );

			if( useScalingViewportAdapter )
				scene.camera.viewportAdapter = new ScalingViewportAdapter( Core.graphicsDevice, 256, 144 );
			else
				scene.camera.viewportAdapter = new BoxingViewportAdapter( Core.graphicsDevice, 256, 144 );
			Core.setScreenSize( 256 * 4, 144 * 4 );


			// load a TiledMap and move it back so is drawn before other entities
			var tiledEntity = scene.createAndAddEntity<Entity>( "tiled-map-entity" );
			var tiledmap = scene.contentManager.Load<TiledMap>( "bin/MacOSX/Tilemap/tilemap" );
			tiledEntity.addComponent( new TiledMapComponent( tiledmap, "collision" ) );
			tiledEntity.depth += 5;


			// create a sprite animation from an atlas
			var plumeTexture = scene.contentManager.Load<Texture2D>( "Images/plume" );
			var subtextures = Subtexture.subtexturesFromAtlas( plumeTexture, 16, 16 );
			var spriteAnimation = new SpriteAnimation( subtextures )
			{
				loop = true,
				fps = 10
			};

			var sprite = new Sprite<int>( subtextures[0] );
			sprite.addAnimation( 0, spriteAnimation );
			sprite.play( 0 );

			var spriteEntity = scene.createAndAddEntity<Entity>( "sprite-dude" );
			spriteEntity.position = new Vector2( 40, 40 );
			spriteEntity.addComponent( sprite );

			return scene;
		}


		public static Scene sceneTwo()
		{
			var scene = new Scene();
			scene.clearColor = Color.Coral;
			scene.camera.centerOrigin();
			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );
			var bmFont = scene.contentManager.Load<BitmapFont>( "bin/MacOSX/Fonts/pixelfont" );
			bmFont.spacing = 2f;

			// setup a renderer that renders everything to a RenderTexture making sure its order is before standard renderers!
			var renderer = new DefaultRenderer( scene.camera, -1 );
			renderer.renderTexture = new RenderTexture( Core.graphicsDevice, 320, 240 );
			renderer.renderTextureClearColor = Color.CornflowerBlue;
			scene.addRenderer( renderer );

			// add a standard renderer that renders to the screen
			scene.addRenderer( new DefaultRenderer() );

			// stick a couple moons on screen
			var entity = scene.createAndAddEntity<Entity>( "moon" );
			var image = new Image( moonTexture );
			image.originNormalized = Vector2Extension.halfVector();
			image.zoom = 2f;
			entity.addComponent( image );
			entity.addComponent( new FramesPerSecondCounter( Graphics.instance.bitmapFont, Color.White, FramesPerSecondCounter.FPSDockPosition.TopLeft ) );
			entity.position = new Vector2( 120f, 0f );
			entity.collider = new CircleCollider( moonTexture.Width * 1.5f );


			entity = scene.createAndAddEntity<Entity>( "new-moon" );
			image = new Image( moonTexture );
			entity.position = new Vector2( 130f, 130f );
			entity.addComponent( image );


			entity = scene.createAndAddEntity<Entity>( "bmfont" );
			entity.addComponent( new Text( Graphics.instance.bitmapFont, "This text is a BMFont\nPOOOP", new Vector2( 0, 30 ), Color.White ) );
			entity.addComponent( new Text( bmFont, "This text is a BMFont\nPOOOP", new Vector2( 0, 70 ), Color.Black ) );


			// texture atlas tester
			var anotherAtlas = scene.contentManager.Load<TextureAtlas>( "bin/MacOSX/TextureAtlasTest/AnotherAtlas" );
			var textureAtlas = scene.contentManager.Load<TextureAtlas>( "bin/MacOSX/TextureAtlasTest/AtlasImages" );

			entity = scene.createAndAddEntity<Entity>( "texture-atlas-sprite" );
			entity.position = new Vector2( 30f, 330f );

			// create a sprite animation from an atlas
			var spriteAnimation = new SpriteAnimation()
			{
				loop = true,
				fps = 10
			};
			spriteAnimation.addFrame( textureAtlas.getSubtexture( "Ninja_Idle_0" ) );
			spriteAnimation.addFrame( textureAtlas.getSubtexture( "Ninja_Idle_1" ) );
			spriteAnimation.addFrame( textureAtlas.getSubtexture( "Ninja_Idle_2" ) );
			spriteAnimation.addFrame( textureAtlas.getSubtexture( "Ninja_Idle_3" ) );
			spriteAnimation.addFrame( anotherAtlas.getSubtexture( "Ninja_Air Dash_0" ) );
			spriteAnimation.addFrame( anotherAtlas.getSubtexture( "Ninja_Air Dash_1" ) );
			spriteAnimation.addFrame( anotherAtlas.getSubtexture( "Ninja_Air Dash_2" ) );
			spriteAnimation.addFrame( anotherAtlas.getSubtexture( "Ninja_Air Dash_3" ) );

			var sprite = new Sprite<int>( 1, anotherAtlas.getSpriteAnimation( "hardLanding" ) );
			sprite.addAnimation( 0, spriteAnimation );
			sprite.play( 1 );
			entity.addComponent( sprite );

			entity = scene.createAndAddEntity<Entity>( "particles" );
			entity.addComponent( new SimpleParticles() );


			// add a post processor to display the RenderTexture
			var postProcessor = new SimplePostProcessor( renderer.renderTexture );
			scene.addPostProcessStep( postProcessor );
			scene.enablePostProcessing = true;

			return scene;
		}


		public static Scene sceneThree( bool useBoxColliders = true )
		{
			var scene = Scene.createWithDefaultRenderer( useBoxColliders ? Color.BlanchedAlmond : Color.Azure );
			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );


			// create some moons
			Action<Vector2,string,bool> moonMaker = ( Vector2 pos, string name, bool isTrigger ) =>
			{
				var ent = scene.createAndAddEntity<Entity>( name );
				ent.position = pos;
				ent.addComponent( new Image( moonTexture ) );
				if( useBoxColliders )
					ent.collider = new BoxCollider();
				else
					ent.collider = new CircleCollider();

				ent.collider.isTrigger = isTrigger;
			};

			moonMaker( new Vector2( 400, 10 ), "moon1", false );
			moonMaker( new Vector2( 10, 10 ), "moon2", false );
			moonMaker( new Vector2( 50, 500 ), "moon3", true );
			moonMaker( new Vector2( 500, 250 ), "moon4", false );


			// add an animation to "moon4" to test moving collisions
			scene.findEntity( "moon4" ).addComponent( new SimpleMovingPlatform( 250, 400 ) );

			// create a player moon
			var entity = scene.createAndAddEntity<Entity>( "player-moon" );
			entity.addComponent( new SimpleMoonMover() );
			entity.position = new Vector2( 220, 220 );
			var sprite = new Sprite<int>( new Subtexture( moonTexture ) );
			sprite.color = Color.Blue;
			entity.addComponent( sprite );

			if( useBoxColliders )
				entity.collider = new BoxCollider();
			else
				entity.collider = new CircleCollider();


			var uglyBackgroundEntity = scene.createAndAddEntity<Entity>( "bg" );
			uglyBackgroundEntity.depth = 5;
			var image = new Image( scene.contentManager.Load<Texture2D>( "Images/dots-512" ) );
			image.zoom = 4f;
			uglyBackgroundEntity.addComponent( image );


			// add a follow camera
			var camFollow = scene.createAndAddEntity<Entity>( "camera-follow" );
			camFollow.addComponent( new FollowCamera( entity ) );

			return scene;
		}


		public static Scene sceneOverlap2D()
		{
			var scene = Scene.createWithDefaultRenderer( Color.Aquamarine );
			scene.camera.centerOrigin();

			var sceneEntity = scene.createAndAddEntity<Entity>( "overlap2d-scene-entity" );
			var o2ds = scene.contentManager.Load<O2DScene>( "bin/MacOSX/Overlap2D/MainScene" );
			var sceneTexture = scene.contentManager.Load<LibGdxAtlas>( "bin/MacOSX/Overlap2D/packatlas" );
			foreach( var si in o2ds.sImages )
			{
				try
				{
					var i = new Image( sceneTexture.getSubtexture( si.imageName ) );
					i.localPosition = new Vector2( si.x, -si.y );
					i.origin = new Vector2( si.originX, si.originY );
					i.scale = new Vector2( si.scaleX, si.scaleY );
					sceneEntity.addComponent( i );
				}
				catch( Exception )
				{
				}
			}


			return scene;
		}

	}
}

