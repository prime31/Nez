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
			//Debug.log( "w: {0}, h: {1}", Core.instance._graphicsManager.PreferredBackBufferWidth, Core.instance._graphicsManager.PreferredBackBufferHeight );

			var vp = Core.graphicsDevice.Viewport;
			vp.Bounds = Core.graphicsDevice.PresentationParameters.Bounds;
			Core.graphicsDevice.Viewport = vp;
			Debug.log( "new viewport: {0}", Core.graphicsDevice.Viewport );

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
			entity.addComponent( new FramesPerSecondCounter( Graphics.defaultGraphics.defaultFont, Color.White, FramesPerSecondCounter.FPSDockPosition.TopLeft ) );
			entity.position.Y = 120;

			entity = scene.createAndAddEntity<Entity>( "new-moon" );
			image = new Image( scene.contentManager.Load<Texture2D>( "Images/moon" ) );
			entity.position.X = 130;
			entity.position.Y = 130;
			entity.addComponent( image );

			// add a post processor to display the RenderTexture
			var postProcessor = new SimplePostProcessor( renderer.renderTexture );
			scene.addPostProcessStep( postProcessor );
			scene.enablePostProcessing = true;

			return scene;
		}
	}
}

