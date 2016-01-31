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
using System.Collections.Generic;
using System.IO;
using Nez.Particles;
using Nez.PhysicsShapes;
using System.Linq;
using Nez.Tweens;


namespace MacTester
{
	public static class Scenes
	{
		public static Scene sceneOne( bool showAll = true )
		{
			var scene = Scene.createWithDefaultRenderer( Color.Black );
			scene.letterboxColor = Color.MonoGameOrange;

			if( showAll )
				scene.setDesignResolution( 256, 144, Scene.SceneResolutionPolicy.ShowAllPixelPerfect );
			else
				scene.setDesignResolution( 256, 144, Scene.SceneResolutionPolicy.NoBorderPixelPerfect );

			// load a TiledMap and move it back so is drawn before other entities
			var tiledEntity = scene.createEntity( "tiled-map-entity" );
			var tiledmap = scene.contentManager.Load<TiledMap>( "bin/MacOSX/Tilemap/tilemap" );
			tiledEntity.addComponent( new TiledMapComponent( tiledmap, "collision" ) );
			tiledEntity.updateOrder += 5;

			var tiledEntityTwo = scene.createEntity( "tiled-map-entity-two" );
			tiledEntityTwo.transform.position = new Vector2( 256, 0 );
			tiledEntityTwo.addComponent( new TiledMapComponent( tiledmap, "collision" ) );
			tiledEntityTwo.updateOrder += 5;

			// create a sprite animation from an atlas
			var plumeTexture = scene.contentManager.Load<Texture2D>( "Images/plume" );
			var subtextures = Subtexture.subtexturesFromAtlas( plumeTexture, 16, 16 );
			var spriteAnimation = new SpriteAnimation( subtextures ) {
				loop = true,
				fps = 10
			};

			var sprite = new Sprite<int>( subtextures[0] );
			sprite.addAnimation( 0, spriteAnimation );
			sprite.play( 0 );

			var spriteEntity = scene.createEntity( "sprite-dude" );
			spriteEntity.transform.position = new Vector2( 40, 40 );
			spriteEntity.addComponent( sprite );


			scene.finalRenderDelegate = new PixelMosaicRenderDelegate();

			return scene;
		}


		public static Scene sceneOneBloom()
		{
			var scene = new Scene();
			var bloomLayerRenderer = scene.addRenderer( new RenderLayerRenderer( -1, 1 ) );
			bloomLayerRenderer.renderTarget = RenderTarget.create( 256, 144 );
			bloomLayerRenderer.renderTargetClearColor = Color.Transparent;

			scene.addRenderer( new RenderLayerExcludeRenderer( 0, 1 ) );
			scene.letterboxColor = Color.MonoGameOrange;
			scene.setDesignResolution( 256, 144, Scene.SceneResolutionPolicy.ShowAllPixelPerfect );

			// load a TiledMap and move it back so is drawn before other entities
			var tiledEntity = scene.createEntity( "tiled-map-entity" );
			var tiledmap = scene.contentManager.Load<TiledMap>( "bin/MacOSX/Tilemap/tilemap" );

			var tc1 = new TiledMapComponent( tiledmap, "collision" );
			tc1.layerIndicesToRender = new int[] { 0, 1, 2 };
			tiledEntity.addComponent( tc1 );


			var tc2 = new TiledMapComponent( tiledmap );
			tiledEntity.addComponent( tc2 );
			tc2.renderLayer = 1;
			tc2.layerIndicesToRender = new int[] { 3 };


			scene.addPostProcessor( new PixelBloomPostProcessor( bloomLayerRenderer.renderTarget, -1 ) );

			return scene;
		}


		public static Scene sceneTwo()
		{
			var scene = new Scene();
			scene.clearColor = Color.Coral;
			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );
			var bmFont = scene.contentManager.Load<BitmapFont>( "bin/MacOSX/Fonts/pixelfont" );
			bmFont.spacing = 2f;

			// setup a renderer that renders everything to a RenderTarget making sure its order is before standard renderers!
			var renderer = new DefaultRenderer( -1 );
			renderer.renderTarget = RenderTarget.create( 320, 240 );
			renderer.renderTargetClearColor = Color.CornflowerBlue;
			scene.addRenderer( renderer );

			// add a standard renderer that renders to the screen
			scene.addRenderer( new DefaultRenderer() );

