using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public struct Ray2D
	{
		public Vector2 start;
		public Vector2 end;
		public Vector2 direction;

		
		public Ray2D( Vector2 position, Vector2 end )
		{
			this.start = position;
			this.end = end;
			direction = end - start;
		}
	}
}

