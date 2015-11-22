using System.Collections;


namespace Nez.Tweens
{
	public enum EaseType
	{
		Linear,

		SineIn,
		SineOut,
		SineInOut,

		QuadIn,
		QuadOut,
		QuadInOut,

		CubicIn,
		CubicOut,
		CubicInOut,

		QuartIn,
		QuartOut,
		QuartInOut,

		QuintIn,
		QuintOut,
		QuintInOut,

		ExpoIn,
		ExpoOut,
		ExpoInOut,

		CircIn,
		CircOut,
		CircInOut,

		ElasticIn,
		ElasticOut,
		ElasticInOut,
		Punch,

		BackIn,
		BackOut,
		BackInOut,

		BounceIn,
		BounceOut,
		BounceInOut
	}


	/// <summary>
	/// helper with a single method that takes in an EaseType and applies that ease equation with the given
	/// duration and time parameters. We do this to avoid passing around Funcs which create bogs of trash for
	/// the garbage collector (function pointers please!)
	/// </summary>
	public static class EaseHelper
	{
		public static float ease( EaseType easeType, float t, float duration )
		{
			switch( easeType )
			{
				case EaseType.Linear:
					return Easing.Linear.EaseNone( t, duration );

				case EaseType.BackIn:
					return Easing.Back.EaseIn( t, duration );
				case EaseType.BackOut:
					return Easing.Back.EaseOut( t, duration );
				case EaseType.BackInOut:
					return Easing.Back.EaseInOut( t, duration );

				case EaseType.BounceIn:
					return Easing.Bounce.EaseIn( t, duration );
				case EaseType.BounceOut:
					return Easing.Bounce.EaseOut( t, duration );
				case EaseType.BounceInOut:
					return Easing.Bounce.EaseInOut( t, duration );

				case EaseType.CircIn:
					return Easing.Circular.EaseIn( t, duration );
				case EaseType.CircOut:
					return Easing.Circular.EaseOut( t, duration );
				case EaseType.CircInOut:
					return Easing.Circular.EaseInOut( t, duration );

				case EaseType.CubicIn:
					return Easing.Cubic.EaseIn( t, duration );
				case EaseType.CubicOut:
					return Easing.Cubic.EaseOut( t, duration );
				case EaseType.CubicInOut:
					return Easing.Cubic.EaseInOut( t, duration );

				case EaseType.ElasticIn:
					return Easing.Elastic.EaseIn( t, duration );
				case EaseType.ElasticOut:
					return Easing.Elastic.EaseOut( t, duration );
				case EaseType.ElasticInOut:
					return Easing.Elastic.EaseInOut( t, duration );
				case EaseType.Punch:
					return Easing.Elastic.Punch( t, duration );

				case EaseType.ExpoIn:
					return Easing.Exponential.EaseIn( t, duration );
				case EaseType.ExpoOut:
					return Easing.Exponential.EaseOut( t, duration );
				case EaseType.ExpoInOut:
					return Easing.Exponential.EaseInOut( t, duration );

				case EaseType.QuadIn:
					return Easing.Quadratic.EaseIn( t, duration );
				case EaseType.QuadOut:
					return Easing.Quadratic.EaseOut( t, duration );
				case EaseType.QuadInOut:
					return Easing.Quadratic.EaseInOut( t, duration );

				case EaseType.QuartIn:
					return Easing.Quartic.EaseIn( t, duration );
				case EaseType.QuartOut:
					return Easing.Quartic.EaseOut( t, duration );
				case EaseType.QuartInOut:
					return Easing.Quartic.EaseInOut( t, duration );

				case EaseType.QuintIn:
					return Easing.Quintic.EaseIn( t, duration );
				case EaseType.QuintOut:
					return Easing.Quintic.EaseOut( t, duration );
				case EaseType.QuintInOut:
					return Easing.Quintic.EaseInOut( t, duration );

				case EaseType.SineIn:
					return Easing.Sinusoidal.EaseIn( t, duration );
				case EaseType.SineOut:
					return Easing.Sinusoidal.EaseOut( t, duration );
				case EaseType.SineInOut:
					return Easing.Sinusoidal.EaseInOut( t, duration );
				
				default:
					return Easing.Linear.EaseNone( t, duration );
			}
		}
	}

}
