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
		/// defaults to 3. the normal map is sampled twice then combined. The 2nd sampling is scaled by this value.
		/// </summary>
		/// <value>The second displacement scale.</value>
		public float secondDisplacementScale { set { _secondDisplacementScaleParam.SetValue( value ); } }


		EffectParameter _timeParam;
		EffectParameter _sparkleIntensityParam;
		EffectParameter _sparkleColorParam;
		EffectParameter _screenSpaceVerticalOffsetParam;
		EffectParameter _perspectiveCorrectionIntensityParam;
		EffectParameter _secondDisplacementScaleParam;


		public WaterReflectionEffect() : base()
		{
			CurrentTechnique = Techniques["WaterReflectionTechnique"];

			_timeParam = Parameters["_time"];
			_sparkleIntensityParam = Parameters["_sparkleIntensity"];
			_sparkleColorParam = Parameters["_sparkleColor"];
			_screenSpaceVerticalOffsetParam = Parameters["_screenSpaceVerticalOffset"];
			_perspectiveCorrectionIntensityParam = Parameters["_perspectiveCorrectionIntensity"];
			_secondDisplacementScaleParam = Parameters["_secondDisplacementScale"];

			_sparkleIntensityParam.SetValue( 0.015f );
			_sparkleColorParam.SetValue( Vector3.One );
			_perspectiveCorrectionIntensityParam.SetValue( 0.3f );
			_secondDisplacementScaleParam.SetValue( 3f );

			// override some defaults from the ReflectionEffect
			reflectionIntensity = 0.8f;
			normalMagnitude = 0.03f;
		}


		protected override bool OnApply()
		{
			_timeParam.SetValue( Time.time );
			return base.OnApply();
		}
	}
}

