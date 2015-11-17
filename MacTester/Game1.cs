using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections;


namespace MacTester
{
	public class Game1 : Core
	{
		public Game1() : base( 256 * 4, 144 * 4 )
		{}


		protected override void Initialize()
		{
			base.Initialize();

			var spriteFont = Content.Load<SpriteFont>( "bin/MacOSX/Fonts/DefaultFont" );
			Graphics.defaultGraphics.defaultFont = spriteFont;
			IMGUI.init( GraphicsDevice, spriteFont );

			scene = Scenes.sceneOne();
		}


		protected override void Update( GameTime gameTime )
		{
			base.Update( gameTime );

			if( Input.leftMouseButtonPressed )
			{
				Debug.log( "camera bounds: {0}", scene.camera.bounds );
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

			if( IMGUI.button( "Scene 1" ) )
				nextScene = Scenes.sceneOne();

			if( IMGUI.button( "Scene 2" ) )
				nextScene = Scenes.sceneTwo();

			IMGUI.endWindow();			
		}

	}
}

