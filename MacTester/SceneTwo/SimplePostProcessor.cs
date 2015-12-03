using System;
using Nez;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


namespace MacTester
{
	public class SimplePostProcessor : PostProcessor
	{
		public Rectangle sourceRect;

		RenderTexture _renderTexture;
		Effect _effect;


		public SimplePostProcessor( RenderTexture renderTexture, Effect effect )
		{
			_renderTexture = renderTexture;
			_effect = effect;
			sourceRect = new Rectangle( 250, 10, _renderTexture.textureBounds.Width * 2, _renderTexture.textureBounds.Height * 2 );
		}


		public override void process()
		{
			Graphics.instance.spriteBatch.Begin( effect: _effect );
			Graphics.instance.spriteBatch.Draw( _renderTexture.texture2D, null, sourceRect );
			Graphics.instance.spriteBatch.End();
		}

	}
}

