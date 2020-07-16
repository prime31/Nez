using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public static partial class ShapeCollisions
	{
		public static bool CircleToCircleCast(Circle first, Circle second, Vector2 deltaMovement, out RaycastHit hit)
		{
			hit = new RaycastHit();

			//http://ericleong.me/research/circle-circle/
			// Find the closest point on the movement vector of the moving circle (first) to the center of the non-moving circle
			var endPointOfCast = first.position + deltaMovement;
			var d = ClosestPointOnLine(first.position, endPointOfCast, second.position);

			// Then find the distance between the closest point and the center of the non-moving circle
			var closestDistanceSquared = Vector2.DistanceSquared(second.position, d);
			var sumOfRadiiSquared = (first.Radius + second.Radius) * (first.Radius + second.Radius);

			// If it is smaller than the sum of the sum of the radii, then a collision has occurred
			if (closestDistanceSquared <= sumOfRadiiSquared)
			{
				var normalizedDeltaMovement = Vector2.Normalize(deltaMovement);

				// edge case: if the end point is equal to the closest point on the line then a line from it to the second.position
				// will not be perpindicular to the ray. We need it to be to use Pythagorus
				if (d == endPointOfCast)
				{
					// extend the end point of the cast radius distance so we get a point that is perpindicular and recalc everything
					endPointOfCast = first.position + deltaMovement + normalizedDeltaMovement * second.Radius;
					d = ClosestPointOnLine(first.position, endPointOfCast, second.position);
					closestDistanceSquared = Vector2.DistanceSquared(second.position, d);
				}

				var backDist = Mathf.Sqrt(sumOfRadiiSquared - closestDistanceSquared);

				hit.Centroid = d - backDist * normalizedDeltaMovement;
				hit.Normal = Vector2.Normalize(hit.Centroid - second.position);
				hit.Fraction = (hit.Centroid.X - first.position.X) / deltaMovement.X;
				Vector2.Distance(ref first.position, ref hit.Centroid, out hit.Distance);
				hit.Point = second.position + hit.Normal * second.Radius;

				return true;
			}

			return false;
		}


		public static bool CircleToCircle(Circle first, Circle second, out CollisionResult result)
		{
			result = new CollisionResult();

			// avoid the square root until we actually need it
			var distanceSquared = Vector2.DistanceSquared(first.position, second.position);
			var sumOfRadii = first.Radius + second.Radius;
			var collided = distanceSquared < sumOfRadii * sumOfRadii;
			if (collided)
			{
				result.Normal = Vector2.Normalize(first.position - second.position);
				var depth = sumOfRadii - Mathf.Sqrt(distanceSquared);
				result.MinimumTranslationVector = -depth * result.Normal;
				result.Point = second.position + result.Normal * second.Radius;

				// this gets the actual point of collision which may or may not be useful so we'll leave it here for now
				//var collisionPointX = ( ( first.position.X * second.radius ) + ( second.position.X * first.radius ) ) / sumOfRadii;
				//var collisionPointY = ( ( first.position.Y * second.radius ) + ( second.position.Y * first.radius ) ) / sumOfRadii;
				//result.point = new Vector2( collisionPointX, collisionPointY );

				return true;
			}

			return false;
		}


		/// <summary>
		/// works for circles whos center is in the box as well as just overlapping with the center out of the box.
		/// </summary>
		/// <returns><c>true</c>, if to box was circled, <c>false</c> otherwise.</returns>
		/// <param name="circle">First.</param>
		/// <param name="box">Second.</param>
		/// <param name="result">Result.</param>
		public static bool CircleToBox(Circle circle, Box box, out CollisionResult result)
		{
			result = new CollisionResult();

			var closestPointOnBounds =
				box.bounds.GetClosestPointOnRectangleBorderToPoint(circle.position, out result.Normal);

			// deal with circles whos center is in the box first since its cheaper to see if we are contained
			if (box.ContainsPoint(circle.position))
			{
				result.Point = closestPointOnBounds;

				// calculate mtv. Find the safe, non-collided position and get the mtv from that.
				var safePlace = closestPointOnBounds + result.Normal * circle.Radius;
				result.MinimumTranslationVector = circle.position - safePlace;

				return true;
			}

			float sqrDistance;
			Vector2.DistanceSquared(ref closestPointOnBounds, ref circle.position, out sqrDistance);

			// see if the point on the box is less than radius from the circle
			if (sqrDistance == 0)
			{
				result.MinimumTranslationVector = result.Normal * circle.Radius;
			}
			else if (sqrDistance <= circle.Radius * circle.Radius)
			{
				result.Normal = circle.position - closestPointOnBounds;
				var depth = result.Normal.Length() - circle.Radius;

				result.Point = closestPointOnBounds;
				Vector2Ext.Normalize(ref result.Normal);
				result.MinimumTranslationVector = depth * result.Normal;

				return true;
			}

			return false;
		}


		public static bool CircleToPolygon(Circle circle, Polygon polygon, out CollisionResult result)
		{
			result = new CollisionResult();

			// circle position in the polygons coordinates
			var poly2Circle = circle.position - polygon.position;

			// first, we need to find the closest distance from the circle to the polygon
			float distanceSquared;
			var closestPoint = Polygon.GetClosestPointOnPolygonToPoint(polygon.Points, poly2Circle, out distanceSquared,
				out result.Normal);

			// make sure the squared distance is less than our radius squared else we are not colliding. Note that if the Circle is fully
			// contained in the Polygon the distance could be larger than the radius. Because of that we also  make sure the circle position
			// is not inside the poly.
			var circleCenterInsidePoly = polygon.ContainsPoint(circle.position);
			if (distanceSquared > circle.Radius * circle.Radius && !circleCenterInsidePoly)
				return false;

			// figure out the mtv. We have to be careful to deal with circles fully contained in the polygon or with their center contained.
			Vector2 mtv;
			if (circleCenterInsidePoly)
			{
				mtv = result.Normal * (Mathf.Sqrt(distanceSquared) - circle.Radius);
			}
			else
			{
				// if we have no distance that means the circle center is on the polygon edge. Move it only by its radius
				if (distanceSquared == 0)
				{
					mtv = result.Normal * circle.Radius;
				}
				else
				{
					var distance = Mathf.Sqrt(distanceSquared);
					mtv = -(poly2Circle - closestPoint) * ((circle.Radius - distance) / distance);
				}
			}

			result.MinimumTranslationVector = mtv;
			result.Point = closestPoint + polygon.position;

			return true;
		}


		// something isnt right here with this one
		static bool CircleToPolygon2(Circle circle, Polygon polygon, out CollisionResult result)
		{
			result = new CollisionResult();

			var closestPointIndex = -1;
			var poly2Circle = circle.position - polygon.position;
			var poly2CircleNormalized = Vector2.Normalize(poly2Circle);
			var max = float.MinValue;

			for (var i = 0; i < polygon.Points.Length; i++)
			{
				var projection = Vector2.Dot(polygon.Points[i], poly2CircleNormalized);
				if (max < projection)
				{
					closestPointIndex = i;
					max = projection;
				}
			}

			var poly2CircleLength = poly2Circle.Length();
			if (poly2CircleLength - max - circle.Radius > 0 && poly2CircleLength > 0)
				return false;

			// we have a collision
			// find the closest point on the polygon. we know the closest index so we only have 2 edges to test
			var prePointIndex = closestPointIndex - 1;
			var postPointIndex = closestPointIndex + 1;

			// handle wrapping the points
			if (prePointIndex < 0)
				prePointIndex = polygon.Points.Length - 1;

			if (postPointIndex == polygon.Points.Length)
				postPointIndex = 0;

			var circleCenter = circle.position - polygon.position;
			var closest1 = ClosestPointOnLine(polygon.Points[prePointIndex], polygon.Points[closestPointIndex],
				circleCenter);
			var closest2 = ClosestPointOnLine(polygon.Points[closestPointIndex], polygon.Points[postPointIndex],
				circleCenter);
			float distance1, distance2;
			Vector2.DistanceSquared(ref circleCenter, ref closest1, out distance1);
			Vector2.DistanceSquared(ref circleCenter, ref closest2, out distance2);

			var radiusSquared = circle.Radius * circle.Radius;

			float seperationDistance;
			if (distance1 < distance2)
			{
				// make sure the squared distance is less than our radius squared else we are not colliding
				if (distance1 > radiusSquared)
					return false;

				seperationDistance = circle.Radius - Mathf.Sqrt(distance1);
				var edge = polygon.Points[closestPointIndex] - polygon.Points[prePointIndex];
				result.Normal = new Vector2(edge.Y, -edge.X);
				result.Point = polygon.position + closest1;
			}
			else
			{
				// make sure the squared distance is less than our radius squared else we are not colliding
				if (distance2 > radiusSquared)
					return false;

				seperationDistance = circle.Radius - Mathf.Sqrt(distance2);
				var edge = polygon.Points[postPointIndex] - polygon.Points[closestPointIndex];
				result.Normal = new Vector2(edge.Y, -edge.X);
				result.Point = polygon.position + closest2;
			}

			result.Normal.Normalize();
			result.MinimumTranslationVector = result.Normal * -seperationDistance;

			return true;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ClosestPointOnLine(Vector2 lineA, Vector2 lineB, Vector2 closestTo)
		{
			var v = lineB - lineA;
			var w = closestTo - lineA;
			var t = Vector2.Dot(w, v) / Vector2.Dot(v, v);
			t = MathHelper.Clamp(t, 0, 1);

			return lineA + v * t;
		}
	}
}