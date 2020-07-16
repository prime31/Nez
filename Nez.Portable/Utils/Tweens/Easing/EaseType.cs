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
		/// <summary>
		/// returns the opposite EaseType of easeType
		/// </summary>
		/// <returns>The ease type.</returns>
		/// <param name="easeType">Ease type.</param>
		public static EaseType OppositeEaseType(EaseType easeType)
		{
			switch (easeType)
			{
				case EaseType.Linear:
					return easeType;

				case EaseType.BackIn:
					return EaseType.BackOut;
				case EaseType.BackOut:
					return EaseType.BackIn;
				case EaseType.BackInOut:
					return easeType;

				case EaseType.BounceIn:
					return EaseType.BounceOut;
				case EaseType.BounceOut:
					return EaseType.BounceIn;
				case EaseType.BounceInOut:
					return easeType;

				case EaseType.CircIn:
					return EaseType.CircOut;
				case EaseType.CircOut:
					return EaseType.CircIn;
				case EaseType.CircInOut:
					return easeType;

				case EaseType.CubicIn:
					return EaseType.CubicOut;
				case EaseType.CubicOut:
					return EaseType.CubicIn;
				case EaseType.CubicInOut:
					return easeType;

				case EaseType.ElasticIn:
					return EaseType.ElasticOut;
				case EaseType.ElasticOut:
					return EaseType.ElasticIn;
				case EaseType.ElasticInOut:
					return easeType;

				case EaseType.Punch:
					return easeType;

				case EaseType.ExpoIn:
					return EaseType.ExpoOut;
				case EaseType.ExpoOut:
					return EaseType.ExpoIn;
				case EaseType.ExpoInOut:
					return easeType;

				case EaseType.QuadIn:
					return EaseType.QuadOut;
				case EaseType.QuadOut:
					return EaseType.QuadIn;
				case EaseType.QuadInOut:
					return easeType;

				case EaseType.QuartIn:
					return EaseType.QuartOut;
				case EaseType.QuartOut:
					return EaseType.QuartIn;
				case EaseType.QuartInOut:
					return easeType;

				case EaseType.QuintIn:
					return EaseType.QuintOut;
				case EaseType.QuintOut:
					return EaseType.QuintIn;
				case EaseType.QuintInOut:
					return easeType;

				case EaseType.SineIn:
					return EaseType.SineOut;
				case EaseType.SineOut:
					return EaseType.SineIn;
				case EaseType.SineInOut:
					return easeType;

				default:
					return easeType;
			}
		}


		public static float Ease(EaseType easeType, float t, float duration)
		{
			switch (easeType)
			{
				case EaseType.Linear:
					return Easing.Linear.EaseNone(t, duration);

				case EaseType.BackIn:
					return Easing.Back.EaseIn(t, duration);
				case EaseType.BackOut:
					return Easing.Back.EaseOut(t, duration);
				case EaseType.BackInOut:
					return Easing.Back.EaseInOut(t, duration);

				case EaseType.BounceIn:
					return Easing.Bounce.EaseIn(t, duration);
				case EaseType.BounceOut:
					return Easing.Bounce.EaseOut(t, duration);
				case EaseType.BounceInOut:
					return Easing.Bounce.EaseInOut(t, duration);

				case EaseType.CircIn:
					return Easing.Circular.EaseIn(t, duration);
				case EaseType.CircOut:
					return Easing.Circular.EaseOut(t, duration);
				case EaseType.CircInOut:
					return Easing.Circular.EaseInOut(t, duration);

				case EaseType.CubicIn:
					return Easing.Cubic.EaseIn(t, duration);
				case EaseType.CubicOut:
					return Easing.Cubic.EaseOut(t, duration);
				case EaseType.CubicInOut:
					return Easing.Cubic.EaseInOut(t, duration);

				case EaseType.ElasticIn:
					return Easing.Elastic.EaseIn(t, duration);
				case EaseType.ElasticOut:
					return Easing.Elastic.EaseOut(t, duration);
				case EaseType.ElasticInOut:
					return Easing.Elastic.EaseInOut(t, duration);
				case EaseType.Punch:
					return Easing.Elastic.Punch(t, duration);

				case EaseType.ExpoIn:
					return Easing.Exponential.EaseIn(t, duration);
				case EaseType.ExpoOut:
					return Easing.Exponential.EaseOut(t, duration);
				case EaseType.ExpoInOut:
					return Easing.Exponential.EaseInOut(t, duration);

				case EaseType.QuadIn:
					return Easing.Quadratic.EaseIn(t, duration);
				case EaseType.QuadOut:
					return Easing.Quadratic.EaseOut(t, duration);
				case EaseType.QuadInOut:
					return Easing.Quadratic.EaseInOut(t, duration);

				case EaseType.QuartIn:
					return Easing.Quartic.EaseIn(t, duration);
				case EaseType.QuartOut:
					return Easing.Quartic.EaseOut(t, duration);
				case EaseType.QuartInOut:
					return Easing.Quartic.EaseInOut(t, duration);

				case EaseType.QuintIn:
					return Easing.Quintic.EaseIn(t, duration);
				case EaseType.QuintOut:
					return Easing.Quintic.EaseOut(t, duration);
				case EaseType.QuintInOut:
					return Easing.Quintic.EaseInOut(t, duration);

				case EaseType.SineIn:
					return Easing.Sinusoidal.EaseIn(t, duration);
				case EaseType.SineOut:
					return Easing.Sinusoidal.EaseOut(t, duration);
				case EaseType.SineInOut:
					return Easing.Sinusoidal.EaseInOut(t, duration);

				default:
					return Easing.Linear.EaseNone(t, duration);
			}
		}
	}
}