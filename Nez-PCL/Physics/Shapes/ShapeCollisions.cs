using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.PhysicsShapes
{
	public static class ShapeCollisions
	{
		// storage for polygon SAT checks
		static Vector2[] _satAxisArray = new Vector2[64];
		// a maximum of 32 vertices per poly is supported
		static float[] _satTimerPerAxis = new float[64];


		/// <summary>
		/// swept collision check
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="deltaMovement">Delta movement.</param>
		/// <param name="hit">Hit.</param>
		public static bool collide( Shape first, Shape second, Vector2 deltaMovement, out RaycastHit hit )
		{
			hit = new RaycastHit();
			throw new NotImplementedException( "this should probably be in each Shape class and it still needs to be implemented ;)" );
		}


		#region Box to Box

		/// <summary>
		/// checks the result of a box being moved by deltaMovement with second
		/// </summary>
		/// <returns><c>true</c>, if to box cast was boxed, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="deltaMovement">Delta movement.</param>
		/// <param name="hit">Hit.</param>
		public static bool boxToBoxCast( Box first, Box second, Vector2 movement, out RaycastHit hit )
		{
			// http://hamaluik.com/posts/swept-aabb-collision-using-minkowski-difference/
			hit = new RaycastHit();

			// first we check for an overlap. if we have an overlap we dont do the sweep test
			var minkowskiDiff = minkowskiDifference( first, second );
			if( minkowskiDiff.contains( 0f, 0f ) )
			{
				// calculate the MTV. if it is zero then we can just call this a non-collision
				var mtv = minkowskiDiff.getClosestPointOnBoundsToOrigin();
				if( mtv == Vector2.Zero )
					return false;
						
				hit.normal = -mtv;
				hit.normal.Normalize();
				hit.distance = 0f;
				hit.fraction = 0f;

				return true;
			}
			else
			{
				// ray-cast the movement vector against the Minkowski AABB
				var ray = new Ray2D( Vector2.Zero, -movement );
				float fraction;
				if( minkowskiDiff.rayIntersects( ref ray, out fraction ) && fraction <= 1.0f )
				{
					hit.fraction = fraction;
					hit.distance = movement.Length() * fraction;
					hit.normal = -movement;
					hit.normal.Normalize();
					hit.centroid = first.bounds.center + movement * fraction;

					return true;
				}
			}

			return false;
		}


		public static bool boxToBox( Box first, Box second, out CollisionResult result )
		{
			result = new CollisionResult();

			var minkowskiDiff = minkowskiDifference( first, second );
			if( minkowskiDiff.contains( 0f, 0f ) )
			{
				// calculate the MTV. if it is zero then we can just call this a non-collision
				result.minimumTranslationVector = minkowskiDiff.getClosestPointOnBoundsToOrigin();

				if( result.minimumTranslationVector == Vector2.Zero )
					return false;

				result.normal = -result.minimumTranslationVector;
				result.normal.Normalize();

				return true;
			}

			return false;
		}


		static RectangleF minkowskiDifference( Box first, Box second )
		{
			var topLeft = first.position - second.bounds.max;
			var fullSize = first.bounds.size + second.bounds.size;

			return new RectangleF( topLeft.X, topLeft.Y, fullSize.X, fullSize.Y );
		}

		#endregion

		
		#region Polygon to Polygon

		/// <summary>
		/// casts first at second
		/// </summary>
		/// <returns><c>true</c>, if to polygon was polygoned, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="deltaMovement">Delta movement.</param>
		public static bool polygonToPolygonCast( Polygon first, Polygon second, Vector2 deltaMovement, out RaycastHit hit )
		{
			hit = new RaycastHit();
			float timeOfCollision;

			if( polygonToPolygon( first, second, deltaMovement, out hit.normal, out timeOfCollision ) )
			{
				hit.fraction = timeOfCollision;

				// if timeOfCollision is less than 0 this is an overlap
				if( timeOfCollision < 0f )
				{
					hit.centroid = first.position - hit.normal * timeOfCollision;
				}
				else
				{
					hit.centroid = first.position + deltaMovement * timeOfCollision;
				}

				return true;
			}

			return false;
		}


		/// <summary>
		/// does an overlap check of first vs second. ShapeCollisionResult retuns the data for moving first so it isn't colliding with second.
		/// </summary>
		/// <returns><c>true</c>, if to polygon was polygoned, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="result">Result.</param>
		public static bool polygonToPolygon( Polygon first, Polygon second, out CollisionResult result )
		{
			result = new CollisionResult();
			float timeOfCollision;

			if( polygonToPolygon( first, second, null, out result.normal, out timeOfCollision ) )
			{
				result.minimumTranslationVector = result.normal * ( timeOfCollision );
				return true;
			}

			return false;
		}


		/// <summary>
		/// checks for a collision between first and second. deltaMovement is applied to first to see if a collision occurs in the future.
		/// - a negative timeOfCollision means we have an overlap
		/// - a positive timeOfCollision means we have a future collision
		/// based on http://elancev.name/oliver/2D%20polygon.htm
		/// </summary>
		/// <returns><c>true</c>, if to polygon was polygoned, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="deltaMovement">Delta movement.</param>
		/// <param name="responseNormal">Response normal.</param>
		/// <param name="timeOfCollision">Time of collision.</param>
		static bool polygonToPolygon( Polygon first, Polygon second, Vector2? deltaMovement, out Vector2 responseNormal, out float timeOfCollision )
		{
			timeOfCollision = float.MinValue;
			responseNormal = Vector2.Zero;
			// polygon verts are in local space so we need to convert one of the polys to be in the space of the other. We use the distance
			// between them to do so.
			var polygonOffset = first.position - second.position;

			// All the separation axes
			var iNumAxes = 0;


			if( deltaMovement.HasValue )
			{
				_satAxisArray[iNumAxes] = new Vector2( -deltaMovement.Value.Y, deltaMovement.Value.X );
				var fVel2 = Vector2.Dot( deltaMovement.Value, deltaMovement.Value );
				if( fVel2 > Mathf.epsilon )
				{
					if( !intervalIntersect(	first, second, ref _satAxisArray[iNumAxes], ref polygonOffset, ref deltaMovement, out _satTimerPerAxis[iNumAxes] ) )
						return false;
					iNumAxes++;
				}
			}

			// test separation axes of A
			for( int j = first.points.Length - 1, i = 0; i < first.points.Length; j = i, i++ )
			{
				// we only need to check 2 axis for boxes
				if( second.isBox && i == 2 )
					break;
				
				var point0 = first.points[j];
				var point1 = first.points[i];
				var edge = point1 - point0;
				_satAxisArray[iNumAxes] = new Vector2( -edge.Y, edge.X );

				if( !intervalIntersect(	first, second, ref _satAxisArray[iNumAxes], ref polygonOffset, ref deltaMovement, out _satTimerPerAxis[iNumAxes] ) )
					return false;
				iNumAxes++;
			}

			// test separation axes of B
			for( int j = second.points.Length - 1, i = 0; i < second.points.Length; j = i, i++ )
			{
				// we only need to check 2 axis for boxes
				if( second.isBox && i == 2 )
					break;

				var point0 = second.points[j];
				var point1 = second.points[i];
				var edge = point1 - point0;
				_satAxisArray[iNumAxes] = new Vector2( -edge.Y, edge.X );

				if( !intervalIntersect(	first, second, ref _satAxisArray[iNumAxes], ref polygonOffset, ref deltaMovement, out _satTimerPerAxis[iNumAxes] ) )
					return false;
				iNumAxes++;
			}

			if( !findMinimumTranslationDistance( iNumAxes, out responseNormal, out timeOfCollision ) )
				return false;

			// make sure the polygons gets pushed away from each other.
			if( Vector2.Dot( responseNormal, polygonOffset ) < 0f )
				responseNormal = -responseNormal;

			return true;
		}


		static bool intervalIntersect( Polygon first, Polygon second, ref Vector2 axis, ref Vector2 shapeOffset, ref Vector2? deltaMovement, out float taxis )
		{
			taxis = float.MinValue;
			float min0, max0;
			float min1, max1;
			getInterval( first, first.points.Length, axis, out min0, out max0 );
			getInterval( second, second.points.Length, axis, out min1, out max1 );

			var h = Vector2.Dot( shapeOffset, axis );
			min0 += h;
			max0 += h;

			var d0 = min0 - max1; // if overlapped, d0 < 0
			var d1 = min1 - max0; // if overlapped, d1 < 0

			// separated, test dynamic intervals
			if( d0 > 0.0f || d1 > 0.0f )
			{
				// if we have no velocity we are done
				if( !deltaMovement.HasValue )
					return false;
				
				var v = Vector2.Dot( deltaMovement.Value, axis );

				// small velocity, so only the overlap test will be relevant. 
				if( Math.Abs( v ) < 0.0000001f )
					return false;

				var t0 = -d0 / v; // time of impact to d0 reaches 0
				var t1 = d1 / v; // time of impact to d0 reaches 1

				if( t0 > t1 )
				{
					var temp = t0;
					t0 = t1;
					t1 = temp;
				}

				taxis = t0 > 0.0f ? t0 : t1;
				if( taxis < 0.0f || taxis > 1.0f )
					return false;

				return true;
			}
			else
			{
				// overlap. get the interval, as a the smallest of |d0| and |d1|
				// return negative number to mark it as an overlap
				taxis = d0 > d1 ? d0 : d1;
				return true;
			}
		}


		/// <summary>
		/// calculates the projection range of a polygon along an axis
		/// </summary>
		/// <param name="A">A.</param>
		/// <param name="iNumVertices">I number vertices.</param>
		/// <param name="xAxis">X axis.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		static void getInterval( Polygon polygon, int numVertices, Vector2 axis, out float min, out float max )
		{
			min = max = Vector2.Dot( polygon.points[0], axis );

			for( var i = 1; i < numVertices; i++ )
			{
				var d = Vector2.Dot( polygon.points[i], axis );
				if( d < min )
					min = d;
				else if( d > max )
					max = d;
			}
		}


		static bool findMinimumTranslationDistance( int iNumAxes, out Vector2 normal, out float timeOfIntersection )
		{
			// find collision first
			var mini = -1;
			timeOfIntersection = 0f;
			normal = new Vector2( 0, 0 );

			for( var i = 0; i < iNumAxes; i++ )
			{
				if( _satTimerPerAxis[i] > 0 && _satTimerPerAxis[i] > timeOfIntersection )
				{
					mini = i;
					timeOfIntersection = _satTimerPerAxis[i];
					normal = _satAxisArray[i];
					normal.Normalize();
				}
			}

			// found one
			if( mini != -1 )
				return true; 

			// nope, find overlaps
			mini = -1;
			for( var i = 0; i < iNumAxes; i++ )
			{
				var n = _satAxisArray[i].Length();
				_satAxisArray[i].Normalize();
				_satTimerPerAxis[i] /= n;

				if( _satTimerPerAxis[i] > timeOfIntersection || mini == -1 )
				{
					mini = i;
					timeOfIntersection = _satTimerPerAxis[i];
					normal = _satAxisArray[i];
				}
			}

			if( mini == -1 )
				Debug.error( "Error" );

			return ( mini != -1 );
		}

		#endregion


		#region Circle

		public static bool circleToCircleCast( Circle first, Circle second, Vector2 deltaMovement, out RaycastHit hit )
		{
			hit = new RaycastHit();

			//http://ericleong.me/research/circle-circle/
			// Find the closest point on the movement vector of the moving circle (first) to the center of the non-moving circle
			var endPointOfCast = first.position + deltaMovement;
			var d = closestPointOnLine( first.position, endPointOfCast, second.position );
			// Then find the distance between the closest point and the center of the non-moving circle
			var closestDistanceSquared = Vector2.DistanceSquared( second.position, d );
			var sumOfRadiiSquared = ( first.radius + second.radius ) * ( first.radius + second.radius );

			// If it is smaller than the sum of the sum of the radii, then a collision has occurred
			if( closestDistanceSquared <= sumOfRadiiSquared )
			{
				var normalizedDeltaMovement = Vector2.Normalize( deltaMovement );
				// edge case: if the end point is equal to the closest point on the line then a line from it to the second.position
				// will not be perpindicular to the ray. We need it to be to use Pythagorus
				if( d == endPointOfCast )
				{
					// extend the end point of the cast radius distance so we get a point that is perpindicular and recalc everything
					endPointOfCast = first.position + deltaMovement + normalizedDeltaMovement * second.radius;
					d = closestPointOnLine( first.position, endPointOfCast, second.position );
					closestDistanceSquared = Vector2.DistanceSquared( second.position, d );
				}

				var backDist = Mathf.sqrt( sumOfRadiiSquared - closestDistanceSquared );

				hit.centroid = d - backDist * normalizedDeltaMovement;
				hit.normal = Vector2.Normalize( hit.centroid - second.position );
				hit.fraction = ( hit.centroid.X - first.position.X ) / deltaMovement.X;
				Vector2.Distance( ref first.position, ref hit.centroid, out hit.distance );
				hit.point = second.position + hit.normal * second.radius;

				return true;
			}

			return false;
		}


		public static bool circleToCircle( Circle first, Circle second, out CollisionResult result )
		{
			result = new CollisionResult();

			// avoid the square root until we actually need it
			var distanceSquared = Vector2.DistanceSquared( first.position, second.position );
			var sumOfRadii = first.radius + second.radius;
			var collided = distanceSquared < sumOfRadii * sumOfRadii;
			if( collided )
			{
				result.normal = Vector2.Normalize( first.position - second.position );
				var depth = sumOfRadii - Mathf.sqrt( distanceSquared );
				result.minimumTranslationVector = -depth * result.normal;
				result.point = second.position + result.normal * second.radius;

				// this gets the actual point of collision which may or may not be useful so we'll leave it here for now
				//var collisionPointX = ( ( first.position.X * second.radius ) + ( second.position.X * first.radius ) ) / sumOfRadii;
				//var collisionPointY = ( ( first.position.Y * second.radius ) + ( second.position.Y * first.radius ) ) / sumOfRadii;
				//result.point = new Vector2( collisionPointX, collisionPointY );

				return true;
			}

			return false;
		}


		/// <summary>
		/// note: if circle center lies in the box the collision result will be incorrect!
		/// </summary>
		/// <returns><c>true</c>, if to box was circled, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="result">Result.</param>
		public static bool circleToBox( Circle first, Box second, out CollisionResult result )
		{
			result = new CollisionResult();

			var closestPointOnBounds = second.bounds.getClosestPointOnRectangleBorderToPoint( first.position );

			float sqrDistance;
			Vector2.DistanceSquared( ref closestPointOnBounds, ref first.position, out sqrDistance );

			// see if the point on the box is less than radius from the circle
			if( sqrDistance <= first.radius * first.radius )
			{
				var distanceToCircle = first.position - closestPointOnBounds;
				var depth = distanceToCircle.Length() - first.radius;

				result.point = closestPointOnBounds;
				result.normal = Vector2.Normalize( distanceToCircle );
				result.minimumTranslationVector = depth * result.normal;

				return true;
			}

			return false;
		}


		public static bool circleToPolygon( Circle circle, Polygon polygon, out CollisionResult result )
		{
			result = new CollisionResult();

			// circle position in the polygons coordinates
			var poly2Circle = circle.position - polygon.position;

			// first, we need to find the closest distance from the circle to the polygon
			float distanceSquared;
			var closestPoint = polygon.getClosestPointOnPolygonToPoint( poly2Circle, out distanceSquared, out result.normal );

			// make sure the squared distance is less than our radius squared else we are not colliding
			if( distanceSquared > circle.radius * circle.radius )
				return false;

			// figure out the mtd
			var distance = Mathf.sqrt( distanceSquared );
			var mtv = ( poly2Circle - closestPoint ) * ( ( circle.radius - distance ) / distance );

			result.minimumTranslationVector = -mtv;
			result.normal.Normalize();

			return true;
		}


		// something isnt right here with this one
		static bool circleToPolygon2( Circle circle, Polygon polygon, out CollisionResult result )
		{
			result = new CollisionResult();

			var closestPointIndex = -1;
			var poly2Circle = circle.position - polygon.position;
			var poly2CircleNormalized = Vector2.Normalize( poly2Circle );
			var max = float.MinValue;

			for( var i = 0; i < polygon.points.Length; i++ )
			{
				var projection = Vector2.Dot( polygon.points[i], poly2CircleNormalized );
				if( max < projection )
				{
					closestPointIndex = i;
					max = projection;
				}
			} 

			var poly2CircleLength = poly2Circle.Length();
			if( poly2CircleLength - max - circle.radius > 0 && poly2CircleLength > 0 )
				return false;

			// we have a collision
			// find the closest point on the polygon. we know the closest index so we only have 2 edges to test
			var prePointIndex = closestPointIndex - 1;
			var postPointIndex = closestPointIndex + 1;

			// handle wrapping the points
			if( prePointIndex < 0 )
				prePointIndex = polygon.points.Length - 1;

			if( postPointIndex == polygon.points.Length )
				postPointIndex = 0;

			var circleCenter = circle.position - polygon.position;
			var closest1 = closestPointOnLine( polygon.points[prePointIndex], polygon.points[closestPointIndex], circleCenter );
			var closest2 = closestPointOnLine( polygon.points[closestPointIndex], polygon.points[postPointIndex], circleCenter );
			float distance1, distance2;
			Vector2.DistanceSquared( ref circleCenter, ref closest1, out distance1 );
			Vector2.DistanceSquared( ref circleCenter, ref closest2, out distance2 );

			var radiusSquared = circle.radius * circle.radius;

			float seperationDistance;
			if( distance1 < distance2 )
			{
				// make sure the squared distance is less than our radius squared else we are not colliding
				if( distance1 > radiusSquared )
					return false;

				seperationDistance = circle.radius - Mathf.sqrt( distance1 );
				var edge = polygon.points[closestPointIndex] - polygon.points[prePointIndex];
				result.normal = new Vector2( edge.Y, -edge.X );
				result.point = polygon.position + closest1;
			}
			else
			{
				// make sure the squared distance is less than our radius squared else we are not colliding
				if( distance2 > radiusSquared )
					return false;

				seperationDistance = circle.radius - Mathf.sqrt( distance2 );
				var edge = polygon.points[postPointIndex] - polygon.points[closestPointIndex];
				result.normal = new Vector2( edge.Y, -edge.X );
				result.point = polygon.position + closest2;
			}

			result.normal.Normalize();
			result.minimumTranslationVector = result.normal * -seperationDistance;

			return true;
		}

		#endregion


		#region Lines

		public static bool lineToPoly( Vector2 start, Vector2 end, Polygon polygon, out RaycastHit hit )
		{
			hit = new RaycastHit();
			var normal = Vector2.Zero;
			var intersectionPoint = Vector2.Zero;
			var fraction = float.MaxValue;
			var hasIntersection = false;

			for( int j = polygon.points.Length - 1, i = 0; i < polygon.points.Length; j = i, i++ )
			{
				var edge1 = polygon.position + polygon.points[j];
				var edge2 = polygon.position + polygon.points[i];
				Vector2 intersection;
				if( lineToLine( edge1, edge2, start, end, out intersection ) )
				{
					hasIntersection = true;
					// TODO: is this the correct way to get the fraction?
					var distanceFraction = ( intersection.X - start.X ) / ( end.X - start.X );
					if( distanceFraction < fraction )
					{
						var edge = edge2 - edge1;
						normal = new Vector2( edge.Y, -edge.X );
						fraction = distanceFraction;
						intersectionPoint = intersection;
					}
				}
			}

			if( hasIntersection )
			{
				normal.Normalize();
				float distance;
				Vector2.Distance( ref start, ref intersectionPoint, out distance );
				hit.setValues( fraction, distance, intersectionPoint, normal );
				return true;
			}

			return false;
		}


		public static bool lineToCircle( Vector2 start, Vector2 end, Circle s, out RaycastHit hit )
		{
			hit = new RaycastHit();

			// calculate the length here and normalize d separately since we will need it to get the fraction if we have a hit
			var lineLength = Vector2.Distance( start, end );
			var d = ( end - start ) / lineLength;
			var m = start - s.position;
			var b = Vector2.Dot( m, d );
			var c = Vector2.Dot( m, m ) - s.radius * s.radius;

			// exit if r's origin outside of s (c > 0) and r pointing away from s (b > 0)
			if( c > 0f && b > 0f )
				return false;

			var discr = b * b - c;

			// a negative descriminant means the line misses the circle
			if( discr < 0 )
				return false;

			// ray intersects circle. calculate details now.
			hit.fraction = -b - Mathf.sqrt( discr );

			// if fraction is negative, ray started inside circle so clamp fraction to 0
			if( hit.fraction < 0 )
				hit.fraction = 0;

			hit.point = start + hit.fraction * d;
			Vector2.Distance( ref start, ref hit.point, out hit.distance );
			hit.normal = Vector2.Normalize( hit.point - s.position );
			hit.fraction = hit.distance / lineLength;

			return true;
		}


		static public bool lineToLine( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection )
		{
			intersection = Vector2.Zero;

			var b = a2 - a1;
			var d = b2 - b1;
			var bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if( bDotDPerp == 0 )
				return false;

			var c = b1 - a1;
			var t = ( c.X * d.Y - c.Y * d.X ) / bDotDPerp;
			if( t < 0 || t > 1 )
				return false;

			var u = ( c.X * b.Y - c.Y * b.X ) / bDotDPerp;
			if( u < 0 || u > 1 )
				return false;

			intersection = a1 + t * b;

			return true;
		}

		#endregion


		public static Vector2 closestPointOnLine( Vector2 lineA, Vector2 lineB, Vector2 closestTo )
		{
			var v = lineB - lineA;
			var w = closestTo - lineA;
			var t = Vector2.Dot( w, v ) / Vector2.Dot( v, v );
			t = MathHelper.Clamp( t, 0, 1 );

			return lineA + v * t;
		}

	}
}

