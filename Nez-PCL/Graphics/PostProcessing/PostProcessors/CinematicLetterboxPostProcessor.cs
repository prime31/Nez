using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;
using Nez.Tweens;


namespace Nez
{
	public class CinematicLetterboxPostProcessor : PostProcessor
	{
		/// <summary>
		/// color of the letterbox
		/// </summary>
		/// <value>The color.</value>
		public Color color
		{
			get { return _color; }
			set
			{
				if( _color != value )
				{
					_color = value;

					if( effect != null )
						_colorParam.SetValue( _color.ToVector4() );
				}
			}
		}

		/// <summary>
		/// size in pixels of the letterbox
		/// </summary>
		/// <value>The size of the letterbox.</value>
		public float letterboxSize
		{
			get { return _letterboxSize; }
			set
			{
				if( _letterboxSize != value )
				{
					_letterboxSize = value;

					if( effect != null )
						_letterboxSizeParam.SetValue( _letterboxSize );
				}
			}
		}

		Color _color = Color.Black;
		float _letterboxSize = 0f;
		EffectParameter _colorParam;
		EffectParameter _letterboxSizeParam;
		bool _isAnimating;


		public CinematicLetterboxPostProcessor( int executionOrder ) : base( executionOrder )
		{}


		public override void onAddedToScene()
		{
			effect = scene.contentManager.loadEffect<Effect>( "vignette", EffectResource.letterboxBytes );

			_colorParam = effect.Parameters["_color"];
			_letterboxSizeParam = effect.Parameters["_letterboxSize"];
			_colorParam.SetValue( _color.ToVector4() );
			_letterboxSizeParam.SetValue( _letterboxSize );
		}


		/// <summary>
		/// animates the letterbox in
		/// </summary>
		/// <returns>The in.</returns>
		/// <param name="letterboxSize">Letterbox size.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="easeType">Ease type.</param>
		public IEnumerator animateIn( float letterboxSize, float duration = 2, EaseType easeType = EaseType.ExpoOut )
		{
			// wait for any current animations to complete
			while( _isAnimating )
				yield return null;
			
			_isAnimating = true;
			var elapsedTime = 0f;
			while( elapsedTime < duration )
			{
				elapsedTime += Time.deltaTime;
				this.letterboxSize = Lerps.ease( easeType, 0, letterboxSize, elapsedTime, duration );
				yield return null;
			}
			_isAnimating = false;
		}


		/// <summary>
		/// animates the letterbox out
		/// </summary>
		/// <returns>The out.</returns>
		/// <param name="duration">Duration.</param>
		/// <param name="easeType">Ease type.</param>
		public IEnumerator animateOut( float duration = 2, EaseType easeType = EaseType.ExpoIn )
		{
			// wait for any current animations to complete
			while( _isAnimating )
				yield return null;
			
			_isAnimating = true;
			var startSize = letterboxSize;
			var elapsedTime = 0f;
			while( elapsedTime < duration )
			{
				elapsedTime += Time.deltaTime;
				this.letterboxSize = Lerps.ease( easeType, startSize, 0, elapsedTime, duration );
				yield return null;
			}
			_isAnimating = false;
		}
	}
}