			// stick a couple moons on screen
			var entity = scene.createEntity( "moon" );
			var image = new Sprite( moonTexture );
			entity.transform.scale = new Vector2( 2 );
			entity.addComponent( image );
			entity.addComponent( new FramesPerSecondCounter( Graphics.instance.bitmapFont, Color.White, FramesPerSecondCounter.FPSDockPosition.TopLeft ) );
			entity.transform.position = new Vector2( 120f, 0f );
			entity.colliders.add( new CircleCollider( moonTexture.Width * 1.5f ) );

			entity = scene.createEntity( "new-moon" );
			image = new Sprite( moonTexture );
			entity.transform.position = new Vector2( 130f, 230f );
			entity.addComponent( image );


			entity = scene.createEntity( "bmfont" );
			entity.addComponent( new Text( Graphics.instance.bitmapFont, "This text is a BMFont\nPOOOP", new Vector2( 0, 30 ), Color.Red ) );
			entity.addComponent( new Text( bmFont, "This text is a BMFont\nPOOOP", new Vector2( 0, 70 ), Color.Cornsilk ) );


			// texture atlas tester
			var anotherAtlas = scene.contentManager.Load<TextureAtlas>( "bin/MacOSX/TextureAtlasTest/AnotherAtlas" );
			var textureAtlas = scene.contentManager.Load<TextureAtlas>( "bin/MacOSX/TextureAtlasTest/AtlasImages" );

			entity = scene.createEntity( "texture-atlas-sprite" );
			entity.transform.position = new Vector2( 30f, 330f );

			// create a sprite animation from an atlas
			var spriteAnimation = new SpriteAnimation() {
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
			entity.addComponent( sprite )
				.addComponent( new SimpleMoonMover() )
				.addComponent( new SpriteTrail( sprite ) );
			entity.getComponent<SpriteTrail>().enableSpriteTrail();


			// add a post processor to display the RenderTarget
			var effect = scene.contentManager.LoadEffect( "Content/Effects/Invert.ogl.mgfxo" );
			var postProcessor = new SimplePostProcessor( renderer.renderTarget, effect );
			scene.addPostProcessor( postProcessor );

			return scene;
		}


		public static Scene sceneThree( bool useBoxColliders = true )
		{
			var scene = Scene.createWithDefaultRenderer( useBoxColliders ? Color.BlanchedAlmond : Color.Azure );
			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );

			// create some moons
			Action<Vector2,string,bool> moonMaker = ( Vector2 pos, string name, bool isTrigger ) =>
			{
				var ent = scene.createEntity( name );
				ent.transform.position = pos;
				ent.addComponent( new Sprite( moonTexture ) );
				if( useBoxColliders )
					ent.colliders.add( new BoxCollider() );
				else
					ent.colliders.add( new CircleCollider() );

				ent.colliders[0].isTrigger = isTrigger;
			};

			moonMaker( new Vector2( 400, 10 ), "moon1", false );
			moonMaker( new Vector2( 0, 0 ), "moon2", false );
			moonMaker( new Vector2( 50, 500 ), "moon3", true );
			moonMaker( new Vector2( 500, 250 ), "moon4", false );


			// add an animation to "moon4" to test moving collisions
			scene.findEntity( "moon4" ).addComponent( new SimpleMovingPlatform( 250, 400 ) );

			// create a player moon
			var entity = scene.createEntity( "player-moon" );
			entity.transform.position = new Vector2( 220, 220 );
			var sprite = new Sprite( moonTexture );
			entity.addComponent( sprite )
				.addComponent( new SimpleMoonOutlineRenderer( sprite ) )
				.addComponent( new SimpleMoonMover() );

			if( useBoxColliders )
				entity.colliders.add( new BoxCollider() );
			else
				entity.colliders.add( new CircleCollider() );


			// add a follow camera
			var camFollow = scene.createEntity( "camera-follow" );
			camFollow.addComponent( new FollowCamera( entity ) )
				.addComponent( new CameraShake() );

			return scene;
		}


