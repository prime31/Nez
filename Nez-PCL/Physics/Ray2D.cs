using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Ray2D
	{
		public Vector2 position;
		public Vector2 direction;

		
		public Ray2D( Vector2 position, Vector2 direction )
		{
			this.position = position;
			this.direction = direction;
		}
	}
}

