using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Polygon : Shape
	{
		public Vector2[] points;
		internal bool isBox;


		public Polygon( Vector2[] points )
		{
			this.points = points;
		}


		/// <summary>
		/// creates a symmetrical polygon based on the radius and vertCount passed in
		/// </summary>
		/// <param name="vertCount">Vert count.</param>
		/// <param name="radius">Radius.</param>
		public Polygon( int vertCount, float radius ) : this( buildSymmetricalPolygon( vertCount, radius ) )
		{}


		public static Vector2[] buildSymmetricalPolygon( int vertCount, float radius )
		{
			var verts = new Vector2[vertCount];

			for( var i = 0; i < vertCount; i++ )
			{
				var a = 2.0f * MathHelper.Pi * ( i / (float)vertCount );
				verts[i] = new Vector2( Mathf.cos( a ), Mathf.sin( a ) ) * radius;
			}

			return verts;
		}


		internal override void recalculateBounds( Collider collider )
		{
			position = collider.absolutePosition;
			bounds = RectangleF.rectEncompassingPoints( points );
			bounds.location += position;
		}


		// Dont know  adjancent vertices so take each vertex
		// If you know adjancent vertices, perform hill climbing algorithm
		public Vector2 getFarthestPointInDirection( Vector2 direction )
		{
			var index = 0;
			float dot;
			float maxDot;
			Vector2.Dot( ref points[index], ref direction, out maxDot );

			for( var i = 1; i < points.Length; i++ )
			{
				Vector2.Dot( ref points[i], ref direction, out dot );

				if( dot > maxDot )
				{
					maxDot = dot;
					index = i;
				}
			}

			return points[index];
		}


		/// <summary>
		/// iterates all the edges of the polygon and gets the closest point on any edge to point. Returns via out the squared distance
		/// to the closest point and the normal of the edge it is on.
		/// </summary>
		/// <returns>The closest point on polygon to point.</returns>
		/// <param name="point">Point.</param>
		/// <param name="distanceSquared">Distance squared.</param>
		/// <param name="edgeNormal">Edge normal.</param>
		public Vector2 getClosestPointOnPolygonToPoint( Vector2 point, out float distanceSquared, out Vector2 edgeNormal )
		{
			distanceSquared = float.MaxValue;
			edgeNormal = Vector2.Zero;
			var closestPoint = Vector2.Zero;
			float tempDistanceSquared;
			for( var i = 0; i < points.Length; i++ )
			{
				var j = i + 1;
				if( j == points.Length )
					j = 0;
				
				var closest = ShapeCollisions.closestPointOnLine( points[i], points[j], point );
				Vector2.DistanceSquared( ref point, ref closest, out tempDistanceSquared );

				if( tempDistanceSquared < distanceSquared )
				{
					distanceSquared = tempDistanceSquared;
					closestPoint = closest;

					// get the normal of the line
					var line = points[j] - points[i];
					edgeNormal.X = line.Y;
					edgeNormal.Y = -line.X;
				}
			}

			return closestPoint;
		}


		#region Shape abstract methods

		public override bool overlaps( Shape other )
		{
			CollisionResult result;
			if( other is Polygon )
				return ShapeCollisions.polygonToPolygon( this, other as Polygon, out result );

			if( other is Circle )
			{
				if( ShapeCollisions.circleToPolygon( other as Circle, this, out result ) )
				{
					result.invertResult();
					return true;
				}
				return false;
			}

			throw new NotImplementedException( string.Format( "overlaps of Polygon to {0} are not supported", other ) );
		}


		public override bool collidesWithShape( Shape other, out CollisionResult result )
		{
			if( other is Polygon )
				return ShapeCollisions.polygonToPolygon( this, other as Polygon, out result );

			if( other is Circle )
			{
				if( ShapeCollisions.circleToPolygon( other as Circle, this, out result ) )
				{
					result.invertResult();
					return true;
				}
				return false;
			}

			throw new NotImplementedException( string.Format( "overlaps of Polygon to {0} are not supported", other ) );
		}


		public override bool collidesWithLine( Vector2 start, Vector2 end, out RaycastHit hit )
		{
			hit = new RaycastHit();
			return ShapeCollisions.lineToPoly( start, end, this, out hit );
		}

		#endregion

	}
}

