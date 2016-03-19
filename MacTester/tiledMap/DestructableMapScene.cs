using System;
using Nez;
using Nez.Tiled;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.Sprites;


namespace MacTester
{
	public class DestructableMapScene : Scene
	{
		public override void initialize()
		{
			clearColor = Color.Black;
			addRenderer( new DefaultRenderer() );
			setDesignResolution( 1280 / 2, 720 / 2 + 8, Scene.SceneResolutionPolicy.ShowAllPixelPerfect );

			// load a TiledMap and move it back so is drawn before other entities
			var tiledEntity = createEntity( "tiled-map" );
			var tiledmap = contentManager.Load<TiledMap>( "bin/MacOSX/Tilemap/destructable-map" );
			tiledEntity.addComponent( new TiledMapComponent( tiledmap, "main" ) );

			var objects = tiledmap.getObjectGroup( "objects" );
			var spawn = objects.objectWithName( "spawn" );
			var ball = objects.objectWithName( "ball" );

			var atlas = contentManager.Load<Texture2D>( "bin/MacOSX/Tilemap/desert-palace-tiles2x" );
			var atlasParts = Subtexture.subtexturesFromAtlas( atlas, 16, 16 );
			var playerSubtexture = atlasParts[96];

			var playerEntity = createEntity( "player" );
			playerEntity.transform.position = new Vector2( spawn.x + 8, spawn.y + 8 );
			playerEntity.addComponent( new Sprite( playerSubtexture ) );
			playerEntity.addComponent( new PlayerDashMover() );
			playerEntity.addComponent( new CameraShake() );
			playerEntity.addComponent( new Nez.Shadows.PointLight( 100 )
			{
				collidesWithLayers = 1 << 0,
				color = Color.MonoGameOrange * 0.5f
			});

			var trail = playerEntity.addComponent( new SpriteTrail( playerEntity.getComponent<Sprite>() ) );
			trail.fadeDelay = 0;
			trail.fadeDuration = 0.2f;
			trail.minDistanceBetweenInstances = 10f;
			trail.initialColor = Color.White * 0.5f;
			playerEntity.colliders.add( new BoxCollider() );
			playerEntity.colliders.mainCollider.physicsLayer = 1 << 2;
			playerEntity.colliders.mainCollider.collidesWithLayers = 1 << 0;

			var ballSubtexture = atlasParts[atlasParts.Count - 26 + 8];
			var ballEntity = createEntity( "ball" );
			ballEntity.transform.position = new Vector2( ball.x, ball.y );
			ballEntity.addComponent( new Sprite( ballSubtexture ) );
			ballEntity.addComponent( new ArcadeRigidbody() );
			ballEntity.colliders.add( new CircleCollider() );
			ballEntity.colliders.mainCollider.physicsLayer = 1 << 1;
			ballEntity.colliders.mainCollider.collidesWithLayers = 1 << 0;
		}
	}
}

