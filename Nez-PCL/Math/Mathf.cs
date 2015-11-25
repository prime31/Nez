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


		public static float minOf( float a, float b, float c, float d )
		{
			return Math.Min( a, Math.Min( b, Math.Min( c, d ) ) );
		}


		public static float maxOf( float a, float b, float c, float d )
		{
			return Math.Max( a, Math.Max( b, Math.Max( c, d ) ) );
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

		static public float angleBetweenVectors( Vector2 from, Vector2 to )
		{
			return (float)Math.Atan2( to.Y - from.Y, to.X - from.X );
		}


		static public Vector2 angleToVector( float angleRadians, float length )
		{
			return new Vector2( (float)Math.Cos( angleRadians ) * length, (float)Math.Sin( angleRadians ) * length );
		}


		static public void floor( ref Vector2 val )
		{
			val.X = (int)val.X;
			val.Y = (int)val.Y;
		}


		static public Vector2 floor( Vector2 val )
		{
			return new Vector2( (int)val.X, (int)val.Y );
		}


		/// <summary>
		/// rounds the x and y values in place
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static void round( ref Vector2 vec )
		{
			vec.X = Mathf.round( vec.X );
			vec.Y = Mathf.round( vec.Y );
		}


		/// <summary>
		/// rounds the x and y values and returns a new Vector2
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static Vector2 round( Vector2 vec )
		{
			return new Vector2( Mathf.round( vec.X ), Mathf.round( vec.Y ) );
		}


		/// <summary>
		/// helper for moving a value around in a circle.
		/// </summary>
		static public Vector2 rotateAround( Vector2 position, float speed )
		{
			var time = Time.time * speed;

			var x = (float)Math.Cos( time );
			var y = (float)Math.Sin( time );

			return new Vector2( position.X + x, position.Y + y );
		}


		static public Vector2 perpendicularVector( Vector2 vector )
		{
			return new Vector2( -vector.Y, vector.X );
		}

		#endregion

	}
}

