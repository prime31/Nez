using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Collisions
	{
		[Flags]
		public enum PointSectors
		{
			Center = 0,
			Top = 1,
			Bottom = 2,
			TopLeft = 9,
			TopRight = 5,
			Left = 8,
			Right = 4,
			BottomLeft = 10,
			BottomRight = 6
		};


		#region Line

		static public bool lineToLine( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2 )
		{
			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if( bDotDPerp == 0 )
				return false;

			Vector2 c = b1 - a1;
			float t = ( c.X * d.Y - c.Y * d.X ) / bDotDPerp;
			if( t < 0 || t > 1 )
				return false;

			float u = ( c.X * b.Y - c.Y * b.X ) / bDotDPerp;
			if( u < 0 || u > 1 )
				return false;

			return true;
		}


		static public bool lineToLine( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection )
		{
			intersection = Vector2.Zero;

			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if( bDotDPerp == 0 )
				return false;

			Vector2 c = b1 - a1;
			float t = ( c.X * d.Y - c.Y * d.X ) / bDotDPerp;
			if( t < 0 || t > 1 )
				return false;

			float u = ( c.X * b.Y - c.Y * b.X ) / bDotDPerp;
			if( u < 0 || u > 1 )
				return false;

			intersection = a1 + t * b;

			return true;
		}

		#endregion


		#region Circle

		static public Vector2 closestPointOnLine( Vector2 lineA, Vector2 lineB, Vector2 closestTo )
		{
			var v = lineB - lineA;
			var w = closestTo - lineA;
			var t = Vector2.Dot( w, v ) / Vector2.Dot( v, v );
			t = MathHelper.Clamp( t, 0, 1 );

			return lineA + v * t;
		}


		static public bool circleToCircle( Vector2 circleCenter1, float circleRadius1, Vector2 circleCenter2, float circleRadius2 )
		{
			return Vector2.DistanceSquared( circleCenter1, circleCenter2 ) < ( circleRadius1 + circleRadius2 ) * ( circleRadius1 + circleRadius2 );
		}


		static public bool circleToLine( Vector2 circleCenter, float radius, Vector2 lineFrom, Vector2 lineTo )
		{
			return Vector2.DistanceSquared( circleCenter, closestPointOnLine( lineFrom, lineTo, circleCenter ) ) < radius * radius;
		}


		static public bool circleToPoint( Vector2 circleCenter, float radius, Vector2 point )
		{
			return Vector2.DistanceSquared( circleCenter, point ) < radius * radius;
		}

		#endregion


		#region Bounds/Rect

		static public bool rectToCircle( float rX, float rY, float rW, float rH, Vector2 circleCenter, float radius )
		{
			//Check if the circle contains the rectangle's center-point
			if( Collisions.circleToPoint( circleCenter, radius, new Vector2( rX + rW / 2, rY + rH / 2 ) ) )
				return true;

			//Check the circle against the relevant edges
			Vector2 edgeFrom;
			Vector2 edgeTo;
			var sector = getSector( rX, rY, rW, rH, circleCenter );

			if( ( sector & PointSectors.Top ) != 0 )
			{
				edgeFrom = new Vector2( rX, rY );
				edgeTo = new Vector2( rX + rW, rY );
				if( circleToLine( circleCenter, radius, edgeFrom, edgeTo ) )
					return true;
			}

			if( ( sector & PointSectors.Bottom ) != 0 )
			{
				edgeFrom = new Vector2( rX, rY + rH );
				edgeTo = new Vector2( rX + rW, rY + rH );
				if( circleToLine( circleCenter, radius, edgeFrom, edgeTo ) )
					return true;
			}

			if( ( sector & PointSectors.Left ) != 0 )
			{
				edgeFrom = new Vector2( rX, rY );
				edgeTo = new Vector2( rX, rY + rH );
				if( circleToLine( circleCenter, radius, edgeFrom, edgeTo ) )
					return true;
			}

			if( ( sector & PointSectors.Right ) != 0 )
			{
				edgeFrom = new Vector2( rX + rW, rY );
				edgeTo = new Vector2( rX + rW, rY + rH );
				if( circleToLine( circleCenter, radius, edgeFrom, edgeTo ) )
					return true;
			}

			return false;
		}


		static public bool rectToCircle( ref Rectangle rect, Vector2 cPosition, float cRadius )
		{
			return rectToCircle( rect.X, rect.Y, rect.Width, rect.Height, cPosition, cRadius );
		}


		static public bool rectToCircle( Rectangle rect, Vector2 cPosition, float cRadius )
		{
			return rectToCircle( rect.X, rect.Y, rect.Width, rect.Height, cPosition, cRadius );
		}


		static public bool rectToLine( ref Rectangle rect, Vector2 lineFrom, Vector2 lineTo )
		{
			return rectToLine( rect.X, rect.Y, rect.Width, rect.Height, lineFrom, lineTo );
		}


		static public bool rectToLine( Rectangle rect, Vector2 lineFrom, Vector2 lineTo )
		{
			return rectToLine( rect.X, rect.Y, rect.Width, rect.Height, lineFrom, lineTo );
		}


		static public bool rectToLine( float rX, float rY, float rW, float rH, Vector2 lineFrom, Vector2 lineTo )
		{
			var fromSector = Collisions.getSector( rX, rY, rW, rH, lineFrom );
			var toSector = Collisions.getSector( rX, rY, rW, rH, lineTo );

			if( fromSector == PointSectors.Center || toSector == PointSectors.Center )
				return true;
			else if( ( fromSector & toSector ) != 0 )
				return false;
			else
			{
				PointSectors both = fromSector | toSector;

				//Do line checks against the edges
				Vector2 edgeFrom;
				Vector2 edgeTo;

				if( ( both & PointSectors.Top ) != 0 )
				{
					edgeFrom = new Vector2( rX, rY );
					edgeTo = new Vector2( rX + rW, rY );
					if( Collisions.lineToLine( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}

				if( ( both & PointSectors.Bottom ) != 0 )
				{
					edgeFrom = new Vector2( rX, rY + rH );
					edgeTo = new Vector2( rX + rW, rY + rH );
					if( Collisions.lineToLine( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}

				if( ( both & PointSectors.Left ) != 0 )
				{
					edgeFrom = new Vector2( rX, rY );
					edgeTo = new Vector2( rX, rY + rH );
					if( Collisions.lineToLine( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}

				if( ( both & PointSectors.Right ) != 0 )
				{
					edgeFrom = new Vector2( rX + rW, rY );
					edgeTo = new Vector2( rX + rW, rY + rH );
					if( Collisions.lineToLine( edgeFrom, edgeTo, lineFrom, lineTo ) )
						return true;
				}
			}

			return false;
		}


		static public bool rectToPoint( float rX, float rY, float rW, float rH, Vector2 point )
		{
			return point.X >= rX && point.Y >= rY && point.X < rX + rW && point.Y < rY + rH;
		}


		static public bool rectToPoint( Rectangle rect, Vector2 point )
		{
			return rectToPoint( rect.X, rect.Y, rect.Width, rect.Height, point );
		}

		#endregion


		#region Polygon

		// Structure that stores the results of the PolygonCollision function
		public struct PolygonCollisionResult
		{
			/// <summary>
			/// Are the polygons currently intersecting
			/// </summary>
			public bool intersects;

			/// <summary>
			/// The translation to apply to polygon A to push the polygons appart.
			/// </summary>
			public Vector2 minimumTranslationVector;
		}


		// currently, this is just a brute force check of each polygon edge
		public static bool polygonToLine( PolygonCollider polygon, Vector2 lineFrom, Vector2 lineTo )
		{
			Vector2 p1;
			Vector2 p2;

			for( var i = 0; i < polygon.worldSpacePoints.Length; i++ )
			{
				p1 = polygon.worldSpacePoints[i];
				if( i + 1 >= polygon.worldSpacePoints.Length )
					p2 = polygon.worldSpacePoints[0];
				else
					p2 = polygon.worldSpacePoints[i + 1];

				if( lineToLine( p1, p2, lineFrom, lineTo ) )
					return true;
			}

			return false;
		}


		// http://www.dyn4j.org/2010/01/sat/
		// http://www.bitlush.com/posts/circle-vs-polygon-collision-detection-in-c-sharp
		public static bool polygonToCircle( PolygonCollider polygon, Vector2 circleCenter, float circleRadius )
		{
			var radiusSquared = circleRadius * circleRadius;
			var vertex = polygon.worldSpacePoints[polygon.worldSpacePoints.Length - 1];

			var nearestDistance = float.MaxValue;
			var nearestIsInside = false;
			var nearestVertex = -1;
			var lastIsInside = false;

			for( var i = 0; i < polygon.worldSpacePoints.Length; i++ )
			{
				var nextVertex = polygon.worldSpacePoints[i];
				var axis = circleCenter - vertex;
				var distance = axis.LengthSquared() - radiusSquared;

				if( distance <= 0 )
					return true;

				var isInside = false;
				var edge = nextVertex - vertex;
				var edgeLengthSquared = edge.LengthSquared();

				if( edgeLengthSquared != 0 )
				{
					var dot = Vector2.Dot( edge, axis );

					if( dot >= 0 && dot <= edgeLengthSquared )
					{
						var projection = vertex + ( dot / edgeLengthSquared ) * edge;

						axis = projection - circleCenter;

						if( axis.LengthSquared() <= radiusSquared )
						{
							return true;
						}
						else
						{
							if( edge.X > 0 )
							{
								if( axis.Y > 0 )
									return false;
							}
							else if( edge.X < 0 )
							{
								if( axis.Y < 0 )
									return false;
							}
							else if( edge.Y > 0 )
							{
								if( axis.X < 0 )
									return false;
							}
							else
							{
								if( axis.X > 0 )
									return false;
							}

							isInside = true;
						}
					}
				}

				if( distance < nearestDistance )
				{
					nearestDistance = distance;
					nearestIsInside = isInside || lastIsInside;
					nearestVertex = i;
				}

				vertex = nextVertex;
				lastIsInside = isInside;
			}

			if( nearestVertex == 0 )
			{
				return nearestIsInside || lastIsInside;
			}
			else
			{
				return nearestIsInside;
			}
		}


		/// <summary>
		/// Check if polygon A is going to collide with polygon B for the given velocity
		/// </summary>
		/// <returns>The to polygon.</returns>
		/// <param name="polygonA">Polygon a.</param>
		/// <param name="polygonB">Polygon b.</param>
		/// <param name="velocity">Velocity.</param>
		public static PolygonCollisionResult polygonToPolygon( PolygonCollider polygonA, PolygonCollider polygonB )
		{
			var result = new PolygonCollisionResult();
			result.intersects = true;

			var edgeCountA = polygonA.edges.Length;
			var edgeCountB = polygonB.edges.Length;
			var minIntervalDistance = float.PositiveInfinity;
			//var translationAxis = new Vector2();
			Vector2 edge;

			// Loop through all the edges of both polygons
			for( var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++ )
			{
				if( edgeIndex < edgeCountA )
					edge = polygonA.edges[edgeIndex];
				else
					edge = polygonB.edges[edgeIndex - edgeCountA];


				// ===== 1. Find if the polygons are currently intersecting =====

				// Find the axis perpendicular to the current edge
				var axis = new Vector2( -edge.Y, edge.X );
				axis.Normalize();

				// Find the projection of the polygon on the current axis
				var minA = 0f;
				var minB = 0f;
				var maxA = 0f;
				var maxB = 0f;
				projectPolygon( axis, polygonA, ref minA, ref maxA );
				projectPolygon( axis, polygonB, ref minB, ref maxB );

				// Check if the polygon projections are currentlty intersecting
				var intervalDist = intervalDistance( minA, maxA, minB, maxB );
				if( intervalDist > 0 )
					result.intersects = false;


				/* here we could project a Velocity onto the axis and do another check with the velocity added to the projection.
				 * for now this is removed since no other collision methods take velocity into account
				// ===== 2. Now find if the polygons *will* intersect =====

				// Project the velocity on the current axis
				var velocityProjection = Vector2.Dot( axis, velocity );

				// Get the projection of polygon A during the movement
				if( velocityProjection < 0 )
					minA += velocityProjection;
				else
					maxA += velocityProjection;

				// Do the same test as above for the new projection
				var intervalDist = intervalDistance( minA, maxA, minB, maxB );
				if( intervalDist > 0 )
					result.willIntersect = false;
				*/

				// If the polygons are not intersecting and won't intersect, exit the loop
				if( !result.intersects )
					break;

				// Check if the current interval distance is the minimum one. If so store the interval distance and the current distance.
				// This will be used to calculate the minimum translation vector
				intervalDist = Math.Abs( intervalDist );
				if( intervalDist < minIntervalDistance )
				{
					minIntervalDistance = intervalDist;

					// this relies on a center property and is really only useful if we use the velocity calculations above
					//translationAxis = axis;
					//var d = polygonA.center - polygonB.center;
					//if( Vector2.Dot( d, translationAxis ) < 0 )
					//	translationAxis = -translationAxis;
				}
			}

			// The minimum translation vector can be used to push the polygons appart.
			// First moves the polygons by their velocity
			// then move polygonA by MinimumTranslationVector.
			//if( result.willIntersect )
			//	result.minimumTranslationVector = translationAxis * minIntervalDistance;

			return result;
		}


		/// <summary>
		/// Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
		/// </summary>
		/// <param name="axis">Axis.</param>
		/// <param name="polygon">Polygon.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		static void projectPolygon( Vector2 axis, PolygonCollider polygon, ref float min, ref float max )
		{
			// To project a point on an axis use the dot product
			var d = Vector2.Dot( axis, polygon.worldSpacePoints[0] );
			min = d;
			max = d;
			for( var i = 0; i < polygon.worldSpacePoints.Length; i++ )
			{
				d = Vector2.Dot( polygon.worldSpacePoints[i], axis );
				if( d < min )
					min = d;
				else if( d > max )
					max = d;
			}
		}


		/// <summary>
		/// Calculate the distance between [minA, maxA] and [minB, maxB]. The distance will be negative if the intervals overlap
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="minA">Minimum a.</param>
		/// <param name="maxA">Max a.</param>
		/// <param name="minB">Minimum b.</param>
		/// <param name="maxB">Max b.</param>
		static float intervalDistance( float minA, float maxA, float minB, float maxB )
		{
			if( minA < minB )
				return minB - maxA;
			else
				return minA - maxB;
		}


		/// <summary>
		/// returns true if the OBBs overlap
		/// </summary>
		/// <returns><c>true</c>, if box collider to oriented box collider was orienteded, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		public static bool orientedBoxColliderToOrientedBoxCollider( OrientedBoxCollider first, OrientedBoxCollider second )
		{
			// we have to check both directions for OBBs
			return orientedBoxColliderOverlapsOneWay( first, second ) && orientedBoxColliderOverlapsOneWay( second, first );
		}


		/// <summary>
		/// Returns true if other overlaps any dimension of this
		/// </summary>
		/// <returns><c>true</c>, if way was overlaps1ed, <c>false</c> otherwise.</returns>
		/// <param name="other">Other.</param>
		static bool orientedBoxColliderOverlapsOneWay( OrientedBoxCollider first, OrientedBoxCollider second )
		{
			// TODO: something is a bit off here. we get tiny penetrations which causes stuck collisions
			for( var i = 0; i < 2; i++ )
			{
				var t = Vector2.Dot( second.worldSpacePoints[0], first.edges[i] );

				// find the extent of box 2 on axis a
				var tMin = t;
				var tMax = t;

				for( var j = 1; j < 4; j++ )
				{
					t = Vector2.Dot( second.worldSpacePoints[j], first.edges[i] );
					if( t < tMin )
						tMin = t;
					else if( t > tMax )
						tMax = t;
				}

				// we have to subtract off the origin
				// See if [tMin, tMax] intersects [0, 1]
				if( tMin > 1f + first._corner0ProjectionsOnEdges[i] || tMax < first._corner0ProjectionsOnEdges[i] )
				{
					// there was no intersection along this dimension so the boxes cannot possibly overlap.
					return false;
				}
			}

			// there was no dimension along which there is no intersection. Therefore the boxes overlap.
			return true;
		}

		#endregion


		#region Sectors

		/*
         *  Bitflags and helpers for using the Cohen–Sutherland algorithm
         *  http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
         *  
         *  Sector bitflags:
         *      1001  1000  1010
         *      0001  0000  0010
         *      0101  0100  0110
         */

		static public PointSectors getSector( Rectangle rect, Vector2 point )
		{
			PointSectors sector = PointSectors.Center;

			if( point.X < rect.Left )
				sector |= PointSectors.Left;
			else if( point.X >= rect.Right )
				sector |= PointSectors.Right;

			if( point.Y < rect.Top )
				sector |= PointSectors.Top;
			else if( point.Y >= rect.Bottom )
				sector |= PointSectors.Bottom;

			return sector;
		}


		static public PointSectors getSector( float rX, float rY, float rW, float rH, Vector2 point )
		{
			PointSectors sector = PointSectors.Center;

			if( point.X < rX )
				sector |= PointSectors.Left;
			else if( point.X >= rX + rW )
				sector |= PointSectors.Right;

			if( point.Y < rY )
				sector |= PointSectors.Top;
			else if( point.Y >= rY + rH )
				sector |= PointSectors.Bottom;

			return sector;
		}

		#endregion
	
	}
}

