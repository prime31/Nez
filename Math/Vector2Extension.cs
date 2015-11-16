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
		public static void round( this Vector2 vec )
		{
			vec.X = Mathf.round( vec.X );
			vec.Y = Mathf.round( vec.Y );
		}
	}
}

