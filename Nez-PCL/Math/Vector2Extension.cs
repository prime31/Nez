using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Vector2Extension
	{
		/// <summary>
		/// rounds the x and y values in place
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static Vector2 round( this Vector2 vec )
		{
			return new Vector2( Mathf.round( vec.X ), Mathf.round( vec.Y ) );
		}
	}
}

