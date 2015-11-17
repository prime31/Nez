using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class ScalingViewportAdapter : ViewportAdapter
	{
		readonly protected int _virtualWidth;
		public override int virtualWidth
		{
			get { return _virtualWidth; }
		}

		readonly protected int _virtualHeight;
		public override int virtualHeight
		{
			get { return _virtualHeight; }
		}


		public ScalingViewportAdapter( GraphicsDevice graphicsDevice, int virtualWidth, int virtualHeight ) : base( graphicsDevice )
		{
			_virtualWidth = virtualWidth;
			_virtualHeight = virtualHeight;
			onGraphicsDeviceReset();
		}


		protected override void onGraphicsDeviceReset()
		{
			var scaleX = (float)viewportWidth / _virtualWidth;
			var scaleY = (float)viewportHeight / _virtualHeight;
			scaleMatrix = Matrix.CreateScale( scaleX, scaleY, 1.0f );
		}

	}
}

