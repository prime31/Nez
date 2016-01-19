using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Vector2Ext
	{
		/// <summary>
		/// rounds the x and y values in place
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static Vector2 round( this Vector2 vec )
		{
			return new Vector2( Mathf.round( vec.X ), Mathf.round( vec.Y ) );
		}


		/// <summary>
		/// returns a 0.5, 0.5 vector
		/// </summary>
		/// <returns>The vector.</returns>
		public static Vector2 halfVector()
		{
			return new Vector2( 0.5f, 0.5f );
		}


		/// <summary>
		/// compute the 2d pseudo cross product Dot( Perp( u ), v )
		/// </summary>
		/// <param name="u">U.</param>
		/// <param name="v">V.</param>
		public static float cross( Vector2 u, Vector2 v )
		{
			return u.Y * v.X - u.X * v.Y;
		}
	}
}

