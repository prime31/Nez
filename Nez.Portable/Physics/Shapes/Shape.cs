using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public abstract class Shape
	{
		/// <summary>
		/// having a separate position field lets us alter the position of the shape for collisions checks as opposed to having to change the
		/// Entity.position which triggers collider/bounds/hash updates.
		/// </summary>
		internal Vector2 position;

		/// <summary>
		/// center is kind of a misnomer. This value isnt necessarily the center of an object. It is more accurately the Collider.localOffset
		/// with any Transform rotations applied
		/// </summary>
		internal Vector2 center;

		/// <summary>
		/// cached bounds for the Shape
		/// </summary>
		internal RectangleF bounds;


		internal abstract void recalculateBounds( Collider collider );

		public abstract bool overlaps( Shape other );

		public abstract bool collidesWithShape( Shape other, out CollisionResult result );

		public abstract bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit );

		public abstract bool containsPoint( Vector2 point );

		public abstract bool pointCollidesWithShape( Vector2 point, out CollisionResult result );


		public virtual Shape clone()
		{
			return MemberwiseClone() as Shape;
		}

	}
}

