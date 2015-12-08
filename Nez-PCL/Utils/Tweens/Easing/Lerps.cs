using EaseFunction = System.Func<float, float, float>;
using Microsoft.Xna.Framework;
using System;


namespace Nez.Tweens
{
	/// <summary>
	/// series of static methods to handle all common tween type structs along with unclamped lerps for them.
	/// unclamped lerps are required for bounce, elastic or other tweens that exceed the 0 - 1 range.
	/// </summary>
	public static class Lerps
	{
		#region Lerps

		public static float unclampedLerp( float from, float to, float t )
		{
			return from + ( to - from ) * t;
		}
		
		
		// remainingFactorPerSecond is the percentage of the distance it covers every second. should be between 0 and 1.
		// if it's 0.25 it means it covers 75% of the remaining distance every second independent of the framerate
		public static float lerpTowards( float from, float to, float remainingFactorPerSecond, float deltaTime )
		{
			return unclampedLerp( from, to, 1f - Mathf.pow( remainingFactorPerSecond, deltaTime ) );
		}


		public static Vector2 unclampedLerp( Vector2 from, Vector2 to, float t )
		{
			return new Vector2( from.X + ( to.X - from.X ) * t, from.Y + ( to.Y - from.Y ) * t );
		}
		
		
		// remainingFactorPerSecond is the percentage of the distance it covers every second. should be between 0 and 1.
		// if it's 0.25 it means it covers 75% of the remaining distance every second independent of the framerate
		public static Vector2 lerpTowards( Vector2 from, Vector2 to, float remainingFactorPerSecond, float deltaTime )
		{
			return unclampedLerp( from, to, 1f - Mathf.pow( remainingFactorPerSecond, deltaTime ) );
		}


		public static Vector3 unclampedLerp( Vector3 from, Vector3 to, float t )
		{
			return new Vector3( from.X + ( to.X - from.X ) * t, from.Y + ( to.Y - from.Y ) * t, from.Z + ( to.Z - from.Z ) * t );
		}
		
		
		// remainingFactorPerSecond is the percentage of the distance it covers every second. should be between 0 and 1.
		// if it's 0.25 it means it covers 75% of the remaining distance every second independent of the framerate
		public static Vector3 lerpTowards( Vector3 from, Vector3 to, float remainingFactorPerSecond, float deltaTime )
		{
			return unclampedLerp( from, to, 1f - Mathf.pow( remainingFactorPerSecond, deltaTime ) );
		}
		
		
		// a different variant that requires the target details to calculate the lerp
		public static Vector3 lerpTowards( Vector3 followerCurrentPosition, Vector3 targetPreviousPosition, Vector3 targetCurrentPosition, float smoothFactor, float deltaTime )
	    {
			var targetDiff = targetCurrentPosition - targetPreviousPosition;
	        var temp = followerCurrentPosition - targetPreviousPosition + targetDiff / ( smoothFactor * deltaTime );
	        return targetCurrentPosition - targetDiff / ( smoothFactor * deltaTime ) + temp * Mathf.exp( -smoothFactor * deltaTime );
	    }


		public static Vector3 unclampedAngledLerp( Vector3 from, Vector3 to, float t )
		{
			// we calculate the shortest difference between the angles for this lerp
			var toMinusFrom = new Vector3( Mathf.deltaAngle( from.X, to.X ), Mathf.deltaAngle( from.Y, to.Y ), Mathf.deltaAngle( from.Z, to.Z ) );
			return new Vector3( from.X + toMinusFrom.X * t, from.Y + toMinusFrom.Y * t, from.Z + toMinusFrom.Z * t );
		}


		public static Vector4 unclampedLerp( Vector4 from, Vector4 to, float t )
		{
			return new Vector4( from.X + ( to.X - from.X ) * t, from.Y + ( to.Y - from.Y ) * t, from.Z + ( to.Z - from.Z ) * t, from.W + ( to.W - from.W ) * t );
		}


