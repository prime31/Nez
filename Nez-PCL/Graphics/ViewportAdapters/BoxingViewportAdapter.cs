using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class BoxingViewportAdapter : ScalingViewportAdapter
	{
		public BoxingViewportAdapter( GraphicsDevice graphicsDevice, int virtualWidth, int virtualHeight ) : base( graphicsDevice, virtualWidth, virtualHeight )
		{
			onGraphicsDeviceReset();
		}


		protected override void onGraphicsDeviceReset()
		{
			float screenWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
			float screenHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;
			int drawWidth, drawHeight;

			if( screenWidth / virtualWidth > screenHeight / virtualHeight )
			{
				drawWidth = (int)( screenHeight / virtualHeight * virtualWidth );
				drawHeight = (int)screenHeight;
			}
			else
			{
				drawWidth = (int)screenWidth;
				drawHeight = (int)( screenWidth / virtualWidth * virtualHeight );
			}


			var x = (int)( screenWidth / 2 ) - ( drawWidth / 2 );
			var y = (int)( screenHeight / 2 ) - ( drawHeight / 2 );
			_graphicsDevice.Viewport = new Viewport( x, y, drawWidth, drawHeight );

			scaleMatrix = Matrix.CreateScale( drawWidth / (float)virtualWidth );
		}


		public override Vector2 pointToVirtualViewport( Vector2 point )
		{
			point.X -= _graphicsDevice.Viewport.X;
			point.Y -= _graphicsDevice.Viewport.Y;

			return point;
		}


		public override Vector2 screenToVirtualViewport( Vector2 point )
		{
			point.X += _graphicsDevice.Viewport.X;
			point.Y += _graphicsDevice.Viewport.Y;

			return point;
		}

	}
}

