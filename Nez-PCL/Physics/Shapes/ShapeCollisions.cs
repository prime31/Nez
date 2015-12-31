using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.PhysicsShapes
{
	public static class ShapeCollisions
	{
		// storage for polygon SAT checks
		static Vector2[] _satAxisArray = new Vector2[64]; // a maximum of 32 vertices per poly is supported
		static float[] _satTimerPerAxis = new float[64];


		public static bool collide( Shape first, Shape second, Vector2 deltaMovement )
		{
			return false;
		}

		
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
					hit.centroid = first.position - hit.normal * ( timeOfCollision * 1.01f );
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
		public static bool polygonToPolygon( Polygon first, Polygon second, out ShapeCollisionResult result )
		{
			result = new ShapeCollisionResult();
			float timeOfCollision;

			if( polygonToPolygon( first, second, Vector2.Zero, out result.normal, out timeOfCollision ) )
			{
				result.minimumTranslationVector = result.normal * ( timeOfCollision * 1.0f );
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
		static bool polygonToPolygon( Polygon first, Polygon second, Vector2 deltaMovement, out Vector2 responseNormal, out float timeOfCollision )
		{
			timeOfCollision = float.MinValue;
			responseNormal = Vector2.Zero;
			// polygon verts are in local space so we need to convert one of the polys to be in the space of the other. We use the distance
			// between them to do so.
			var polygonOffset = first.position - second.position;

			// All the separation axes
			var iNumAxes = 0;

			_satAxisArray[iNumAxes] = new Vector2( -deltaMovement.Y, deltaMovement.X );
			var fVel2 = Vector2.Dot( deltaMovement, deltaMovement );
			if( fVel2 > 0.00001f )
			{
				if( !intervalIntersect(	first, second, _satAxisArray[iNumAxes], polygonOffset, deltaMovement, out _satTimerPerAxis[iNumAxes] ) )
					return false;
				iNumAxes++;
			}

			// test separation axes of A
			for( int j = first.points.Length - 1, i = 0; i < first.points.Length; j = i, i++ )
			{
				var E0 = first.points[j];
				var E1 = first.points[i];
				var edge = E1 - E0;
				_satAxisArray[iNumAxes] = new Vector2( -edge.Y, edge.X );

				if( !intervalIntersect(	first, second, _satAxisArray[iNumAxes], polygonOffset, deltaMovement, out _satTimerPerAxis[iNumAxes] ) )
					return false;
				iNumAxes++;
			}

			// test separation axes of B
			for( int j = second.points.Length - 1, i = 0; i < second.points.Length; j = i, i++ )
			{
				var E0 = second.points[j];
				var E1 = second.points[i];
				var edge = E1 - E0;
				_satAxisArray[iNumAxes] = new Vector2( -edge.Y, edge.X );

				if( !intervalIntersect(	first, second, _satAxisArray[iNumAxes], polygonOffset, deltaMovement, out _satTimerPerAxis[iNumAxes] ) )
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


		static bool intervalIntersect( Polygon first, Polygon second, Vector2 axis, Vector2 shapeOffset, Vector2 deltaMovement, out float taxis )
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
				var v = Vector2.Dot( deltaMovement, axis );

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


		public static bool circleToCircle( Circle first, Circle second, out ShapeCollisionResult result )
		{
			result = new ShapeCollisionResult();

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


		public static bool circleToPolygon( Circle circle, Polygon polygon, out ShapeCollisionResult result )
		{
			result = new ShapeCollisionResult();

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

			float seperationDistance;
			if( distance1 < distance2 )
			{
				seperationDistance = circle.radius - Mathf.sqrt( distance1 );
				var edge = polygon.points[closestPointIndex] - polygon.points[prePointIndex];
				result.normal = new Vector2( edge.Y, -edge.X );
				result.point = polygon.position + closest1;
			}
			else
			{
				seperationDistance = circle.radius - Mathf.sqrt( distance2 );
				var edge = polygon.points[postPointIndex] - polygon.points[closestPointIndex];
				result.normal = new Vector2( edge.Y, -edge.X );
				result.point = polygon.position + closest2;
			}

			result.normal.Normalize();
			Debug.drawLine( result.point, result.point + result.normal * 40, Color.Blue, 1f );

			result.minimumTranslationVector = result.normal * -seperationDistance;
			Debug.log( "Circle hits poly: distance1: {0}, fix: {1}", distance1, result.minimumTranslationVector );

			return true;
		}

		#endregion


		#region Lines

		public static bool lineToPoly( Vector2 start, Vector2 end, Polygon polygon, out RaycastHit hit )
		{
			hit = new RaycastHit();
			Vector2 normal = Vector2.Zero;
			Vector2 intersectionPoint = Vector2.Zero;
			float fraction = float.MaxValue;
			var hasIntersection = false;
			for( int j = polygon.points.Length - 1, i = 0; i < polygon.points.Length; j = i, i++ )
			{
				var edge1 = polygon.position + polygon.points[j];
				var edge2 = polygon.position + polygon.points[i];
				Vector2 intersection;
				if( lineToLine( edge1, edge2, start, end, out intersection ) )
				{
					hasIntersection = true;
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


		public static bool lineToCircle( Vector2 start, Vector2 end, Circle circle, out RaycastHit hit )
		{
			hit = new RaycastHit();
			float a, b, c, det;

			var lineDir = end - start;
			var diff = start - circle.position;

			a = lineDir.X * lineDir.X + lineDir.Y * lineDir.Y;
			b = 2f * ( lineDir.X * diff.X + lineDir.Y * diff.Y );
			c = diff.X * diff.X + diff.Y * diff.Y - circle.radius * circle.radius;

			det = b * b - 4 * a * c;
			if( ( a <= 0.0000001 ) || ( det < 0 ) )
			{
				// No solutions
				return false;
			}
			else if( det == 0 )
			{
				// one solution.
				var t = -b / ( 2 * a );
				var intersection = new Vector2( start.X + t * lineDir.X, start.Y + t * lineDir.Y );
				var normal = Vector2.Normalize( intersection - circle.position );
				var fraction = ( intersection.X - start.X ) / ( end.X - start.X );
				float distance;
				Vector2.Distance( ref start, ref intersection, out distance );

				hit.setValues( fraction, distance, intersection, normal );
				return true;
			}
			else
			{
				// possibly two solutions but we only care about the closest one. to get the other one just add the Sqrt instead of subtract it
				var t = (float)( ( -b - Math.Sqrt( det ) ) / ( 2 * a ) );
				if( t <= 1f )
				{
					var intersection = new Vector2( start.X + t * lineDir.X, start.Y + t * lineDir.Y );
					var normal = Vector2.Normalize( intersection - circle.position );
					var fraction = ( intersection.X - start.X ) / ( end.X - start.X );
					float distance;
					Vector2.Distance( ref start, ref intersection, out distance );

					hit.setValues( fraction, distance, intersection, normal );
					return true;
				}
			}

			return false;
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

