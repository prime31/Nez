using System;
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
		public float verticalSize
		{
			get { return _verticalSize; }
			set
			{
				if( _verticalSize != value )
				{
					_verticalSize = value;

					if( effect != null )
						_verticalSizeParam.SetValue( _verticalSize );
				}
			}
		}

		/// <summary>
		/// horizontal shift in pixels. default 10.0
		/// </summary>
		/// <value>The horizontal offset.</value>
		public float horizontalOffset
		{
			get { return _horizontalOffset; }
			set
			{
				if( _horizontalOffset != value )
				{
					_horizontalOffset = value;

					if( effect != null )
						_horizontalOffsetParam.SetValue( _horizontalOffset );
				}
			}
		}


		float _verticalSize = 5f;
		float _horizontalOffset = 10f;
		EffectParameter _verticalSizeParam;
		EffectParameter _horizontalOffsetParam;
		EffectParameter _screenSizeParam;


		public PixelGlitchPostProcessor( int executionOrder ) : base( executionOrder )
		{}


		public override void onAddedToScene()
		{
			effect = scene.contentManager.loadEffect<Effect>( "pixelGlitch", EffectResource.pixelGlitchBytes );

			_verticalSizeParam = effect.Parameters["_verticalSize"];
			_horizontalOffsetParam = effect.Parameters["_horizontalOffset"];
			_screenSizeParam = effect.Parameters["_screenSize"];

			_verticalSizeParam.SetValue( _verticalSize );
			_horizontalOffsetParam.SetValue( _horizontalOffset );
			_screenSizeParam.SetValue( new Vector2( Screen.width, Screen.height ) );
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			_screenSizeParam.SetValue( new Vector2( newWidth, newHeight ) );
		}
	}
}

