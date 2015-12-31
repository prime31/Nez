using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Circle : Shape
	{
		public float radius;


		public Circle( float radius )
		{
			this.radius = radius;
		}


		internal override void recalculateBounds( Collider collider )
		{
			bounds = RectangleExt.fromFloats( collider.entity.position.X + collider.localPosition.X + collider.origin.X - radius, collider.entity.position.Y + collider.localPosition.Y + collider.origin.Y - radius, radius * 2f, radius * 2f );
		}


		public override bool collidesWithShape( Shape other, Vector2 deltaMovement, out ShapeCollisionResult result )
		{
			result = new ShapeCollisionResult();

			if( other is Circle )
				return ShapeCollisions.circleToCircle( this, other as Circle, out result );

			if( other is Polygon )
				return ShapeCollisions.circleToPolygon( this, other as Polygon, out result );

			return false;
		}


		public override bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit )
		{
			hit = new RaycastHit();
			return ShapeCollisions.lineToCircle( start, end, this, out hit );
		}

	}
}

