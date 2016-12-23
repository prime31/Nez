using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public static partial class ShapeCollisions
	{
		public static bool pointToCircle( Vector2 point, Circle circle, out CollisionResult result )
		{
			result = new CollisionResult();

			// avoid the square root until we actually need it
			var distanceSquared = Vector2.DistanceSquared( point, circle.position );
			var sumOfRadii = 1 + circle.radius;
			var collided = distanceSquared < sumOfRadii * sumOfRadii;
			if( collided )
			{
				result.normal = Vector2.Normalize( point - circle.position );
				var depth = sumOfRadii - Mathf.sqrt( distanceSquared );
				result.minimumTranslationVector = -depth * result.normal;
				result.point = circle.position + result.normal * circle.radius;

				return true;
			}

			return false;
		}


		public static bool pointToBox( Vector2 point, Box box, out CollisionResult result )
		{
			result = new CollisionResult();

			if( box.containsPoint( point ) )
			{
				// get the point in the space of the Box
				result.point = box.bounds.getClosestPointOnRectangleBorderToPoint( point, out result.normal );
				result.minimumTranslationVector = point - result.point;

				return true;
			}

			return false;
		}


		public static bool pointToPoly( Vector2 point, Polygon poly, out CollisionResult result )
		{
			result = new CollisionResult();

			if( poly.containsPoint( point ) )
			{
				float distanceSquared;
				var closestPoint = Polygon.getClosestPointOnPolygonToPoint( poly.points, point - poly.position, out distanceSquared, out result.normal );

				result.minimumTranslationVector = result.normal * Mathf.sqrt( distanceSquared );
				result.point = closestPoint + poly.position;

				return true;
			}

			return false;
		}

	}
}
