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



		/// <summary>
		/// helper for moving a value around in a circle.
		/// </summary>
		static Vector2 rotateAround( Vector2 position, float speed )
		{
			var time = Time.time * speed;

			var x = (float)Math.Cos( time );
			var y = (float)Math.Sin( time );

			return new Vector2( position.X + x, position.Y + y );
		}
	}
}

