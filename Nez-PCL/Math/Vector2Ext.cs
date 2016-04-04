using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Vector2Ext
	{
		#if FNA
		/// <summary>
		/// Gets a <see cref="Point"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Point"/> representation for this object.</returns>
		public static Point ToPoint( this Vector2 self )
		{
			return new Point( (int)self.X, (int)self.Y );
		}
		#endif
		
	
		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static Vector2 round( this Vector2 vec )
		{
			return new Vector2( Mathf.round( vec.X ), Mathf.round( vec.Y ) );
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