		public static Color unclampedLerp( Color from, Color to, float t )
		{
			var t256 = (int)( t * 256 );
			return new Color( from.R + ( to.R - from.R ) * t256 / 256, from.G + ( to.G - from.G ) * t256 / 256, from.B + ( to.B - from.B ) * t256 / 256, from.A + ( to.A - from.A ) * t256 / 256 );
		}


		public static Rectangle unclampedLerp( Rectangle from, Rectangle to, float t )
		{
			return new Rectangle
			(
				(int)( from.X + ( to.X - from.X ) * t ),
				(int)( from.Y + ( to.Y - from.Y ) * t ),
				(int)( from.Width + ( to.Width - from.Width ) * t ),
				(int)( from.Height + ( to.Height - from.Height ) * t )
			);
		}

		#endregion


		#region Easers

		public static float ease( EaseType easeType, float from, float to, float t, float duration )
		{
			return unclampedLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Vector2 ease( EaseType easeType, Vector2 from, Vector2 to, float t, float duration )
		{
			return unclampedLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Vector3 ease( EaseType easeType, Vector3 from, Vector3 to, float t, float duration )
		{
			return unclampedLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Vector3 easeAngle( EaseType easeType, Vector3 from, Vector3 to, float t, float duration )
		{
			return unclampedAngledLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Vector4 ease( EaseType easeType, Vector4 from, Vector4 to, float t, float duration )
		{
			return unclampedLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Quaternion ease( EaseType easeType, Quaternion from, Quaternion to, float t, float duration )
		{
			return Quaternion.Lerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Color ease( EaseType easeType, Color from, Color to, float t, float duration )
		{
			return unclampedLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
		}


		public static Rectangle ease( EaseType easeType, Rectangle from, Rectangle to, float t, float duration )
		{
			return unclampedLerp( from, to, EaseHelper.ease( easeType, t, duration ) );
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
		public static float fastSpring( float currentValue, float targetValue, ref float velocity, float dampingRatio, float angularFrequency )
		{
			velocity += -2.0f * Time.deltaTime * dampingRatio * angularFrequency * velocity + Time.deltaTime * angularFrequency * angularFrequency * ( targetValue - currentValue );
			currentValue += Time.deltaTime * velocity;

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
		public static float stableSpring( float currentValue, float targetValue, ref float velocity, float dampingRatio, float angularFrequency )
		{
			var f = 1f + 2f * Time.deltaTime * dampingRatio * angularFrequency;
			var oo = angularFrequency * angularFrequency;
			var hoo = Time.deltaTime * oo;
			var hhoo = Time.deltaTime * hoo;
			var detInv = 1.0f / ( f + hhoo );
			var detX = f * currentValue + Time.deltaTime * velocity + hhoo * targetValue;
			var detV = velocity + hoo * ( targetValue - currentValue );

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
		public static Vector3 fastSpring( Vector3 currentValue, Vector3 targetValue, ref Vector3 velocity, float dampingRatio, float angularFrequency )
		{
			velocity += -2.0f * Time.deltaTime * dampingRatio * angularFrequency * velocity + Time.deltaTime * angularFrequency * angularFrequency * ( targetValue - currentValue );
			currentValue += Time.deltaTime * velocity;

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
		public static Vector3 stableSpring( Vector3 currentValue, Vector3 targetValue, ref Vector3 velocity, float dampingRatio, float angularFrequency )
		{
			var f = 1f + 2f * Time.deltaTime * dampingRatio * angularFrequency;
			var oo = angularFrequency * angularFrequency;
			var hoo = Time.deltaTime * oo;
			var hhoo = Time.deltaTime * hoo;
			var detInv = 1.0f / ( f + hhoo );
			var detX = f * currentValue + Time.deltaTime * velocity + hhoo * targetValue;
			var detV = velocity + hoo * ( targetValue - currentValue );

			currentValue = detX * detInv;
			velocity = detV * detInv;

			return currentValue;
		}

		#endregion

	}
}
