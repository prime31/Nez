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

		RenderTarget2D _renderTarget;


		public SimplePostProcessor( RenderTarget2D renderTarget, Effect effect ) : base( 0 )
		{
			_renderTarget = renderTarget;
			this.effect = effect;
			sourceRect = new Rectangle( 250, 10, _renderTarget.Bounds.Width * 2, _renderTarget.Bounds.Height * 2 );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination  )
		{
			Core.graphicsDevice.SetRenderTarget( destination );

			Graphics.instance.spriteBatch.Begin( effect: effect );
			Graphics.instance.spriteBatch.Draw( source, Vector2.Zero, Color.White );
			Graphics.instance.spriteBatch.Draw( _renderTarget, null, sourceRect );
			Graphics.instance.spriteBatch.End();
		}
	}
}

