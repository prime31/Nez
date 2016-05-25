using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


namespace Nez
{
	public class FramesPerSecondCounter : Text, IUpdatable
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

		/// <summary>
		/// total number of samples that should be stored and averaged for calculating the FPS
		/// </summary>
		public int maximumSamples;


		/// <summary>
		/// position the FPS counter should be docked
		/// </summary>
		/// <value>The dock position.</value>
		public FPSDockPosition dockPosition
		{
			get { return _dockPosition; }
			set
			{
				_dockPosition = value;
				updateTextPosition();
			}
		}

		/// <summary>
		/// offset from dockPosition the FPS counter should be drawn
		/// </summary>
		/// <value>The dock offset.</value>
		public Vector2 dockOffset
		{
			get { return _dockOffset; }
			set
			{
				_dockOffset = value;
				updateTextPosition();
			}
		}

		FPSDockPosition _dockPosition;
		Vector2 _dockOffset;
		readonly Queue<float> _sampleBuffer = new Queue<float>();


		public FramesPerSecondCounter( BitmapFont font, Color color, FPSDockPosition dockPosition = FPSDockPosition.TopRight, int maximumSamples = 100 ) : base( font, string.Empty, Vector2.Zero, color )
		{
			this.maximumSamples = maximumSamples;
			this.dockPosition = dockPosition;
			initialize();
		}


		public FramesPerSecondCounter( NezSpriteFont font, Color color, FPSDockPosition dockPosition = FPSDockPosition.TopRight, int maximumSamples = 100 ) : base( font, string.Empty, Vector2.Zero, color )
		{
			this.maximumSamples = maximumSamples;
			this.dockPosition = dockPosition;
			initialize();
		}


		void initialize()
		{
			updateTextPosition();
		}


		void updateTextPosition()
		{
			switch( dockPosition )
			{
				case FPSDockPosition.TopLeft:
					_horizontalAlign = HorizontalAlign.Left;
					_verticalAlign = VerticalAlign.Top;
					localOffset = dockOffset;
				break;
				case FPSDockPosition.TopRight:
					_horizontalAlign = HorizontalAlign.Right;
					_verticalAlign = VerticalAlign.Top;
					localOffset = new Vector2( Core.graphicsDevice.Viewport.Width - dockOffset.X, dockOffset.Y );
				break;
				case FPSDockPosition.BottomLeft:
					_horizontalAlign = HorizontalAlign.Left;
					_verticalAlign = VerticalAlign.Bottom;
					localOffset = new Vector2( dockOffset.X, Core.graphicsDevice.Viewport.Height - dockOffset.Y );
				break;
				case FPSDockPosition.BottomRight:
					_horizontalAlign = HorizontalAlign.Right;
					_verticalAlign = VerticalAlign.Bottom;
					localOffset = new Vector2( Core.graphicsDevice.Viewport.Width - dockOffset.X, Core.graphicsDevice.Viewport.Height - dockOffset.Y );
				break;
			}
		}


		public void reset()
		{
			totalFrames = 0;
			_sampleBuffer.Clear();
		}


		void IUpdatable.update()
		{
			currentFramesPerSecond = 1.0f / Time.unscaledDeltaTime;
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


		public override void render( Graphics graphics, Camera camera )
		{
			// we override render and use position instead of entityPosition. this keeps the text in place even if the entity moves
			graphics.batcher.drawString( _font, _text, localOffset, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, layerDepth );
		}


		public override void debugRender( Graphics graphics )
		{
			// due to the override of position in render we have to do the same here
			var rect = bounds;
			rect.location = localOffset;
			graphics.batcher.drawHollowRect( rect, Color.Yellow );
		}


		#region Fluent setters

		/// <summary>
		/// Sets how far the fps text will appear from the edges of the screen.
		/// </summary>
		/// <param name="dockOffset">Offset from screen edges</param>
		public FramesPerSecondCounter setDockOffset( Vector2 dockOffset )
		{
			this.dockOffset = dockOffset;
			return this;
		}


		/// <summary>
		/// Sets which corner of the screen the fps text will show.
		/// </summary>
		/// <param name="dockPosition">Corner of the screen</param>
		public FramesPerSecondCounter setDockPosition( FPSDockPosition dockPosition )
		{
			this.dockPosition = dockPosition;
			return this;
		}

		#endregion
	
	}
}
