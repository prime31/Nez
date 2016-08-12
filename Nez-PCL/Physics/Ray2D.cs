﻿using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// while technically not a ray (rays are just start and direction) it does double duty as both a line and a ray.
	/// </summary>
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

