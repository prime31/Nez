using Microsoft.Xna.Framework;


namespace Nez.Tweens
{
	/// <summary>
	/// series of static methods to handle all common tween type structs along with unclamped lerps for them.
	/// unclamped lerps are required for bounce, elastic or other tweens that exceed the 0 - 1 range.
	/// </summary>
	public static class Lerps
	{
		#region Lerps

		public static float Lerp(float from, float to, float t)
		{
			return from + (to - from) * t;
		}


		/// <summary>
		/// remainingFactorPerSecond is the percentage of the distance it covers every second. should be between 0 and 1.
		/// if it's 0.25 it means it covers 75% of the remaining distance every second independent of the framerate
		/// </summary>
		/// <returns>The towards.</returns>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="remainingFactorPerSecond">Remaining factor per second.</param>
		/// <param name="deltaTime">Delta time.</param>
		public static float LerpTowards(float from, float to, float remainingFactorPerSecond, float deltaTime)
		{
			return Lerp(from, to, 1f - Mathf.Pow(remainingFactorPerSecond, deltaTime));
		}


		/// <summary>
		/// A smoothing rate of zero will give you back the target value (i.e. no smoothing), and a rate of 1 is technically not allowed,
		/// but will just give you back the source value (i.e. infinite smoothing). Note that this is the opposite of the way a lerp
		/// parameter works, but if you so desire, you can just use additive inverse of the smoothing parameter inside the Pow.
		/// Smoothing rate dictates the proportion of source remaining after one second
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="target">Target.</param>
		/// <param name="smoothing">Smoothing.</param>
		/// <param name="dt">Dt.</param>
		public static float LerpDamp(float source, float target, float smoothing)
		{
			return MathHelper.Lerp(source, target, 1 - Mathf.Pow(smoothing, Time.DeltaTime));
		}


		public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
		{
			return new Vector2(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);
		}


		// remainingFactorPerSecond is the percentage of the distance it covers every second. should be between 0 and 1.
		// if it's 0.25 it means it covers 75% of the remaining distance every second independent of the framerate
		public static Vector2 LerpTowards(Vector2 from, Vector2 to, float remainingFactorPerSecond, float deltaTime)
		{
			return Lerp(from, to, 1f - Mathf.Pow(remainingFactorPerSecond, deltaTime));
		}


		public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
		{
			return new Vector3(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t,
				from.Z + (to.Z - from.Z) * t);
		}


		// remainingFactorPerSecond is the percentage of the distance it covers every second. should be between 0 and 1.
		// if it's 0.25 it means it covers 75% of the remaining distance every second independent of the framerate
		public static Vector3 LerpTowards(Vector3 from, Vector3 to, float remainingFactorPerSecond, float deltaTime)
		{
			return Lerp(from, to, 1f - Mathf.Pow(remainingFactorPerSecond, deltaTime));
		}


		// a different variant that requires the target details to calculate the lerp
		public static Vector3 LerpTowards(Vector3 followerCurrentPosition, Vector3 targetPreviousPosition,
		                                  Vector3 targetCurrentPosition, float smoothFactor, float deltaTime)
		{
			var targetDiff = targetCurrentPosition - targetPreviousPosition;
			var temp = followerCurrentPosition - targetPreviousPosition + targetDiff / (smoothFactor * deltaTime);
			return targetCurrentPosition - targetDiff / (smoothFactor * deltaTime) +
			       temp * Mathf.Exp(-smoothFactor * deltaTime);
		}


		public static Vector2 AngleLerp(Vector2 from, Vector2 to, float t)
		{
			// we calculate the shortest difference between the angles for this lerp
			var toMinusFrom = new Vector2(Mathf.DeltaAngle(from.X, to.X), Mathf.DeltaAngle(from.Y, to.Y));
			return new Vector2(from.X + toMinusFrom.X * t, from.Y + toMinusFrom.Y * t);
		}


		public static Vector4 Lerp(Vector4 from, Vector4 to, float t)
		{
			return new Vector4(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t, from.Z + (to.Z - from.Z) * t,
				from.W + (to.W - from.W) * t);
		}


		public static Color Lerp(Color from, Color to, float t)
		{
			var t255 = (int) (t * 255);
			return new Color(from.R + (to.R - from.R) * t255 / 255, from.G + (to.G - from.G) * t255 / 255,
				from.B + (to.B - from.B) * t255 / 255, from.A + (to.A - from.A) * t255 / 255);
		}


		public static Color Lerp(ref Color from, ref Color to, float t)
		{
			var t255 = (int) (t * 255);
			return new Color(from.R + (to.R - from.R) * t255 / 255, from.G + (to.G - from.G) * t255 / 255,
				from.B + (to.B - from.B) * t255 / 255, from.A + (to.A - from.A) * t255 / 255);
		}


		public static Rectangle Lerp(Rectangle from, Rectangle to, float t)
		{
			return new Rectangle
			(
				(int) (from.X + (to.X - from.X) * t),
				(int) (from.Y + (to.Y - from.Y) * t),
				(int) (from.Width + (to.Width - from.Width) * t),
				(int) (from.Height + (to.Height - from.Height) * t)
			);
		}


		public static Rectangle Lerp(ref Rectangle from, ref Rectangle to, float t)
		{
			return new Rectangle
			(
				(int) (from.X + (to.X - from.X) * t),
				(int) (from.Y + (to.Y - from.Y) * t),
				(int) (from.Width + (to.Width - from.Width) * t),
				(int) (from.Height + (to.Height - from.Height) * t)
			);
		}

		#endregion


		#region Easers

