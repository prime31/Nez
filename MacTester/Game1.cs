using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tweens;
using Nez.Sprites;
using Nez.Analysis;
using Nez.Particles;


namespace MacTester
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Core
	{
		public Game1() : base( 256 * 4, 144 * 4 )
		{}


		protected override void Initialize()
		{
			base.Initialize();
		
			Window.AllowUserResizing = true;

			// prep IMGUI for use
			IMGUI.init( Graphics.instance.bitmapFont );
			scene = Scenes.sceneOne();
		}


		protected override void Update( GameTime gameTime )
		{
			base.Update( gameTime );

			if( Input.rightMouseButtonPressed )
			{
				Debug.log( "window size: {0} x {1}", Window.ClientBounds.Width, Window.ClientBounds.Height );
			}

			if( Input.leftMouseButtonPressed )
			{
				var screenPt = scene.camera.screenToWorldPoint( Input.scaledMousePosition );
				var worldPt = scene.camera.worldToScreenPoint( screenPt );
				Debug.log( "mouse pos: {0}, scaled mouse pos: {1}, screen to world point: {2}, world to screen point: {3}", Input.rawMousePosition, Input.scaledMousePosition, screenPt, worldPt );

				var spriteDude = scene.findEntity( "sprite-dude" );
				if( spriteDude != null )
				{
					spriteDude.getComponent<Sprite<int>>().pause();
					var worldPos = scene.camera.screenToWorldPoint( Input.scaledMousePosition );
					PropertyTweens.vector2PropertyTo( spriteDude, "position", worldPos, 0.5f )
						.setLoops( LoopType.PingPong, 1 )
						.setContext( spriteDude )
						.setCompletionHandler( tween =>
						{
							var sprite = (tween.context as Entity).getComponent<Sprite<int>>();

							// if the scene changed during the tween sprite will be null
							if( sprite != null )
								sprite.unPause();
						})
						.start();

					PropertyTweens.vector2PropertyTo( spriteDude.getComponent<RenderableComponent>(), "scale", new Vector2( 1.5f, 2.5f ), 1f )
						.setLoops( LoopType.PingPong, 1 )
						.start();

					PropertyTweens.floatPropertyTo( spriteDude.getComponent<RenderableComponent>(), "rotation", MathHelper.PiOver2, 1f )
						.setLoops( LoopType.PingPong, 1 )
						.start();
				}

				var playerDude = scene.findEntity( "player-moon" );
				if( playerDude != null )
				{
					var start = playerDude.position + new Vector2( 64f, 0f );
					var end = playerDude.position + new Vector2( 256f, 0f );
					Debug.drawLine( start, end, Color.Black, 2f );
					var hit = Physics.linecast( start, end );
					if( hit.collider != null )
					{
						Debug.log( "ray HIT {0}, collider: {1}", hit, hit.collider.entity );
					}
				}


				var cam = scene.findEntity( "camera-follow" );
				if( cam != null && cam.getComponent<CameraShake>() != null )
					cam.getComponent<CameraShake>().shake();


				var particles = scene.findEntity( "particles" );
				if( particles != null && particles.getComponent<ParticleEmitter>() != null )
				{
					if( particles.getComponent<ParticleEmitter>().isPlaying )
						particles.getComponent<ParticleEmitter>().pause();
					else
						particles.getComponent<ParticleEmitter>().play();
				}
			}

			// allow click-drag to move the camera
			if( Input.leftMouseButtonDown )
			{
				var deltaPos = Input.scaledMousePositionDelta;
				scene.camera.position -= deltaPos;
			}

			if( Input.mouseWheelDelta != 0 )
			{
				scene.camera.zoomIn( Input.mouseWheelDelta * 0.0001f );
			}
		}


		protected override void Draw( GameTime gameTime )
		{
			base.Draw( gameTime );

			IMGUI.beginWindow( GraphicsDevice.Viewport.Width - 150, 0, 150, 390 );

			debugRenderEnabled = IMGUI.toggle( "Debug Render", debugRenderEnabled );

			if( IMGUI.button( "Scene 1 ShowAll" ) )
				scene = Scenes.sceneOne( true );

			if( IMGUI.button( "Scene 1 NoBorder" ) )
				scene = Scenes.sceneOne( false );

			if( IMGUI.button( "Scene 1 Pixel Bloom" ) )
				scene = Scenes.sceneOneBloom();

			if( IMGUI.button( "Scene 2" ) )
				scene = Scenes.sceneTwo();

			if( IMGUI.button( "Scene 3 Box" ) )
				scene = Scenes.sceneThree( true );

			if( IMGUI.button( "Scene 3 Circle" ) )
				scene = Scenes.sceneThree( false );

			if( IMGUI.button( "Scene 4" ) )
				scene = Scenes.sceneFour();

			if( IMGUI.button( "Scene 5" ) )
				scene = Scenes.sceneFive();

			if( IMGUI.button( "Overlap2D Scene" ) )
				scene = Scenes.sceneOverlap2D();

			if( IMGUI.button( "Processor Scene" ) )
				scene = Scenes.processorScene();
			
			if( IMGUI.button( "Grab Screenshot" ) )
				scene.requestScreenshot( tex =>
				{
					var path = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), "screenshot.png" );
					using( var stream = new System.IO.FileStream( path, System.IO.FileMode.OpenOrCreate ) )
						tex.SaveAsPng( stream, tex.Width, tex.Height );
					tex.Dispose();
				} );

			IMGUI.endWindow();
		}
	}
}

