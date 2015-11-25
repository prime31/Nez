using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Random
	{
		public static System.Random random = new System.Random();


		static public float nextFloat()
		{
			return (float)random.NextDouble();
		}


		static public float nextFloat( float max )
		{
			return nextFloat() * max;
		}


		static public float nextAngle()
		{
			return nextFloat() * MathHelper.TwoPi;
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
	}
}

