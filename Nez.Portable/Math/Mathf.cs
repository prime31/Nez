using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Mathf
	{
		public const float Epsilon = 0.00001f;
		public const float Deg2Rad = 0.0174532924f;
		public const float Rad2Deg = 57.29578f;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Round(float f)
		{
			return (float)Math.Round(f);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Ceil(float f)
		{
			return (float)Math.Ceiling(f);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CeilToInt(float f)
		{
			return (int)Math.Ceiling((double)f);
		}


		/// <summary>
		/// ceils the float to the nearest int value above y. note that this only works for values in the range of short
		/// </summary>
		/// <returns>The ceil to int.</returns>
		/// <param name="y">F.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FastCeilToInt(float y)
		{
			return 32768 - (int)(32768f - y);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Floor(float f)
		{
			return (float)Math.Floor(f);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloorToInt(float f)
		{
			return (int)Math.Floor((double)f);
		}

		/// <summary>Returns the result of converting a float value from degrees to radians.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Radians(float x) => x * 0.0174532925f;

		/// <summary>Returns the result of converting a double value from radians to degrees.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Degrees(float x) => x * 57.295779513f;

		/// <summary>
		/// floors the float to the nearest int value below x. note that this only works for values in the range of short
		/// </summary>
		/// <returns>The floor to int.</returns>
		/// <param name="x">The x coordinate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FastFloorToInt(float x)
		{
			// we shift to guaranteed positive before casting then shift back after
			return (int)(x + 32768f) - 32768;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RoundToInt(float f)
		{
			return (int)Math.Round(f);
		}


		/// <summary>
		/// Calculates the integral part of a number cast to an int
		/// </summary>
		/// <returns>The to int.</returns>
		/// <param name="f">F.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int TruncateToInt(float f)
		{
			return (int)Math.Truncate(f);
		}


		/// <summary>
		/// clamps value between 0 and 1
		/// </summary>
		/// <param name="value">Value.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp01(float value)
		{
			if (value < 0f)
				return 0f;

			if (value > 1f)
				return 1f;

			return value;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(float value, float min, float max)
		{
			if (value < min)
				return min;

			if (value > max)
				return max;

			return value;
		}


		/// <summary>
		/// Restricts a value to be within a specified range.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c> will be returned.</param>
		/// <param name="max">The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c> will be returned.</param>
		/// <returns>The clamped value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(int value, int min, int max)
		{
			value = (value > max) ? max : value;
			value = (value < min) ? min : value;

			return value;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Snap(float value, float increment)
		{
			return Round(value / increment) * increment;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Snap(float value, float increment, float offset)
		{
			return (Round((value - offset) / increment) * increment) + offset;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Lerp(float from, float to, float t)
		{
			return from + (to - from) * Clamp01(t);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float InverseLerp(float from, float to, float t)
		{
			if (from < to)
			{
				if (t < from)
					return 0.0f;
				else if (t > to)
					return 1.0f;
			}
			else
			{
				if (t < to)
					return 1.0f;
				else if (t > from)
					return 0.0f;
			}

			return (t - from) / (to - from);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float UnclampedLerp(float from, float to, float t)
		{
			return from + (to - from) * t;
		}


		/// <summary>
		/// lerps an angle in degrees between a and b. handles wrapping around 360
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="t">T.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LerpAngle(float a, float b, float t)
		{
			float num = Repeat(b - a, 360f);
			if (num > 180f)
				num -= 360f;

			return a + num * Clamp01(t);
		}


		/// <summary>
		/// lerps an angle in radians between a and b. handles wrapping around 2*Pi
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="t">T.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LerpAngleRadians(float a, float b, float t)
		{
			float num = Repeat(b - a, MathHelper.TwoPi);
			if (num > MathHelper.Pi)
				num -= MathHelper.TwoPi;

			return a + num * Clamp01(t);
		}


		/// <summary>
		/// loops t so that it is never larger than length and never smaller than 0
		/// </summary>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Repeat(float t, float length)
		{
			return t - Floor(t / length) * length;
		}


		/// <summary>
		/// increments t and ensures it is always greater than or equal to 0 and less than length
		/// </summary>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int IncrementWithWrap(int t, int length)
		{
			t++;
			if (t == length)
				return 0;

			return t;
		}


		/// <summary>
		/// decrements t and ensures it is always greater than or equal to 0 and less than length
		/// </summary>
		/// <returns>The with wrap.</returns>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DecrementWithWrap(int t, int length)
		{
			t--;
			if (t < 0)
				return length - 1;

			return t;
		}


		/// <summary>
		/// ping-pongs t so that it is never larger than length and never smaller than 0
		/// </summary>
		/// <returns>The pong.</returns>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float PingPong(float t, float length)
		{
			t = Repeat(t, length * 2f);
			return length - Math.Abs(t - length);
		}


		/// <summary>
		/// if value >= threshold returns its sign else returns 0
		/// </summary>
		/// <returns>The threshold.</returns>
		/// <param name="value">Value.</param>
		/// <param name="threshold">Threshold.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SignThreshold(float value, float threshold)
		{
			if (Math.Abs(value) >= threshold)
				return Math.Sign(value);
			else
				return 0;
		}


		/// <summary>
		/// Calculates the shortest difference between two given angles in degrees
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="current">Current.</param>
		/// <param name="target">Target.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DeltaAngle(float current, float target)
		{
			var num = Repeat(target - current, 360f);
			if (num > 180f)
				num -= 360f;

			return num;
		}


		/// <summary>
		/// Calculates the shortest difference between two given angles given in radians
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="current">Current.</param>
		/// <param name="target">Target.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DeltaAngleRadians(float current, float target)
		{
			var num = Repeat(target - current, 2 * MathHelper.Pi);
			if (num > MathHelper.Pi)
				num -= 2 * MathHelper.Pi;

			return num;
		}


		/// <summary>
		/// moves start towards end by shift amount clamping the result. start can be less than or greater than end.
		/// example: start is 2, end is 10, shift is 4 results in 6
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="shift">Shift.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Approach(float start, float end, float shift)
		{
			if (start < end)
				return Math.Min(start + shift, end);

			return Math.Max(start - shift, end);
		}

		/// <summary>
		/// moves start angle towards end angle by shift amount clamping the result and choosing the shortest path. start can be less than or greater than end.
		/// example 1: start is 30, end is 100, shift is 25 results in 55
		/// example 2: start is 340, end is 30, shift is 25 results in 5 (365 is wrapped to 5)
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="shift">Shift.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ApproachAngle(float start, float end, float shift)
		{
			float deltaAngle = DeltaAngle(start, end);
			if (-shift < deltaAngle && deltaAngle < shift)
				return end;

			return Repeat(Approach(start, start + deltaAngle, shift), 360f);
		}

		/// <summary>
		/// moves start angle towards end angle by shift amount (all in radians) clamping the result and choosing the shortest path. start can be less than or greater than end.
		/// this method works very similar to approachAngle, the only difference is use of radians instead of degrees and wrapping at 2*Pi instead of 360.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="shift">Shift.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ApproachAngleRadians(float start, float end, float shift)
		{
			float deltaAngleRadians = DeltaAngleRadians(start, end);
			if (-shift < deltaAngleRadians && deltaAngleRadians < shift)
				return end;

			return Repeat(Approach(start, start + deltaAngleRadians, shift), MathHelper.TwoPi);
		}


		/// <summary>
		/// checks to see if two values are approximately the same using an acceptable tolerance for the check
		/// </summary>
		/// <param name="value1">Value1.</param>
		/// <param name="value2">Value2.</param>
		/// <param name="tolerance">Tolerance.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Approximately(float value1, float value2, float tolerance = Epsilon)
		{
			return Math.Abs(value1 - value2) <= tolerance;
		}


		/// <summary>
		/// returns the minimum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MinOf(float a, float b, float c)
		{
			return Math.Min(a, Math.Min(b, c));
		}


		/// <summary>
		/// returns the maximum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MaxOf(float a, float b, float c)
		{
			return Math.Max(a, Math.Max(b, c));
		}


		/// <summary>
		/// returns the minimum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MinOf(float a, float b, float c, float d)
		{
			return Math.Min(a, Math.Min(b, Math.Min(c, d)));
		}


		/// <summary>
		/// returns the minimum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MinOf(float a, float b, float c, float d, float e)
		{
			return Math.Min(a, Math.Min(b, Math.Min(c, Math.Min(d, e))));
		}


		/// <summary>
		/// returns the maximum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MaxOf(float a, float b, float c, float d)
		{
			return Math.Max(a, Math.Max(b, Math.Max(c, d)));
		}


		/// <summary>
		/// returns the maximum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MaxOf(float a, float b, float c, float d, float e)
		{
			return Math.Max(a, Math.Max(b, Math.Max(c, Math.Max(d, e))));
		}


		/// <summary>
		/// checks to see if value is between min/max inclusive of min/max
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Between(float value, float min, float max)
		{
			return value >= min && value <= max;
		}


		/// <summary>
		/// checks to see if value is between min/max inclusive of min/max
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Between(int value, int min, int max)
		{
			return value >= min && value <= max;
		}


		/// <summary>
		/// returns true if value is even
		/// </summary>
		/// <returns><c>true</c>, if even was ised, <c>false</c> otherwise.</returns>
		/// <param name="value">Value.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEven(int value)
		{
			return value % 2 == 0;
		}


		/// <summary>
		/// returns true if value is odd
		/// </summary>
		/// <returns><c>true</c>, if odd was ised, <c>false</c> otherwise.</returns>
		/// <param name="value">Value.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOdd(int value)
		{
			return value % 2 != 0;
		}


		/// <summary>
		/// rounds value and returns it and the amount that was rounded
		/// </summary>
		/// <returns>The with remainder.</returns>
		/// <param name="value">Value.</param>
		/// <param name="roundedAmount">roundedAmount.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RoundWithRoundedAmount(float value, out float roundedAmount)
		{
			var rounded = Round(value);
			roundedAmount = value - (rounded * Round(value / rounded));
			return rounded;
		}


		/// <summary>
		/// Maps a value from some arbitrary range to the 0 to 1 range
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Lminimum value.</param>
		/// <param name="max">maximum value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Map01(float value, float min, float max)
		{
			return (value - min) * 1f / (max - min);
		}


		/// <summary>
		/// Maps a value from some arbitrary range to the 1 to 0 range. this is just the reverse of map01
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Lminimum value.</param>
		/// <param name="max">maximum value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Map10(float value, float min, float max)
		{
			return 1f - Map01(value, min, max);
		}


		/// <summary>
		/// mapps value (which is in the range leftMin - leftMax) to a value in the range rightMin - rightMax
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="leftMin">Left minimum.</param>
		/// <param name="leftMax">Left max.</param>
		/// <param name="rightMin">Right minimum.</param>
		/// <param name="rightMax">Right max.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
		{
			return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
		}


		/// <summary>
		/// rounds value to the nearest number in steps of roundToNearest. Ex: found 127 to nearest 5 results in 125
		/// </summary>
		/// <returns>The to nearest.</returns>
		/// <param name="value">Value.</param>
		/// <param name="roundToNearest">Round to nearest.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RoundToNearest(float value, float roundToNearest)
		{
			return Round(value / roundToNearest) * roundToNearest;
		}

        /// <summary>
        /// Checks if the value passed falls under a certain threshold.
        /// Useful for small, precise comparisons.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="ep">The threshold to check the value with. <see cref="Epsilon"/> is used by default.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool WithinEpsilon(float value, float ep = Epsilon)
		{
			return Math.Abs(value) < ep;
		}


		/// <summary>
		/// returns sqrt( x * x + y * y )
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Hypotenuse(float x, float y)
		{
			return Sqrt(x * x + y * y);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ClosestPowerOfTwoGreaterThan(int x)
		{
			x--;
			x |= (x >> 1);
			x |= (x >> 2);
			x |= (x >> 4);
			x |= (x >> 8);
			x |= (x >> 16);

			return (x + 1);
		}


		#region wrappers for Math doubles

		/// <summary>
		/// Returns the square root
		/// </summary>
		/// <param name="val">Value.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(float val)
		{
			return (float)Math.Sqrt(val);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Pow(float x, float y)
		{
			return (float)Math.Pow(x, y);
		}


		/// <summary>
		/// Returns the sine of angle in radians
		/// </summary>
		/// <param name="f">F.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sin(float f)
		{
			return (float)Math.Sin(f);
		}


		/// <summary>
		/// Returns the cosine of angle in radians
		/// </summary>
		/// <param name="f">F.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cos(float f)
		{
			return (float)Math.Cos(f);
		}


		/// <summary>
		/// Returns the arc-cosine of f: the angle in radians whose cosine is f
		/// </summary>
		/// <param name="f">F.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Acos(float f)
		{
			return (float)Math.Acos(f);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Exp(float power)
		{
			return (float)Math.Exp(power);
		}


		/// <summary>
		/// returns the angle whose tangent is the quotient (y/x) of two specified numbers
		/// </summary>
		/// <param name="y">The y coordinate.</param>
		/// <param name="x">The x coordinate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan2(float y, float x)
		{
			return (float)Math.Atan2(y, x);
		}

		#endregion


		#region Vector2

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float AngleBetweenVectors(Vector2 from, Vector2 to)
		{
			return Atan2(to.Y - from.Y, to.X - from.X);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 AngleToVector(float angleRadians, float length)
		{
			return new Vector2(Cos(angleRadians) * length, Sin(angleRadians) * length);
		}


		/// <summary>
		/// helper for moving a value around in a circle.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RotateAround(Vector2 position, float speed)
		{
			var time = Time.TotalTime * speed;

			var x = Cos(time);
			var y = Sin(time);

			return new Vector2(position.X + x, position.Y + y);
		}


		/// <summary>
		/// the rotation is relative to the current position not the total rotation. For example, if you are currently at 90 degrees and
		/// want to rotate to 135 degrees, you would use an angle of 45, not 135.
		/// </summary>
		/// <returns>The around.</returns>
		/// <param name="point">Point.</param>
		/// <param name="center">Center.</param>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RotateAround(Vector2 point, Vector2 center, float angleInDegrees)
		{
			angleInDegrees = MathHelper.ToRadians(angleInDegrees);
			var cos = Cos(angleInDegrees);
			var sin = Sin(angleInDegrees);
			var rotatedX = cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X;
			var rotatedY = sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y;

			return new Vector2(rotatedX, rotatedY);
		}


		/// <summary>
		/// the rotation is relative to the current position not the total rotation. For example, if you are currently at 1 Pi radians and
		/// want to rotate to 1.5 Pi radians, you would use an angle of 0.5 Pi, not 1.5 Pi.
		/// </summary>
		/// <returns>The around.</returns>
		/// <param name="point">Point.</param>
		/// <param name="center">Center.</param>
		/// <param name="angleInDegrees">Angle in radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RotateAroundRadians(Vector2 point, Vector2 center, float angleInRadians)
		{
			var cos = Cos(angleInRadians);
			var sin = Sin(angleInRadians);
			var rotatedX = cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X;
			var rotatedY = sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y;

			return new Vector2(rotatedX, rotatedY);
		}


		/// <summary>
		/// gets a point on the circumference of the circle given its center, radius and angle. 0 degrees is 3 o'clock.
		/// </summary>
		/// <returns>The on circle.</returns>
		/// <param name="circleCenter">Circle center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 PointOnCircle(Vector2 circleCenter, float radius, float angleInDegrees)
		{
			var radians = MathHelper.ToRadians(angleInDegrees);
			return new Vector2
			{
				X = Cos(radians) * radius + circleCenter.X,
				Y = Sin(radians) * radius + circleCenter.Y
			};
		}


		/// <summary>
		/// gets a point on the circumference of the circle given its center, radius and angle. 0 radians is 3 o'clock.
		/// </summary>
		/// <returns>The on circle.</returns>
		/// <param name="circleCenter">Circle center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="angleInDegrees">Angle in radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 PointOnCircleRadians(Vector2 circleCenter, float radius, float angleInRadians)
		{
			return new Vector2
			{
				X = Cos(angleInRadians) * radius + circleCenter.X,
				Y = Sin(angleInRadians) * radius + circleCenter.Y
			};
		}


		/// <summary>
		/// lissajou curve
		/// </summary>
		/// <param name="xFrequency">X frequency.</param>
		/// <param name="yFrequency">Y frequency.</param>
		/// <param name="xMagnitude">X magnitude.</param>
		/// <param name="yMagnitude">Y magnitude.</param>
		/// <param name="phase">Phase.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Lissajou(float xFrequency = 2f, float yFrequency = 3f, float xMagnitude = 1,
									   float yMagnitude = 1, float phase = 0)
		{
			var x = Sin(Time.TotalTime * xFrequency + phase) * xMagnitude;
			var y = Cos(Time.TotalTime * yFrequency) * yMagnitude;

			return new Vector2(x, y);
		}


		/// <summary>
		/// damped version of a lissajou curve with oscillation between 0 and max magnitude over time. Damping should be between 0 and 1 for best
		/// results. oscillationInterval is the time in seconds for half of the animation loop to complete.
		/// </summary>
		/// <returns>The damped.</returns>
		/// <param name="xFrequency">X frequency.</param>
		/// <param name="yFrequency">Y frequency.</param>
		/// <param name="xMagnitude">X magnitude.</param>
		/// <param name="yMagnitude">Y magnitude.</param>
		/// <param name="phase">Phase.</param>
		/// <param name="damping">Damping.</param>
		/// <param name="oscillationInterval">Oscillation interval.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 LissajouDamped(float xFrequency = 2f, float yFrequency = 3f, float xMagnitude = 1,
											 float yMagnitude = 1, float phase = 0.5f, float damping = 0f,
											 float oscillationInterval = 5f)
		{
			var wrappedTime = PingPong(Time.TotalTime, oscillationInterval);
			var damped = Pow(MathHelper.E, -damping * wrappedTime);

			var x = damped * Sin(Time.TotalTime * xFrequency + phase) * xMagnitude;
			var y = damped * Cos(Time.TotalTime * yFrequency) * yMagnitude;

			return new Vector2(x, y);
		}

		#endregion
	}
}