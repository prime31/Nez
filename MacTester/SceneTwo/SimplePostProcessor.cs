using System;
using Nez;
using Nez.TextureAtlases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


namespace MacTester
{
	public class SimplePostProcessor : PostProcessor
	{
		RenderTexture _renderTexture;
		Effect _effect;


		public SimplePostProcessor( RenderTexture renderTexture )
		{
			_renderTexture = renderTexture;
			_effect = Core.instance.Content.Load<Effect>( "Effects/SpriteEffectEffect" );
			_effect = new Effect( Core.graphicsDevice, File.ReadAllBytes( "Content/Effects/Invert.ogl.mgfxo" ) );
		}


		public override void process()
		{
			Graphics.defaultGraphics.spriteBatch.Begin( effect: _effect );
			Graphics.defaultGraphics.spriteBatch.Draw( _renderTexture.texture2D, null, new Rectangle( 250, 10, _renderTexture.textureBounds.Width * 2, _renderTexture.textureBounds.Height * 2 ) );
			Graphics.defaultGraphics.spriteBatch.End();
		}

	}
}

