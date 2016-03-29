using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Random
	{
		public static System.Random random = new System.Random();


		/// <summary>
		/// returns a random float between 0 and 1
		/// </summary>
		/// <returns>The float.</returns>
		static public float nextFloat()
		{
			return (float)random.NextDouble();
		}


		/// <summary>
		/// returns a random float between 0 and max
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="max">Max.</param>
		static public float nextFloat( float max )
		{
			return nextFloat() * max;
		}


		/// <summary>
		/// returns a random float between 0 and 2 * PI
		/// </summary>
		/// <returns>The angle.</returns>
		static public float nextAngle()
		{
			return nextFloat() * MathHelper.TwoPi;
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
		/// <param name="random"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		static public int range( int min, int max )
		{
			return min + random.Next( max - min );
		}


		/// <summary>
		/// Returns a random float between min (inclusive) and max (exclusive)
		/// </summary>
		/// <param name="random"></param>
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
		/// <param name="random"></param>
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
		/// returns true if the next random is < percent. Percent should be between 0 and 1
		/// </summary>
		/// <param name="percent">Percent.</param>
		public static bool chance( float percent )
		{
			return nextFloat() < percent;
		}

	}
}

