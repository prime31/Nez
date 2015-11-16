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

			scene.addRenderer( new DefaultRenderer() );
			scene.camera._viewportAdapter = new ScalingViewportAdapter( Core.graphicsDevice, 256, 144 );
			//Core.instance.setScreenSize( 256 * 4, 144 * 4 );


			// TiledMap and move it back so is drawn before other entities
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
			var renderer = new DefaultRenderer( -1 );
			renderer.renderTexture = new RenderTexture( Core.graphicsDevice, 320, 240 );
			renderer.renderTextureClearColor = Color.CornflowerBlue;
			scene.addRenderer( renderer );

			// add a standard renderer that renders to the screen
			scene.addRenderer( new DefaultRenderer() );

			// stick a couple moons on screen
			var entity = scene.createAndAddEntity<Entity>( "moon" );
			var image = new Image( Core.instance.Content.Load<Texture2D>( "Images/moon" ) );
			entity.addComponent( image );

			entity = scene.createAndAddEntity<Entity>( "new-moon" );
			image = new Image( Core.instance.Content.Load<Texture2D>( "Images/moon" ) );
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

