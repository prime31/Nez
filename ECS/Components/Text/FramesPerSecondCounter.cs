using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class FramesPerSecondCounter : Text
	{
		public enum FPSDockPosition
		{
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight
		}

		public long totalFrames;
		public float averageFramesPerSecond;
		public float currentFramesPerSecond;
		public int maximumSamples;

		readonly Queue<float> _sampleBuffer = new Queue<float>();


		public FramesPerSecondCounter( SpriteFont font, Color color, FPSDockPosition dockPosition = FPSDockPosition.TopRight, int maximumSamples = 100 ) : base( font, string.Empty, Vector2.Zero, color )
		{
			this.maximumSamples = maximumSamples;

			switch( dockPosition )
			{
				case FPSDockPosition.TopLeft:
					_horizontalAlign = HorizontalAlign.Left;
					break;
				case FPSDockPosition.TopRight:
					_horizontalAlign = HorizontalAlign.Right;
					position.X = Core.instance.GraphicsDevice.Viewport.Width;
					position.Y = 0;
					break;
				case FPSDockPosition.BottomLeft:
					_horizontalAlign = HorizontalAlign.Left;
					position.X = 0;
					position.Y = Core.instance.GraphicsDevice.Viewport.Height;
					position = new Vector2( 0, Core.instance.GraphicsDevice.Viewport.Height );
					break;
				case FPSDockPosition.BottomRight:
					_horizontalAlign = HorizontalAlign.Right;
					position.X = Core.instance.GraphicsDevice.Viewport.Width;
					position.Y = Core.instance.GraphicsDevice.Viewport.Height;
					break;
			}
		}


		public void reset()
		{
			totalFrames = 0;
			_sampleBuffer.Clear();
		}


		public override void update()
		{
			currentFramesPerSecond = 1.0f / Core.deltaTime;
			_sampleBuffer.Enqueue( currentFramesPerSecond );

			if( _sampleBuffer.Count > maximumSamples )
			{
				_sampleBuffer.Dequeue();
				averageFramesPerSecond = _sampleBuffer.Average( i => i );
			}
			else
			{
				averageFramesPerSecond = currentFramesPerSecond;
			}

			totalFrames++;

			text = string.Format( "FPS: {0:0.00}", averageFramesPerSecond );
		}


		public override void render( Graphics graphics )
		{
			// we override render and use position instead of entityPosition. this keeps the text in place even if the entity moves
			graphics.spriteBatch.DrawString( font, text, position, color, rotation, origin, scale * zoom, spriteEffects, layerDepth );
		}

	}
}

