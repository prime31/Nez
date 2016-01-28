using System;
using Nez;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


namespace MacTester
{
	public class SimplePostProcessor : PostProcessor<Effect>
	{
		public Rectangle sourceRect;

		RenderTexture _renderTexture;


		public SimplePostProcessor( RenderTexture renderTexture, Effect effect ) : base( 0 )
		{
			_renderTexture = renderTexture;
			this.effect = effect;
			sourceRect = new Rectangle( 250, 10, _renderTexture.renderTarget2D.Bounds.Width * 2, _renderTexture.renderTarget2D.Bounds.Height * 2 );
		}


		public override void process( RenderTexture source, RenderTexture destination  )
		{
			Core.graphicsDevice.SetRenderTarget( destination );

			Graphics.instance.spriteBatch.Begin( effect: effect );
			Graphics.instance.spriteBatch.Draw( source, Vector2.Zero, Color.White );
			Graphics.instance.spriteBatch.Draw( _renderTexture, null, sourceRect );
			Graphics.instance.spriteBatch.End();
		}
	}
}

