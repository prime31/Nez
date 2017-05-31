using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Random
	{
		private static int _seed = Environment.TickCount;
		public static System.Random random = new System.Random( _seed );


		/// <summary>
		/// returns current seed value
		/// </summary>
		/// <returns>Seed.</returns>
		static public int getSeed()
		{
			return _seed;
		}


		/// <summary>
		/// resets rng with new seed
		/// </summary>
		/// <param name="seed">Seed.</param>
		static public void setSeed( int seed )
		{
			_seed = seed;
			random = new System.Random( _seed );
		}


		/// <summary>
		/// returns a random float between 0 (inclusive) and 1 (exclusive)
		/// </summary>
		/// <returns>The float.</returns>
		static public float nextFloat()
		{
			return (float)random.NextDouble();
		}


		/// <summary>
		/// returns a random float between 0 (inclusive) and max (exclusive)
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="max">Max.</param>
		static public float nextFloat( float max )
		{
			return (float)random.NextDouble() * max;
		}


		/// <summary>
		/// returns a random int between 0 (inclusive) and max (exclusive)
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="max">Max.</param>
		static public int nextInt( int max )
		{
			return random.Next( max );
		}


		/// <summary>
		/// returns a random float between 0 and 2 * PI
		/// </summary>
		/// <returns>The angle.</returns>
		static public float nextAngle()
		{
			return (float)random.NextDouble() * MathHelper.TwoPi;
		}


		/// <summary>
		/// returns a random color
		/// </summary>
		/// <returns>The color.</returns>
		public static Color nextColor()
		{
			return new Color( nextFloat(), nextFloat(), nextFloat() );
		}


		/// <summary>
		/// Returns a random integer between min (inclusive) and max (exclusive)
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		static public int range( int min, int max )
		{
			return random.Next( min, max );
		}


		/// <summary>
		/// Returns a random float between min (inclusive) and max (exclusive)
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		static public float range( float min, float max )
		{
			return min + nextFloat( max - min );
		}


		/// <summary>
		/// Returns a random Vector2, and x- and y-values of which are between min (inclusive) and max (exclusive)
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		static public Vector2 range( Vector2 min, Vector2 max )
		{
			return min + new Vector2( nextFloat( max.X - min.X ), nextFloat( max.Y - min.Y ) );
		}


		/// <summary>
		/// returns a random float between -1 and 1
		/// </summary>
		/// <returns>The one to one.</returns>
		static public float minusOneToOne()
		{
			return nextFloat( 2f ) - 1f;
		}


		/// <summary>
		/// returns true if the next random is less than percent. Percent should be between 0 and 1
		/// </summary>
		/// <param name="percent">Percent.</param>
		public static bool chance( float percent )
		{
			return nextFloat() < percent;
		}


		/// <summary>
		/// returns true if the next random is less than value. Value should be between 0 and 100.
		/// </summary>
		/// <param name="value">Value.</param>
		public static bool chance( int value )
		{
			return nextInt( 100 ) < value;
		}


		/// <summary>
		/// randomly returns one of the given values
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T choose<T>( T first, T second )
		{
			if( nextInt( 2 ) == 0 )
				return first;
			return second;
		}


		/// <summary>
		/// randomly returns one of the given values
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="third">Third.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T choose<T>( T first, T second, T third )
		{
			switch( nextInt( 3 ) )
			{
				case 0:
				return first;
				case 1:
				return second;
				default:
				return third;
			}
		}


		/// <summary>
		/// randomly returns one of the given values
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="third">Third.</param>
		/// <param name="fourth">Fourth.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T choose<T>( T first, T second, T third, T fourth )
		{
			switch( nextInt( 4 ) )
			{
				case 0:
				return first;
				case 1:
				return second;
				case 2:
				return third;
				default:
				return fourth;
			}
		}

	}
}

