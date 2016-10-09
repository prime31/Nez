using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	/// <summary>
	/// various collision routines for Shapes. Most of these expect the first Shape to be in the space of the second (i.e. shape1.pos should
	/// be set to shape1.pos - shape2.pos).
	/// </summary>
	public static partial class ShapeCollisions
	{
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


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static RectangleF minkowskiDifference( Box first, Box second )
		{
			// we need the top-left of our first box but it must include our motion. Collider only modifies position with the motion so we
			// need to figure out what the motion was using just the position.
			var positionOffset = first.position - ( first.bounds.location + first.bounds.size / 2f );
			var topLeft = first.bounds.location + positionOffset - second.bounds.max;
			var fullSize = first.bounds.size + second.bounds.size;

			return new RectangleF( topLeft.X, topLeft.Y, fullSize.X, fullSize.Y );
		}

		
		#region Retired Polygon to Polygon

		static Vector2[] _satAxisArray = new Vector2[0];
		static float[] _satTimerPerAxis = new float[0];

		[Obsolete]
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


		[Obsolete]
		public static bool polygonToPolygonOLD( Polygon first, Polygon second, out CollisionResult result )
		{
			result = new CollisionResult();
			float timeOfCollision;

			if( polygonToPolygon( first, second, null, out result.normal, out timeOfCollision ) )
			{
				result.minimumTranslationVector = result.normal * timeOfCollision;
				return true;
			}

			return false;
		}


		[Obsolete]
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


		[Obsolete]
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


		[Obsolete]
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


	}
}