		public static float Ease(EaseType easeType, float from, float to, float t, float duration)
		{
			return Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Vector2 Ease(EaseType easeType, Vector2 from, Vector2 to, float t, float duration)
		{
			return Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Vector3 Ease(EaseType easeType, Vector3 from, Vector3 to, float t, float duration)
		{
			return Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Vector2 EaseAngle(EaseType easeType, Vector2 from, Vector2 to, float t, float duration)
		{
			return AngleLerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Vector4 Ease(EaseType easeType, Vector4 from, Vector4 to, float t, float duration)
		{
			return Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Quaternion Ease(EaseType easeType, Quaternion from, Quaternion to, float t, float duration)
		{
			return Quaternion.Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Color Ease(EaseType easeType, Color from, Color to, float t, float duration)
		{
			return Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Color Ease(EaseType easeType, ref Color from, ref Color to, float t, float duration)
		{
			return Lerp(ref from, ref to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Rectangle Ease(EaseType easeType, Rectangle from, Rectangle to, float t, float duration)
		{
			return Lerp(from, to, EaseHelper.Ease(easeType, t, duration));
		}


		public static Rectangle Ease(EaseType easeType, ref Rectangle from, ref Rectangle to, float t, float duration)
		{
			return Lerp(ref from, ref to, EaseHelper.Ease(easeType, t, duration));
		}

		#endregion


		#region Springs

		/// <summary>
		/// uses the semi-implicit euler method. faster, but not always stable.
		/// see http://allenchou.net/2015/04/game-math-more-on-numeric-springing/
		/// </summary>
		/// <returns>The spring.</returns>
		/// <param name="currentValue">Current value.</param>
		/// <param name="targetValue">Target value.</param>
		/// <param name="velocity">Velocity by reference. Be sure to reset it to 0 if changing the targetValue between calls</param>
		/// <param name="dampingRatio">lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.</param>
		/// <param name="angularFrequency">An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stable</param>
		public static float FastSpring(float currentValue, float targetValue, ref float velocity, float dampingRatio,
		                               float angularFrequency)
		{
			velocity += -2.0f * Time.DeltaTime * dampingRatio * angularFrequency * velocity +
			            Time.DeltaTime * angularFrequency * angularFrequency * (targetValue - currentValue);
			currentValue += Time.DeltaTime * velocity;

			return currentValue;
		}


		/// <summary>
		/// uses the implicit euler method. slower, but always stable.
		/// see http://allenchou.net/2015/04/game-math-more-on-numeric-springing/
		/// </summary>
		/// <returns>The spring.</returns>
		/// <param name="currentValue">Current value.</param>
		/// <param name="targetValue">Target value.</param>
		/// <param name="velocity">Velocity by reference. Be sure to reset it to 0 if changing the targetValue between calls</param>
		/// <param name="dampingRatio">lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.</param>
		/// <param name="angularFrequency">An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stable</param>
		public static float StableSpring(float currentValue, float targetValue, ref float velocity, float dampingRatio,
		                                 float angularFrequency)
		{
			var f = 1f + 2f * Time.DeltaTime * dampingRatio * angularFrequency;
			var oo = angularFrequency * angularFrequency;
			var hoo = Time.DeltaTime * oo;
			var hhoo = Time.DeltaTime * hoo;
			var detInv = 1.0f / (f + hhoo);
			var detX = f * currentValue + Time.DeltaTime * velocity + hhoo * targetValue;
			var detV = velocity + hoo * (targetValue - currentValue);

			currentValue = detX * detInv;
			velocity = detV * detInv;

			return currentValue;
		}


		/// <summary>
		/// uses the semi-implicit euler method. slower, but always stable.
		/// see http://allenchou.net/2015/04/game-math-more-on-numeric-springing/
		/// </summary>
		/// <returns>The spring.</returns>
		/// <param name="currentValue">Current value.</param>
		/// <param name="targetValue">Target value.</param>
		/// <param name="velocity">Velocity by reference. Be sure to reset it to 0 if changing the targetValue between calls</param>
		/// <param name="dampingRatio">lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.</param>
		/// <param name="angularFrequency">An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stable</param>
		public static Vector2 FastSpring(Vector2 currentValue, Vector2 targetValue, ref Vector2 velocity,
		                                 float dampingRatio, float angularFrequency)
		{
			velocity += -2.0f * Time.DeltaTime * dampingRatio * angularFrequency * velocity +
			            Time.DeltaTime * angularFrequency * angularFrequency * (targetValue - currentValue);
			currentValue += Time.DeltaTime * velocity;

			return currentValue;
		}


		/// <summary>
		/// uses the implicit euler method. faster, but not always stable.
		/// see http://allenchou.net/2015/04/game-math-more-on-numeric-springing/
		/// </summary>
		/// <returns>The spring.</returns>
		/// <param name="currentValue">Current value.</param>
		/// <param name="targetValue">Target value.</param>
		/// <param name="velocity">Velocity by reference. Be sure to reset it to 0 if changing the targetValue between calls</param>
		/// <param name="dampingRatio">lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.</param>
		/// <param name="angularFrequency">An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stable</param>
		public static Vector2 StableSpring(Vector2 currentValue, Vector2 targetValue, ref Vector2 velocity,
		                                   float dampingRatio, float angularFrequency)
		{
			var f = 1f + 2f * Time.DeltaTime * dampingRatio * angularFrequency;
			var oo = angularFrequency * angularFrequency;
			var hoo = Time.DeltaTime * oo;
			var hhoo = Time.DeltaTime * hoo;
			var detInv = 1.0f / (f + hhoo);
			var detX = f * currentValue + Time.DeltaTime * velocity + hhoo * targetValue;
			var detV = velocity + hoo * (targetValue - currentValue);

			currentValue = detX * detInv;
			velocity = detV * detInv;

			return currentValue;
		}

		#endregion
	}
}