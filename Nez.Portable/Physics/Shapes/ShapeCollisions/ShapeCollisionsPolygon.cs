using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public static partial class ShapeCollisions
	{
		/// <summary>
		/// checks for a collision between two Polygons
		/// </summary>
		/// <returns>The collision.</returns>
		/// <param name="first">Polygon a.</param>
		/// <param name="second">Polygon b.</param>
		public static bool PolygonToPolygon(Polygon first, Polygon second, out CollisionResult result)
		{
			result = new CollisionResult();
			var isIntersecting = true;

			var firstEdges = first.EdgeNormals;
			var secondEdges = second.EdgeNormals;
			var minIntervalDistance = float.PositiveInfinity;
			var translationAxis = new Vector2();
			var polygonOffset = first.position - second.position;
			Vector2 axis;

			// Loop through all the edges of both polygons
			for (var edgeIndex = 0; edgeIndex < firstEdges.Length + secondEdges.Length; edgeIndex++)
			{
				// 1. Find if the polygons are currently intersecting
				// Polygons have the normalized axis perpendicular to the current edge cached for us
				if (edgeIndex < firstEdges.Length)
					axis = firstEdges[edgeIndex];
				else
					axis = secondEdges[edgeIndex - firstEdges.Length];

				// Find the projection of the polygon on the current axis
				float minA = 0;
				float minB = 0;
				float maxA = 0;
				float maxB = 0;
				var intervalDist = 0f;
				GetInterval(axis, first, ref minA, ref maxA);
				GetInterval(axis, second, ref minB, ref maxB);

				// get our interval to be space of the second Polygon. Offset by the difference in position projected on the axis.
				float relativeIntervalOffset;
				Vector2.Dot(ref polygonOffset, ref axis, out relativeIntervalOffset);
				minA += relativeIntervalOffset;
				maxA += relativeIntervalOffset;

				// check if the polygon projections are currentlty intersecting
				intervalDist = IntervalDistance(minA, maxA, minB, maxB);
				if (intervalDist > 0)
					isIntersecting = false;


				// for Poly-to-Poly casts add a Vector2? parameter called deltaMovement. In the interest of speed we do not use it here
				// 2. Now find if the polygons *will* intersect. only bother checking if we have some velocity
				//if( deltaMovement.HasValue )
				//{
				//	// Project the velocity on the current axis
				//	var velocityProjection = Vector2.Dot( axis, deltaMovement.Value );

				//	// Get the projection of polygon A during the movement
				//	if( velocityProjection < 0 )
				//		minA += velocityProjection;
				//	else
				//		maxA += velocityProjection;

				//	// Do the same test as above for the new projection
				//	intervalDist = intervalDistance( minA, maxA, minB, maxB );
				//	if( intervalDist > 0 )
				//		willIntersect = false;
				//}


				// If the polygons are not intersecting and won't intersect, exit the loop
				if (!isIntersecting)
					return false;

				// Check if the current interval distance is the minimum one. If so store the interval distance and the current distance.
				// This will be used to calculate the minimum translation vector
				intervalDist = Math.Abs(intervalDist);
				if (intervalDist < minIntervalDistance)
				{
					minIntervalDistance = intervalDist;
					translationAxis = axis;

					if (Vector2.Dot(translationAxis, polygonOffset) < 0)
						translationAxis = -translationAxis;
				}
			}

			// The minimum translation vector can be used to push the polygons appart.
			result.Normal = translationAxis;
			result.MinimumTranslationVector = -translationAxis * minIntervalDistance;

			return true;
		}


		/// <summary>
		/// Calculates the distance between [minA, maxA] and [minB, maxB]. The distance will be negative if the intervals overlap
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="minA">Minimum a.</param>
		/// <param name="maxA">Max a.</param>
		/// <param name="minB">Minimum b.</param>
		/// <param name="maxB">Max b.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float IntervalDistance(float minA, float maxA, float minB, float maxB)
		{
			if (minA < minB)
				return minB - maxA;

			return minA - maxB;
		}


		/// <summary>
		/// Calculates the projection of a polygon on an axis and returns it as a [min, max] interval
		/// </summary>
		/// <param name="axis">Axis.</param>
		/// <param name="polygon">Polygon.</param>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void GetInterval(Vector2 axis, Polygon polygon, ref float min, ref float max)
		{
			// To project a point on an axis use the dot product
			float dot;
			Vector2.Dot(ref polygon.Points[0], ref axis, out dot);
			min = max = dot;

			for (var i = 1; i < polygon.Points.Length; i++)
			{
				Vector2.Dot(ref polygon.Points[i], ref axis, out dot);
				if (dot < min)
					min = dot;
				else if (dot > max)
					max = dot;
			}
		}
	}
}