using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


namespace Nez
{
	public class FramesPerSecondCounter : TextComponent, IUpdatable
	{
		public enum FPSDockPosition
		{
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight
		}

		public long TotalFrames;
		public float AverageFramesPerSecond;
		public float CurrentFramesPerSecond;

		/// <summary>
		/// total number of samples that should be stored and averaged for calculating the FPS
		/// </summary>
		public int MaximumSamples;


		/// <summary>
		/// position the FPS counter should be docked
		/// </summary>
		/// <value>The dock position.</value>
		public FPSDockPosition DockPosition
		{
			get => _dockPosition;
			set
			{
				_dockPosition = value;
				UpdateTextPosition();
			}
		}

		/// <summary>
		/// offset from dockPosition the FPS counter should be drawn
		/// </summary>
		/// <value>The dock offset.</value>
		public Vector2 DockOffset
		{
			get => _dockOffset;
			set
			{
				_dockOffset = value;
				UpdateTextPosition();
			}
		}

		FPSDockPosition _dockPosition;
		Vector2 _dockOffset;
		readonly Queue<float> _sampleBuffer = new Queue<float>();


		public FramesPerSecondCounter() : this(Graphics.Instance.BitmapFont, Color.White)
		{ }

		public FramesPerSecondCounter(BitmapFont font, Color color,
		                              FPSDockPosition dockPosition = FPSDockPosition.TopRight, int maximumSamples = 100)
			: base(font, string.Empty, Vector2.Zero, color)
		{
			MaximumSamples = maximumSamples;
			DockPosition = dockPosition;
			Init();
		}

		public FramesPerSecondCounter(NezSpriteFont font, Color color,
		                              FPSDockPosition dockPosition = FPSDockPosition.TopRight, int maximumSamples = 100)
			: base(font, string.Empty, Vector2.Zero, color)
		{
			MaximumSamples = maximumSamples;
			DockPosition = dockPosition;
			Init();
		}

		void Init()
		{
			UpdateTextPosition();
		}

		void UpdateTextPosition()
		{
			switch (DockPosition)
			{
				case FPSDockPosition.TopLeft:
					_horizontalAlign = HorizontalAlign.Left;
					_verticalAlign = VerticalAlign.Top;
					LocalOffset = DockOffset;
					break;
				case FPSDockPosition.TopRight:
					_horizontalAlign = HorizontalAlign.Right;
					_verticalAlign = VerticalAlign.Top;
					LocalOffset = new Vector2(Core.GraphicsDevice.Viewport.Width - DockOffset.X, DockOffset.Y);
					break;
				case FPSDockPosition.BottomLeft:
					_horizontalAlign = HorizontalAlign.Left;
					_verticalAlign = VerticalAlign.Bottom;
					LocalOffset = new Vector2(DockOffset.X, Core.GraphicsDevice.Viewport.Height - DockOffset.Y);
					break;
				case FPSDockPosition.BottomRight:
					_horizontalAlign = HorizontalAlign.Right;
					_verticalAlign = VerticalAlign.Bottom;
					LocalOffset = new Vector2(Core.GraphicsDevice.Viewport.Width - DockOffset.X,
						Core.GraphicsDevice.Viewport.Height - DockOffset.Y);
					break;
			}
		}

		public void Reset()
		{
			TotalFrames = 0;
			_sampleBuffer.Clear();
		}

		public virtual void Update()
		{
			if (Time.UnscaledDeltaTime == 0.0f)
				return;
			CurrentFramesPerSecond = 1.0f / Time.UnscaledDeltaTime;
			_sampleBuffer.Enqueue(CurrentFramesPerSecond);

			if (_sampleBuffer.Count > MaximumSamples)
			{
				_sampleBuffer.Dequeue();
				AverageFramesPerSecond = _sampleBuffer.Average(i => i);
			}
			else
			{
				AverageFramesPerSecond = CurrentFramesPerSecond;
			}

			TotalFrames++;

			Text = string.Format("FPS: {0:0.00}", AverageFramesPerSecond);
		}

		public override bool IsVisibleFromCamera(Camera camera)
		{
			return true;
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			// we override render and use position instead of entityPosition. this keeps the text in place even if the entity moves
			batcher.DrawString(_font, _text, camera.ScreenToWorldPoint(_localOffset), Color, Entity.Transform.Rotation, Origin,
			 	Entity.Transform.Scale/camera.RawZoom, SpriteEffects, LayerDepth);
				
		}

		public override void DebugRender(Batcher batcher)
		{
			// due to the override of position in render we have to do the same here
			var rect = Bounds;
			rect.Location = LocalOffset;
			batcher.DrawHollowRect(rect, Color.Yellow);
		}


		#region Fluent setters

		/// <summary>
		/// Sets how far the fps text will appear from the edges of the screen.
		/// </summary>
		/// <param name="dockOffset">Offset from screen edges</param>
		public FramesPerSecondCounter SetDockOffset(Vector2 dockOffset)
		{
			DockOffset = dockOffset;
			return this;
		}

		/// <summary>
		/// Sets which corner of the screen the fps text will show.
		/// </summary>
		/// <param name="dockPosition">Corner of the screen</param>
		public FramesPerSecondCounter SetDockPosition(FPSDockPosition dockPosition)
		{
			DockPosition = dockPosition;
			return this;
		}

		#endregion
	}
}