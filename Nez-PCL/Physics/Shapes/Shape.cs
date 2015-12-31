using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public abstract class Shape
	{
		public Vector2 position;
		public Rectangle bounds;


		internal abstract void recalculateBounds( Collider collider );

		public abstract bool collidesWithShape( Shape other, Vector2 deltaMovement, out ShapeCollisionResult result );

		public abstract bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit );

	}
}