		public static Scene sceneOverlap2D()
		{
			var scene = Scene.createWithDefaultRenderer( Color.Aquamarine );

			var sceneEntity = scene.createEntity( "overlap2d-scene-entity" );
			var o2ds = scene.contentManager.Load<O2DScene>( "bin/MacOSX/Overlap2D/MainScene" );
			var sceneTexture = scene.contentManager.Load<LibGdxAtlas>( "bin/MacOSX/Overlap2D/packatlas" );
			foreach( var si in o2ds.sImages )
			{
				var i = new Sprite( sceneTexture.getSubtexture( si.imageName ) );
				i.localPosition = new Vector2( si.x, -si.y );
				i.origin = new Vector2( si.originX, si.originY );
				sceneEntity.transform.scale = new Vector2( si.scaleX, si.scaleY );
				sceneEntity.addComponent( i );
			}


			var effect = scene.contentManager.LoadNezEffect<CrosshatchEffect>();
			//var effect = scene.contentManager.LoadNezEffect<NoiseEffect>();
			//var effect = scene.contentManager.LoadNezEffect<TwistEffect>();
			//var effect = scene.contentManager.LoadNezEffect<SpriteBlinkEffect>();
			//effect.blinkColor = new Color( 255, 0, 0, 128 );

			scene.addPostProcessor( new PostProcessor( 1, effect ) );

			return scene;
		}


		public static Scene sceneFour()
		{
			var scene = Scene.createWithDefaultRenderer( Color.Aquamarine );
			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );

			var entity = scene.createEntity( "moon" );
			entity.addComponent( new ScrollingSprite( moonTexture )
			{
				scrollSpeedX = 75f,
				scrollSpeedY = 75f
			});
			entity.transform.position = new Vector2( 200, 200 );
			//entity.colliders.add( new PolygonCollider( 5, 100 ) );
			entity.colliders.add( new BoxCollider() );


			entity = scene.createEntity( "moon2" );
			var image = new Sprite( moonTexture );
			entity.addComponent( image );
			entity.addComponent( new SimpleMoonMover() );
			entity.transform.position = new Vector2( 500, 500 );
			//entity.colliders.add( new PolygonCollider( 7, 60 ) );
			//entity.colliders.add( new BoxCollider() );
			entity.colliders.add( new CircleCollider() );


			scene.addPostProcessor( new ScanlinesPostProcessor( 0 ) );
			scene.addPostProcessor( new VignettePostProcessor( 1 ) );

