using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Mathf
	{
		public static float round( float f )
		{
			return (float)Math.Round( (double)f );
		}


		public static float floor( float f )
		{
			return (float)Math.Floor( (double)f );
		}


		public static int ceilToInt( float f )
		{
			return (int)Math.Ceiling( (double)f );
		}


		public static int floorToInt( float f )
		{
			return (int)Math.Floor( (double)f );
		}


		public static int roundToInt( float f )
		{
			return (int)Math.Round( (double)f );
		}


		public static float clamp01( float value )
		{
			if( value < 0f )
				return 0f;

			if( value > 1f )
				return 1f;

			return value;
		}


		public static float clamp( float value, float min, float max )
		{
			if( value < min )
				return min;

			if( value > max )
				return max;

			return value;
		}


		static public float snap( float value, float increment )
		{
			return round( value / increment ) * increment;
		}


		static public float snap( float value, float increment, float offset )
		{
			return ( round( ( value - offset ) / increment ) * increment ) + offset;
		}


		public static float lerp( float from, float to, float t )
		{
			return from + ( to - from ) * Mathf.clamp01( t );
		}


		public static float unclampedLerp( float from, float to, float t )
		{
			return from + ( to - from ) * t;
		}


		public static float lerpAngle( float a, float b, float t )
		{
			float num = Mathf.repeat( b - a, 360f );
			if( num > 180f )
			{
				num -= 360f;
			}
			return a + num * clamp01( t );
		}


		public static float repeat( float t, float length )
		{
			return t - Mathf.floor( t / length ) * length;
		}


		public static float pingPong( float t, float length )
		{
			t = Mathf.repeat( t, length * 2f );
			return length - Math.Abs( t - length );
		}


		static public float signThreshold( float value, float threshold )
		{
			if( Math.Abs( value ) >= threshold )
				return Math.Sign( value );
			else
				return 0;
		}


		public static float deltaAngle( float current, float target )
		{
			var num = Mathf.repeat( target - current, 360f );
			if( num > 180f )
				num -= 360f;

			return num;
		}


		#region wrappers for Math doubles

		public static float sqrt( float val )
		{
			return (float)Math.Sqrt( val );
		}


		public static float pow( float x, float y )
		{
			return (float)Math.Pow( x, y );
		}


		public static float sin( float f )
		{
			return (float)Math.Sin( f );
		}


		public static float cos( float f )
		{
			return (float)Math.Cos( f );
		}


		public static float exp( float power )
		{
			return (float)Math.Exp( power );
		}

		#endregion


		#region Vector2

		static public Vector2 perpendicular( this Vector2 vector )
		{
			return new Vector2( -vector.Y, vector.X );
		}


		static public float angleBetweenVectors( Vector2 from, Vector2 to )
		{
			return (float)Math.Atan2( to.Y - from.Y, to.X - from.X );
		}


		static public Vector2 angleToVector( float angleRadians, float length )
		{
			return new Vector2( (float)Math.Cos( angleRadians ) * length, (float)Math.Sin( angleRadians ) * length );
		}

		#endregion

	}
}

