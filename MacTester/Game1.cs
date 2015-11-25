#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tweens;
using Nez.Sprites;


#endregion

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
			IMGUI.init( GraphicsDevice, Graphics.instance.spriteFont );
			scene = Scenes.sceneOne();
		}


		protected override void LoadContent()
		{
			base.LoadContent();
		}


		protected override void Update( GameTime gameTime )
		{
			base.Update( gameTime );

			if( Input.leftMouseButtonPressed )
			{
				//Debug.log( "camera bounds: {0}, presention bounds: {1}, viewport: {2}", scene.camera.bounds, GraphicsDevice.PresentationParameters.Bounds.Size, graphicsDevice.Viewport.Bounds );

				var spriteDude = scene.findEntity( "sprite-dude" );
				if( spriteDude != null )
				{
					spriteDude.getComponent<Sprite<int>>().pause();
					var worldPos = scene.camera.screenToWorldPoint( Input.mousePosition );
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
				}

				var playerDude = scene.findEntity( "player-moon" );
				if( playerDude != null )
				{
					var start = playerDude.position + new Vector2( 64f, 0f );
					var end = playerDude.position + new Vector2( 128f, 0f );
					Debug.drawLine( start, end, Color.Black, 2f );
					var hit = Physics.raycast( start, end );
					if( hit.collider != null )
					{
						Debug.log( "ray HIT {0}, collider: {1}", hit.distance, hit.collider.entity );
					}
				}
			}

			// allow click-drag to move the camera
			if( Input.leftMouseButtonDown )
			{
				var deltaPos = Input.mousePositionDelta.ToVector2();

				// if we have a viewport adapter it may be scaling things so deal with that
				if( scene.camera.viewportAdapter != null )
				{
					deltaPos.X /= scene.camera.viewportAdapter.scaleMatrix.Scale.X;
					deltaPos.Y /= scene.camera.viewportAdapter.scaleMatrix.Scale.Y;
				}

				scene.camera.position -= deltaPos;
			}

			if( Input.mouseWheelDelta != 0 )
			{
				scene.camera.move( new Vector2( Input.mouseWheelDelta * 0.001f, Input.mouseWheelDelta * 0.001f ) );
			}
		}


		protected override void Draw( GameTime gameTime )
		{
			base.Draw( gameTime );

			IMGUI.beginWindow( GraphicsDevice.Viewport.Width - 150, 0, 150, 300 );

			debugRenderEnabled = IMGUI.toggle( "Debug Render", debugRenderEnabled );

			if( IMGUI.button( "Scene 1 Scaling" ) )
				scene = Scenes.sceneOne( true );

			if( IMGUI.button( "Scene 1 Boxing" ) )
				scene = Scenes.sceneOne( false );

			if( IMGUI.button( "Scene 2" ) )
				scene = Scenes.sceneTwo();

			if( IMGUI.button( "Scene 3 Box" ) )
				scene = Scenes.sceneThree( true );

			if( IMGUI.button( "Scene 3 Circle" ) )
				scene = Scenes.sceneThree( false );

			IMGUI.endWindow();			
		}
	}
}

