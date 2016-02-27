using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public abstract class Shape
	{
		/// <summary>
		/// having a separate position field lets use alter the position of the shape for collisions checks as opposed to having to change the
		/// Entity.position which triggers collider/bounds/hash updates.
		/// </summary>
		public Vector2 position;
		public RectangleF bounds;


		internal abstract void recalculateBounds( Collider collider );

		public abstract bool overlaps( Shape other );

		public abstract bool collidesWithShape( Shape other, out CollisionResult result );

		public abstract bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit );


		public virtual Shape clone()
		{
			return MemberwiseClone() as Shape;
		}

	}
}

