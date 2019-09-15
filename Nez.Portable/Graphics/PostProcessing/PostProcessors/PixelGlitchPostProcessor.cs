using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// glitch effect where the screen is divided into rows verticalSize high. Each row is shifted horizonalAmount left or right. It is best used
	/// by changing horizontalOffset every few frames for a second then going back to normal.
	/// </summary>
	public class PixelGlitchPostProcessor : PostProcessor
	{
		/// <summary>
		/// vertical size in pixels or each row. default 5.0
		/// </summary>
		/// <value>The size of the vertical.</value>
		public float VerticalSize
		{
			get => _verticalSize;
			set
			{
				if (_verticalSize != value)
				{
					_verticalSize = value;

					if (Effect != null)
						_verticalSizeParam.SetValue(_verticalSize);
				}
			}
		}

		/// <summary>
		/// horizontal shift in pixels. default 10.0
		/// </summary>
		/// <value>The horizontal offset.</value>
		public float HorizontalOffset
		{
			get => _horizontalOffset;
			set
			{
				if (_horizontalOffset != value)
				{
					_horizontalOffset = value;

					if (Effect != null)
						_horizontalOffsetParam.SetValue(_horizontalOffset);
				}
			}
		}

		float _verticalSize = 5f;
		float _horizontalOffset = 10f;
		EffectParameter _verticalSizeParam;
		EffectParameter _horizontalOffsetParam;
		EffectParameter _screenSizeParam;


		public PixelGlitchPostProcessor(int executionOrder) : base(executionOrder)
		{
		}

		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);
			Effect = scene.Content.LoadEffect<Effect>("pixelGlitch", EffectResource.PixelGlitchBytes);

			_verticalSizeParam = Effect.Parameters["_verticalSize"];
			_horizontalOffsetParam = Effect.Parameters["_horizontalOffset"];
			_screenSizeParam = Effect.Parameters["_screenSize"];

			_verticalSizeParam.SetValue(_verticalSize);
			_horizontalOffsetParam.SetValue(_horizontalOffset);
			_screenSizeParam.SetValue(new Vector2(Screen.Width, Screen.Height));
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(Effect);
			base.Unload();
		}

		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			_screenSizeParam.SetValue(new Vector2(newWidth, newHeight));
		}
	}
}