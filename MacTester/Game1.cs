#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tweens;


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
				Debug.log( "camera bounds: {0}, presention bounds: {1}, viewport: {2}", scene.camera.bounds, GraphicsDevice.PresentationParameters.Bounds.Size, graphicsDevice.Viewport.Bounds );

				var spriteDude = scene.findEntity( "sprite-dude" );
				if( spriteDude != null )
				{
					var worldPos = scene.camera.screenToWorldPoint( Input.mousePosition );
					PropertyTweens.vector2PropertyTo( spriteDude, "position", worldPos, 0.5f ).setLoops( LoopType.PingPong, 1 ).start();
				}
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

			enableDebugRender = IMGUI.toggle( "Debug Render", enableDebugRender );

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

