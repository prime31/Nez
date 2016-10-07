using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Polygon : Shape
	{
		public Vector2[] points;
		public Vector2 center;

		internal bool isBox;


		/// <summary>
		/// constructs a Polygon from points. points should be specified in clockwise fashion without duplicating the first/last point.
		/// </summary>
		/// <param name="points">Points.</param>
		public Polygon( Vector2[] points )
		{
			this.points = points;
			recalculateCenter();
		}


		/// <summary>
		/// creates a symmetrical polygon based on the radius and vertCount passed in
		/// </summary>
		/// <param name="vertCount">Vert count.</param>
		/// <param name="radius">Radius.</param>
		public Polygon( int vertCount, float radius ) : this( buildSymmetricalPolygon( vertCount, radius ) )
		{}


		/// <summary>
		/// recalculates the Polygon centers. This must be called if the points are changed!
		/// </summary>
		public void recalculateCenter()
		{
			center = findPolygonCenter( points );
		}


		/// <summary>
		/// builds the Polygon edges
		/// </summary>
		public Vector2[] buildEdges()
		{
			Vector2[] edges = null;

			if( points.Length == 2 )
			{
				if( edges == null || edges.Length != 1 )
					edges = new Vector2[1];
				edges[0] = points[1] - points[0];
				return edges;
			}

			if( edges == null || edges.Length != points.Length )
				edges = new Vector2[points.Length];
			
			Vector2 p1;
			Vector2 p2;
			for( int i = 0; i < points.Length; i++ )
			{
				p1 = points[i];
				if( i + 1 >= points.Length )
					p2 = points[0];
				else
					p2 = points[i + 1];

				edges[i] = p2 - p1;
			}

			return edges;
		}


		/// <summary>
		/// builds a symmetrical polygon (hexagon, octogon, n-gon) and returns the points
		/// </summary>
		/// <returns>The symmetrical polygon.</returns>
		/// <param name="vertCount">Vert count.</param>
		/// <param name="radius">Radius.</param>
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


		/// <summary>
		/// finds the center of the Polygon. Note that this will be accurate for regular polygons. Irregular polygons have no center.
		/// </summary>
		/// <returns>The polygon center.</returns>
		/// <param name="points">Points.</param>
		public static Vector2 findPolygonCenter( Vector2[] points )
		{
			float x = 0, y = 0;

			for( var i = 0; i < points.Length; i++ )
			{
				x += points[i].X;
				y += points[i].Y;
			}

			return new Vector2( x / points.Length, y / points.Length );
		}


		internal override void recalculateBounds( Collider collider )
		{
			position = collider.absolutePosition;
			bounds = RectangleF.rectEncompassingPoints( points );
			bounds.location += position;
		}


		// Dont know adjancent vertices so take each vertex
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
		/// to the closest point and the normal of the edge it is on. point should be in the space of the Polygon (point - poly.position)
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

			Vector2Ext.normalize( ref edgeNormal );

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


		/// <summary>
		/// essentially what the algorithm is doing is shooting a ray from point out. If it intersects an odd number of polygon sides
		/// we know it is inside the polygon.
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="point">Point.</param>
		public override bool containsPoint( Vector2 point )
		{
			// normalize the point to be in our Polygon coordinate space
			point -= position;

			var isInside = false;
			for( int i = 0, j = points.Length - 1; i < points.Length; j = i++ )
			{
				if( ( ( points[i].Y > point.Y ) != ( points[j].Y > point.Y ) ) &&
				( point.X < ( points[j].X - points[i].X ) * ( point.Y - points[i].Y ) / ( points[j].Y - points[i].Y ) + points[i].X ) )
				{
					isInside = !isInside;
				}
			}

			return isInside;
		}


		public override bool pointCollidesWithShape( Vector2 point, out CollisionResult result )
		{
			return ShapeCollisions.pointToPoly( point, this, out result );
		}

		#endregion

	}
}

