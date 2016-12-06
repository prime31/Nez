using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Polygon : Shape
	{
		/// <summary>
		/// the points that make up the Polygon. They should be CW and convex.
		/// </summary>
		public Vector2[] points;

		/// <summary>
		/// edge normals are used for SAT collision detection. We cache them to avoid the squareroots. Note that Boxes will only have
		/// 2 edgeNormals since the other two sides are parallel.
		/// </summary>
		public Vector2[] edgeNormals
		{
			get
			{
				if( _areEdgeNormalsDirty )
					buildEdgeNormals();
				return _edgeNormals;
			}
		}

		bool _areEdgeNormalsDirty = true;
		public Vector2[] _edgeNormals;

		// we cache the original details of our polygon
		internal Vector2[] _originalPoints;
		internal Vector2 _polygonCenter;

		// used as an optimization for unrotated Box collisions
		internal bool isBox;
		public bool isUnrotated = true;


		/// <summary>
		/// constructs a Polygon from points. points should be specified in clockwise fashion without duplicating the first/last point and
		/// they should be centered around 0,0.
		/// </summary>
		/// <param name="points">Points.</param>
		public Polygon( Vector2[] points )
		{
			this.points = points;
			recalculateCenterAndEdgeNormals();

			_originalPoints = new Vector2[points.Length];
			Array.Copy( points, _originalPoints, points.Length );
		}


		internal Polygon( Vector2[] points, bool isBox )
		{
			this.points = points;
			this.isBox = isBox;
			recalculateCenterAndEdgeNormals();

			_originalPoints = new Vector2[points.Length];
			Array.Copy( points, _originalPoints, points.Length );
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
		public void recalculateCenterAndEdgeNormals()
		{
			_polygonCenter = findPolygonCenter( points );
			_areEdgeNormalsDirty = true;
		}


		/// <summary>
		/// builds the Polygon edge normals. These are lazily created and updated only by the edgeNormals getter
		/// </summary>
		void buildEdgeNormals()
		{
			// for boxes we only require 2 edges since the other 2 are parallel
			var totalEdges = isBox ? 2 : points.Length;
			if( _edgeNormals == null || _edgeNormals.Length != totalEdges )
				_edgeNormals = new Vector2[totalEdges];
			
			Vector2 p2;
			for( var i = 0; i < totalEdges; i++ )
			{
				var p1 = points[i];
				if( i + 1 >= points.Length )
					p2 = points[0];
				else
					p2 = points[i + 1];

				var perp = Vector2Ext.perpendicular( ref p1, ref p2 );
				Vector2Ext.normalize( ref perp );
				_edgeNormals[i] = perp;
			}

			return;
		}


		#region static Polygon helpers

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
		/// recenters the points of the polygon
		/// </summary>
		/// <param name="points">Points.</param>
		public static void recenterPolygonVerts( Vector2[] points )
		{
			var center = findPolygonCenter( points );
			for( var i = 0; i < points.Length; i++ )
				points[i] -= center;
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


		// Dont know adjancent vertices so take each vertex
		// If you know adjancent vertices, perform hill climbing algorithm
		public static Vector2 getFarthestPointInDirection( Vector2[] points, Vector2 direction )
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
		public static Vector2 getClosestPointOnPolygonToPoint( Vector2[] points, Vector2 point, out float distanceSquared, out Vector2 edgeNormal )
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
					edgeNormal.X = -line.Y;
					edgeNormal.Y = line.X;
				}
			}

			Vector2Ext.normalize( ref edgeNormal );

			return closestPoint;
		}


		/// <summary>
		/// rotates the originalPoints and copys the rotated values to rotatedPoints
		/// </summary>
		/// <param name="radians">Radians.</param>
		/// <param name="originalPoints">Original points.</param>
		/// <param name="rotatedPoints">Rotated points.</param>
		public static void rotatePolygonVerts( float radians, Vector2[] originalPoints, Vector2[] rotatedPoints )
		{
			var cos = Mathf.cos( radians );
			var sin = Mathf.sin( radians );

			for( var i = 0; i < originalPoints.Length; i++ )
			{
				var position = originalPoints[i];
				rotatedPoints[i] = new Vector2( ( position.X * cos + position.Y * -sin ), ( position.X * sin + position.Y * cos ) );
			}
		}

		#endregion


		#region Shape abstract methods

		internal override void recalculateBounds( Collider collider )
		{
			// if we dont have rotation or dont care about TRS we use localOffset as the center so we'll start with that
			center = collider.localOffset;

			if( collider.shouldColliderScaleAndRotateWithTransform )
			{
				var hasUnitScale = true;
				Matrix2D tempMat;
				var combinedMatrix = Matrix2D.createTranslation( -_polygonCenter );

				if( collider.entity.transform.scale != Vector2.One )
				{
					Matrix2D.createScale( collider.entity.transform.scale.X, collider.entity.transform.scale.Y, out tempMat );
					Matrix2D.multiply( ref combinedMatrix, ref tempMat, out combinedMatrix );

					hasUnitScale = false;
					// scale our offset and set it as center. If we have rotation also it will be reset below
					var scaledOffset = collider.localOffset * collider.entity.transform.scale;
					center = scaledOffset;
				}

				if( collider.entity.transform.rotation != 0 )
				{
					Matrix2D.createRotation( collider.entity.transform.rotation, out tempMat );
					Matrix2D.multiply( ref combinedMatrix, ref tempMat, out combinedMatrix );

					// to deal with rotation with an offset origin we just move our center in a circle around 0,0 with our offset making the 0 angle
					// we have to deal with scale here as well so we scale our offset to get the proper length first.
					var offsetAngle = Mathf.atan2( collider.localOffset.Y, collider.localOffset.X ) * Mathf.rad2Deg;
					var offsetLength = hasUnitScale ? collider._localOffsetLength : ( collider.localOffset * collider.entity.transform.scale ).Length();
					center = Mathf.pointOnCircle( Vector2.Zero, offsetLength, collider.entity.transform.rotationDegrees + offsetAngle );
				}

				Matrix2D.createTranslation( ref _polygonCenter, out tempMat ); // translate back center
				Matrix2D.multiply( ref combinedMatrix, ref tempMat, out combinedMatrix );

				// finaly transform our original points
				Vector2Ext.transform( _originalPoints, ref combinedMatrix, points );

				isUnrotated = collider.entity.transform.rotation == 0;

				// we only need to rebuild our edge normals if we rotated
				if( collider._isRotationDirty )
					_areEdgeNormalsDirty = true;
			}

			position = collider.entity.transform.position + center;
			bounds = RectangleF.rectEncompassingPoints( points );
			bounds.location += position;
		}


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