			return scene;
		}


		static int lastEmitter = 0;
		public static Scene sceneFive()
		{
			var scene = Scene.createWithDefaultRenderer( Color.Black );

			var particles = new string[] {
				"bin/MacOSX/ParticleDesigner/Fire",
				"bin/MacOSX/ParticleDesigner/Snow",
				"bin/MacOSX/ParticleDesigner/Leaves",
				"bin/MacOSX/ParticleDesigner/Atomic Bubble",
				"bin/MacOSX/ParticleDesigner/Blue Flame",
				"bin/MacOSX/ParticleDesigner/Blue Galaxy",
				"bin/MacOSX/ParticleDesigner/Comet",
				"bin/MacOSX/ParticleDesigner/Crazy Blue",
				"bin/MacOSX/ParticleDesigner/Electrons",
				"bin/MacOSX/ParticleDesigner/Giros Gratis",
				"bin/MacOSX/ParticleDesigner/huo1",
				"bin/MacOSX/ParticleDesigner/Into The Blue",
				"bin/MacOSX/ParticleDesigner/JasonChoi_Flash",
				"bin/MacOSX/ParticleDesigner/JasonChoi_rising up",
				"bin/MacOSX/ParticleDesigner/JasonChoi_Swirl01",
				"bin/MacOSX/ParticleDesigner/Meks Blood Spill",
				"bin/MacOSX/ParticleDesigner/Plasma Glow",
				"bin/MacOSX/ParticleDesigner/Real Popcorn",
				"bin/MacOSX/ParticleDesigner/Shooting Fireball",
				"bin/MacOSX/ParticleDesigner/The Sun",
				"bin/MacOSX/ParticleDesigner/Touch Up",
				"bin/MacOSX/ParticleDesigner/Trippy",
				"bin/MacOSX/ParticleDesigner/Winner Stars",
				"bin/MacOSX/ParticleDesigner/wu1"
			};

			if( lastEmitter == particles.Length )
				lastEmitter = 0;
			var whichEmitter = particles[lastEmitter++];


			var entity = scene.createEntity( "particles" );
			entity.transform.position = new Vector2( Screen.backBufferWidth / 2, Screen.backBufferHeight / 2 );
			var particleEmitterConfig = scene.contentManager.Load<ParticleEmitterConfig>( whichEmitter );
			entity.addComponent( new ParticleEmitter( particleEmitterConfig ) );
			entity.getComponent<ParticleEmitter>().collisionConfig.enabled = true;


			entity = scene.createEntity( "text" );
			var textComp = new Text( Graphics.instance.bitmapFont, whichEmitter, new Vector2( 0, 0 ), Color.White );
			entity.transform.scale = new Vector2( 2, 2 );
			textComp.origin = Vector2.Zero;
			entity.addComponent( textComp );


			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );
			entity = scene.createEntity( "moon1" );
			entity.transform.position = new Vector2( Screen.backBufferWidth / 2, Screen.backBufferHeight / 2 + 100 );
			entity.addComponent( new Sprite( moonTexture ) );
			entity.colliders.add( new CircleCollider() );

			entity = scene.createEntity( "moon2" );
			entity.transform.position = new Vector2( Screen.backBufferWidth / 2 - 100, Screen.backBufferHeight / 2 + 100 );
			entity.addComponent( new Sprite( moonTexture ) );
			entity.colliders.add( new CircleCollider() );

			return scene;
		}


		public static Scene processorScene()
		{
			var scene = Scene.createWithDefaultRenderer( Color.Black );
			scene.camera.position += new Vector2( -Screen.backBufferWidth / 2, -Screen.backBufferHeight / 2 );

			var entity = scene.createEntity( "text" );
			var textComp = new Text( Graphics.instance.bitmapFont, "text only", new Vector2( 0, 0 ), Color.White );
			entity.transform.scale = new Vector2( 2, 2 );
			textComp.origin = Vector2.Zero;
			textComp.localPosition = new Vector2( 120, -150 );
			entity.addComponent( textComp );

			entity = scene.createEntity( "text-image" );
			textComp = new Text( Graphics.instance.bitmapFont, "text and image", new Vector2( 0, 20 ), Color.White );
			entity.transform.scale = new Vector2( 2, 2 );
			textComp.origin = Vector2.Zero;
			entity.addComponent( textComp );

			var moonTexture = scene.contentManager.Load<Texture2D>( "Images/moon" );
			var image = new Sprite( moonTexture );
			image.localPosition = new Vector2( -80, 50 );
			entity.addComponent( image );
			entity.colliders.add( new CircleCollider() );


			var m = new Matcher().all( typeof( Text ) );
			var tp = new TextEntityProcessor( m );
			scene.entityProcessors.add( tp );

			m = new Matcher().all( typeof( Sprite ) );
			var ip = new ImageEntityProcessor( m );
			scene.entityProcessors.add( ip );

			Debug.log( tp.matcher.ToString() );
			Debug.log( ip.matcher.ToString() );

			return scene;
		}
	}


	public class TransformScene : Scene
	{
		public override void initialize()
		{
			var textureAtlas = contentManager.Load<TextureAtlas>( "bin/MacOSX/TextureAtlasTest/AtlasImages" );

			var parentEntity = createEntity( "parent" );
			parentEntity.transform.position = new Vector2( 300, 300 );
			parentEntity.transform.rotation = MathHelper.PiOver4;
			parentEntity.addComponent( new Sprite( textureAtlas.getSubtexture( "Ninja_Idle_0" ) ) );


			var childEntity = createEntity( "child" );
			childEntity.transform.parent = parentEntity.transform;
			childEntity.transform.localPosition = new Vector2( 50, 0 );
			childEntity.transform.scale = new Vector2( 2, 2 );
			childEntity.transform.rotation = 0.5f;
			childEntity.addComponent( new Sprite( textureAtlas.getSubtexture( "Ninja_Idle_1" ) ) );


			var childTwoEntity = createEntity( "childTwo" );
			childTwoEntity.transform.parent = childEntity.transform;
			childTwoEntity.addComponent( new Sprite( textureAtlas.getSubtexture( "Ninja_Idle_2" ) ) );
			childTwoEntity.transform.localPosition = new Vector2( 50, 0 );
			childTwoEntity.transform.rotation = 0.5f;


			PropertyTweens.floatPropertyTo( parentEntity.transform, "rotation", MathHelper.TwoPi, 1 ).start();
			//PropertyTweens.floatPropertyTo( childTwoEntity.transform, "rotation", MathHelper.TwoPi, 1 ).start();
		}
	}

}

