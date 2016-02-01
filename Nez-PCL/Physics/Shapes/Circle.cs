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
			position = collider.absolutePosition;
			bounds = RectangleExt.fromFloats( collider.entity.transform.position.X + collider.localPosition.X + collider.origin.X - radius, collider.entity.transform.position.Y + collider.localPosition.Y + collider.origin.Y - radius, radius * 2f, radius * 2f );
		}


		public override bool overlaps( Shape other )
		{
			ShapeCollisionResult result;
			if( other is Box )
				return Collisions.rectToCircle( ref other.bounds, position, radius );

			if( other is Circle )
				return Collisions.circleToCircle( position, radius, other.position, ( other as Circle ).radius );

			if( other is Polygon )
				return ShapeCollisions.circleToPolygon( this, other as Polygon, out result );

			throw new NotImplementedException( string.Format( "overlaps of Circle to {0} are not supported", other ) );
		}


		public override bool collidesWithShape( Shape other, out ShapeCollisionResult result )
		{
			if( other is Box )
				return ShapeCollisions.circleToBox( this, other as Box, out result );

			if( other is Circle )
				return ShapeCollisions.circleToCircle( this, other as Circle, out result );

			if( other is Polygon )
				return ShapeCollisions.circleToPolygon( this, other as Polygon, out result );

			throw new NotImplementedException( string.Format( "Collisions of Circle to {0} are not supported", other ) );
		}


		public override bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit )
		{
			hit = new RaycastHit();
			return ShapeCollisions.lineToCircle( start, end, this, out hit );
		}

	}
}

