using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Mathf
	{
		public const float epsilon = 0.00001f;
		public const float deg2Rad = 0.0174532924f;
		public const float rad2Deg = 57.29578f;


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float round( float f )
		{
			return (float)Math.Round( f );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float ceil( float f )
		{
			return (float)Math.Ceiling( f );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int ceilToInt( float f )
		{
			return (int)Math.Ceiling( (double)f );
		}


		/// <summary>
		/// ceils the float to the nearest int value above y. note that this only works for values in the range of short
		/// </summary>
		/// <returns>The ceil to int.</returns>
		/// <param name="y">F.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int fastCeilToInt( float y )
		{
			return 32768 - (int)( 32768f - y );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float floor( float f )
		{
			return (float)Math.Floor( f );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int floorToInt( float f )
		{
			return (int)Math.Floor( (double)f );
		}


		/// <summary>
		/// floors the float to the nearest int value below x. note that this only works for values in the range of short
		/// </summary>
		/// <returns>The floor to int.</returns>
		/// <param name="x">The x coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int fastFloorToInt( float x )
		{
			// we shift to guaranteed positive before casting then shift back after
			return (int)( x + 32768f ) - 32768;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int roundToInt( float f )
		{
			return (int)Math.Round( f );
		}


		/// <summary>
		/// Calculates the integral part of a number cast to an int
		/// </summary>
		/// <returns>The to int.</returns>
		/// <param name="f">F.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int truncateToInt( float f )
		{
			return (int)Math.Truncate( f );
		}


		/// <summary>
		/// clamps value between 0 and 1
		/// </summary>
		/// <param name="value">Value.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float clamp01( float value )
		{
			if( value < 0f )
				return 0f;

			if( value > 1f )
				return 1f;

			return value;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float clamp( float value, float min, float max )
		{
			if( value < min )
				return min;

			if( value > max )
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int clamp( int value, int min, int max )
		{ 
			value = ( value > max ) ? max : value;
			value = ( value < min ) ? min : value;

			return value;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public float snap( float value, float increment )
		{
			return round( value / increment ) * increment;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public float snap( float value, float increment, float offset )
		{
			return ( round( ( value - offset ) / increment ) * increment ) + offset;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float lerp( float from, float to, float t )
		{
			return from + ( to - from ) * Mathf.clamp01( t );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float inverseLerp( float from, float to, float t )
		{
			if( from < to )
			{
				if( t < from )
					return 0.0f;
				else if( t > to )
					return 1.0f;
			}
			else
			{
				if( t < to )
					return 1.0f;
				else if( t > from )
					return 0.0f;
			}

			return ( t - from ) / ( to - from );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float unclampedLerp( float from, float to, float t )
		{
			return from + ( to - from ) * t;
		}


		/// <summary>
		/// lerps an angle in degrees between a and b. handles wrapping around 360
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="t">T.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float lerpAngle( float a, float b, float t )
		{
			float num = Mathf.repeat( b - a, 360f );
			if( num > 180f )
				num -= 360f;
			
			return a + num * clamp01( t );
		}


		/// <summary>
		/// loops t so that it is never larger than length and never smaller than 0
		/// </summary>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float repeat( float t, float length )
		{
			return t - Mathf.floor( t / length ) * length;
		}


		/// <summary>
		/// increments t and ensures it is always greater than or equal to 0 and less than length
		/// </summary>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int incrementWithWrap( int t, int length )
		{
			t++;
			if( t == length )
				return 0;
			return t;
		}


		/// <summary>
		/// decrements t and ensures it is always greater than or equal to 0 and less than length
		/// </summary>
		/// <returns>The with wrap.</returns>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int decrementWithWrap( int t, int length )
		{
			t--;
			if( t < 0 )
				return length - 1;
			return t;
		}


		/// <summary>
		/// ping-pongs t so that it is never larger than length and never smaller than 0
		/// </summary>
		/// <returns>The pong.</returns>
		/// <param name="t">T.</param>
		/// <param name="length">Length.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float pingPong( float t, float length )
		{
			t = Mathf.repeat( t, length * 2f );
			return length - Math.Abs( t - length );
		}


		/// <summary>
		/// if value >= threshold returns its sign else returns 0
		/// </summary>
		/// <returns>The threshold.</returns>
		/// <param name="value">Value.</param>
		/// <param name="threshold">Threshold.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public float signThreshold( float value, float threshold )
		{
			if( Math.Abs( value ) >= threshold )
				return Math.Sign( value );
			else
				return 0;
		}


		/// <summary>
		/// Calculates the shortest difference between two given angles in degrees
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="current">Current.</param>
		/// <param name="target">Target.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float deltaAngle( float current, float target )
		{
			var num = Mathf.repeat( target - current, 360f );
			if( num > 180f )
				num -= 360f;

			return num;
		}


		/// <summary>
		/// Calculates the shortest difference between two given angles given in radians
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="current">Current.</param>
		/// <param name="target">Target.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float deltaAngleRadians( float current, float target )
		{
			var num = repeat( target - current, 2 * MathHelper.Pi );
			if( num > MathHelper.Pi )
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float approach( float start, float end, float shift )
		{
			if( start < end )
				return Math.Min( start + shift, end );
			return Math.Max( start - shift, end );
		}


		/// <summary>
		/// checks to see if two values are approximately the same using an acceptable tolerance for the check
		/// </summary>
		/// <param name="value1">Value1.</param>
		/// <param name="value2">Value2.</param>
		/// <param name="tolerance">Tolerance.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool approximately( float value1, float value2, float tolerance = epsilon )
		{
			return Math.Abs( value1 - value2 ) <= tolerance;
		}


		/// <summary>
		/// returns the minimum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float minOf( float a, float b, float c )
		{
			return Math.Min( a, Math.Min( b, c ) );
		}


		/// <summary>
		/// returns the maximum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float maxOf( float a, float b, float c )
		{
			return Math.Max( a, Math.Max( b, c ) );
		}


		/// <summary>
		/// returns the minimum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float minOf( float a, float b, float c, float d )
		{
			return Math.Min( a, Math.Min( b, Math.Min( c, d ) ) );
		}


		/// <summary>
		/// returns the minimum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float minOf( float a, float b, float c, float d, float e )
		{
			return Math.Min( a, Math.Min( b, Math.Min( c, Math.Min( d, e ) ) ) );
		}


		/// <summary>
		/// returns the maximum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float maxOf( float a, float b, float c, float d )
		{
			return Math.Max( a, Math.Max( b, Math.Max( c, d ) ) );
		}


		/// <summary>
		/// returns the maximum of the passed in values
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float maxOf( float a, float b, float c, float d, float e )
		{
			return Math.Max( a, Math.Max( b, Math.Max( c, Math.Max( d, e ) ) ) );
		}


		/// <summary>
		/// checks to see if value is between min/max inclusive of min/max
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool between( float value, float min, float max )
		{
			return value >= min && value <= max;
		}


		/// <summary>
		/// returns true if value is even
		/// </summary>
		/// <returns><c>true</c>, if even was ised, <c>false</c> otherwise.</returns>
		/// <param name="value">Value.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool isEven( int value )
		{
			return value % 2 == 0;
		}


		/// <summary>
		/// returns true if value is odd
		/// </summary>
		/// <returns><c>true</c>, if odd was ised, <c>false</c> otherwise.</returns>
		/// <param name="value">Value.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool isOdd( int value )
		{
			return value % 2 != 0;
		}


		/// <summary>
		/// rounds value and returns it and the amount that was rounded
		/// </summary>
		/// <returns>The with remainder.</returns>
		/// <param name="value">Value.</param>
		/// <param name="roundedAmount">roundedAmount.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float roundWithRoundedAmount( float value, out float roundedAmount )
		{
			var rounded = Mathf.round( value );
			roundedAmount = value - ( rounded * Mathf.round( value / rounded ) );
			return rounded;
		}


		/// <summary>
		/// Maps a value from some arbitrary range to the 0 to 1 range
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Lminimum value.</param>
		/// <param name="max">maximum value</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float map01( float value, float min, float max )
		{
			return ( value - min ) * 1f / ( max - min );
		}


		/// <summary>
		/// Maps a value from some arbitrary range to the 1 to 0 range. this is just the reverse of map01
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Lminimum value.</param>
		/// <param name="max">maximum value</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float map10( float value, float min, float max )
		{
			return 1f - map01( value, min, max );
		}


		/// <summary>
		/// mapps value (which is in the range leftMin - leftMax) to a value in the range rightMin - rightMax
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="leftMin">Left minimum.</param>
		/// <param name="leftMax">Left max.</param>
		/// <param name="rightMin">Right minimum.</param>
		/// <param name="rightMax">Right max.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float map( float value, float leftMin, float leftMax, float rightMin, float rightMax )
		{
			return rightMin + ( value - leftMin ) * ( rightMax - rightMin ) / ( leftMax - leftMin );
		}


		/// <summary>
		/// rounds value to the nearest number in steps of roundToNearest. Ex: found 127 to nearest 5 results in 125
		/// </summary>
		/// <returns>The to nearest.</returns>
		/// <param name="value">Value.</param>
		/// <param name="roundToNearest">Round to nearest.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float roundToNearest( float value, float roundToNearest )
		{
			return round( value / roundToNearest ) * roundToNearest;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool withinEpsilon( float floatA, float floatB )
		{
			return Math.Abs( floatA - floatB ) < epsilon;
		}


		/// <summary>
		/// returns sqrt( x * x + y * y )
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float hypotenuse( float x, float y )
		{
			return sqrt( x * x + y * y );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static int closestPowerOfTwoGreaterThan( int x )
		{
			x--;
			x |= ( x >> 1 );
			x |= ( x >> 2 );
			x |= ( x >> 4 );
			x |= ( x >> 8 );
			x |= ( x >> 16 );

			return ( x + 1 );
		}


		#region wrappers for Math doubles

		/// <summary>
		/// Returns the square root
		/// </summary>
		/// <param name="val">Value.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float sqrt( float val )
		{
			return (float)Math.Sqrt( val );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float pow( float x, float y )
		{
			return (float)Math.Pow( x, y );
		}


		/// <summary>
		/// Returns the sine of angle in radians
		/// </summary>
		/// <param name="f">F.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float sin( float f )
		{
			return (float)Math.Sin( f );
		}


		/// <summary>
		/// Returns the cosine of angle in radians
		/// </summary>
		/// <param name="f">F.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float cos( float f )
		{
			return (float)Math.Cos( f );
		}


		/// <summary>
		/// Returns the arc-cosine of f: the angle in radians whose cosine is f
		/// </summary>
		/// <param name="f">F.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float acos( float f )
		{
			return (float)Math.Acos( f );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float exp( float power )
		{
			return (float)Math.Exp( power );
		}


		/// <summary>
		/// returns the angle whose tangent is the quotient (y/x) of two specified numbers
		/// </summary>
		/// <param name="y">The y coordinate.</param>
		/// <param name="x">The x coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float atan2( float y, float x )
		{
			return (float)Math.Atan2( y, x );
		}

		#endregion


		#region Vector2

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public float angleBetweenVectors( Vector2 from, Vector2 to )
		{
			return Mathf.atan2( to.Y - from.Y, to.X - from.X );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public Vector2 angleToVector( float angleRadians, float length )
		{
			return new Vector2( (float)Math.Cos( angleRadians ) * length, (float)Math.Sin( angleRadians ) * length );
		}


		/// <summary>
		/// helper for moving a value around in a circle.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public Vector2 rotateAround( Vector2 position, float speed )
		{
			var time = Time.time * speed;

			var x = cos( time );
			var y = sin( time );

			return new Vector2( position.X + x, position.Y + y );
		}


		/// <summary>
		/// the rotation is relative to the current position not the total rotation. For example, if you are currently at 90 degrees and
		/// want to rotate to 135 degrees, you would use an angle of 45, not 135.
		/// </summary>
		/// <returns>The around.</returns>
		/// <param name="point">Point.</param>
		/// <param name="center">Center.</param>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 rotateAround( Vector2 point, Vector2 center, float angleInDegrees )
		{
			angleInDegrees = MathHelper.ToRadians( angleInDegrees );
			var cos = Mathf.cos( angleInDegrees );
			var sin = Mathf.sin( angleInDegrees );
			var rotatedX = cos * ( point.X - center.X ) - sin * ( point.Y - center.Y ) + center.X;
			var rotatedY = sin * ( point.X - center.X ) + cos * ( point.Y - center.Y ) + center.Y;

			return new Vector2( rotatedX, rotatedY );
		}


		/// <summary>
		/// gets a point on the circumference of the circle given its center, radius and angle. 0 degrees is 3 o'clock.
		/// </summary>
		/// <returns>The on circle.</returns>
		/// <param name="circleCenter">Circle center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 pointOnCircle( Vector2 circleCenter, float radius, float angleInDegrees )
		{
			var radians = MathHelper.ToRadians( angleInDegrees );
			return new Vector2
			{
				X = cos( radians ) * radius + circleCenter.X,
				Y = sin( radians ) * radius + circleCenter.Y
			};
		}

		#endregion

	}
}

