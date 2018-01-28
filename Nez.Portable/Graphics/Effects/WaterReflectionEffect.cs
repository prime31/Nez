using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class WaterReflectionEffect : ReflectionEffect
	{
		/// <summary>
		/// defaults to 0.015. Waves are calculated by sampling the normal map twice. Any values generated that are sparkleIntensity greater
		/// than the actual uv value at the place of sampling will be colored sparkleColor.
		/// </summary>
		/// <value>The sparkle intensity.</value>
		public float sparkleIntensity { set { _sparkleIntensityParam.SetValue( value ); } }

		/// <summary>
		/// the color for the sparkly wave peaks
		/// </summary>
		/// <value>The color of the sparkle.</value>
		public Vector3 sparkleColor { set { _sparkleColorParam.SetValue( value ); } }

		/// <summary>
		/// position in screen space of the top of the water plane
		/// </summary>
		/// <value>The screen space vertical offset.</value>
		public float screenSpaceVerticalOffset { set { _screenSpaceVerticalOffsetParam.SetValue( Mathf.map( value, 0, 1, -1, 1 ) ); } }

		/// <summary>
		/// defaults to 0.3. intensity of the perspective correction
		/// </summary>
		/// <value>The perspective correction intensity.</value>
		public float perspectiveCorrectionIntensity { set { _perspectiveCorrectionIntensityParam.SetValue( value ); } }

		/// <summary>
		/// defaults to 2. speed that the first displacment/normal uv is scrolled
		/// </summary>
		/// <value>The first displacement speed.</value>
		public float firstDisplacementSpeed { set { _firstDisplacementSpeedParam.SetValue( value / 100 ); } }

		/// <summary>
		/// defaults to 6. speed that the second displacment/normal uv is scrolled
		/// </summary>
		/// <value>The second displacement speed.</value>
		public float secondDisplacementSpeed { set { _secondDisplacementSpeedParam.SetValue( value / 100 ); } }

		/// <summary>
		/// defaults to 3. the normal map is sampled twice then combined. The 2nd sampling is scaled by this value.
		/// </summary>
		/// <value>The second displacement scale.</value>
		public float secondDisplacementScale { set { _secondDisplacementScaleParam.SetValue( value ); } }

		const float _sparkleIntensity = 0.015f;
		const float _perspectiveCorrectionIntensity = 0.3f;
		const float _reflectionIntensity = 0.85f;
		const float _normalMagnitude = 0.03f;
		const float _firstDisplacementSpeed = 6f;
		const float _secondDisplacementSpeed = 2f;
		const float _secondDisplacementScale = 3f;

		EffectParameter _timeParam;
		EffectParameter _sparkleIntensityParam;
		EffectParameter _sparkleColorParam;
		EffectParameter _screenSpaceVerticalOffsetParam;
		EffectParameter _perspectiveCorrectionIntensityParam;
		EffectParameter _firstDisplacementSpeedParam;
		EffectParameter _secondDisplacementSpeedParam;
		EffectParameter _secondDisplacementScaleParam;


		public WaterReflectionEffect() : base()
		{
			CurrentTechnique = Techniques["WaterReflectionTechnique"];

			_timeParam = Parameters["_time"];
			_sparkleIntensityParam = Parameters["_sparkleIntensity"];
			_sparkleColorParam = Parameters["_sparkleColor"];
			_screenSpaceVerticalOffsetParam = Parameters["_screenSpaceVerticalOffset"];
			_perspectiveCorrectionIntensityParam = Parameters["_perspectiveCorrectionIntensity"];
			_firstDisplacementSpeedParam = Parameters["_firstDisplacementSpeed"];
			_secondDisplacementSpeedParam = Parameters["_secondDisplacementSpeed"];
			_secondDisplacementScaleParam = Parameters["_secondDisplacementScale"];

			_sparkleIntensityParam.SetValue( _sparkleIntensity );
			_sparkleColorParam.SetValue( Vector3.One );
			_perspectiveCorrectionIntensityParam.SetValue( _perspectiveCorrectionIntensity );
			firstDisplacementSpeed = _firstDisplacementSpeed;
			secondDisplacementSpeed = _secondDisplacementSpeed;
			_secondDisplacementScaleParam.SetValue( _secondDisplacementScale );

			// override some defaults from the ReflectionEffect
			reflectionIntensity = _reflectionIntensity;
			normalMagnitude = _normalMagnitude;
		}


		protected override void OnApply()
		{
			_timeParam.SetValue( Time.time );
		}
	}
}

