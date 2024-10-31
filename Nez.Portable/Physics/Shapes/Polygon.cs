using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Polygon : Shape
	{
		/// <summary>
		/// the points that make up the Polygon. They should be CW and convex.
		/// </summary>
		public Vector2[] Points;

		/// <summary>
		/// edge normals are used for SAT collision detection. We cache them to avoid the squareroots. Note that Boxes will only have
		/// 2 edgeNormals since the other two sides are parallel.
		/// </summary>
		public Vector2[] EdgeNormals
		{
			get
			{
				if (AreEdgeNormalsDirty)
					BuildEdgeNormals();
				return _edgeNormals;
			}
		}

		public bool AreEdgeNormalsDirty = true;
		public Vector2[] _edgeNormals;

		// we cache the original details of our polygon
		public Vector2[] OriginalPoints;
		public Vector2 PolygonCenter;

		// used as an optimization for unrotated Box collisions
		public bool IsBox;
		public bool IsUnrotated = true;


		/// <summary>
		/// constructs a Polygon from points. points should be specified in clockwise fashion without duplicating the first/last point and
		/// they should be centered around 0,0.
		/// </summary>
		/// <param name="points">Points.</param>
		public Polygon(Vector2[] points) => SetPoints(points);

		/// <summary>
		/// creates a symmetrical polygon based on the radius and vertCount passed in
		/// </summary>
		/// <param name="vertCount">Vert count.</param>
		/// <param name="radius">Radius.</param>
		public Polygon(int vertCount, float radius) => SetPoints(BuildSymmetricalPolygon(vertCount, radius));

		public Polygon(Vector2[] points, bool isBox) : this(points) => this.IsBox = isBox;

		/// <summary>
		/// resets the points and recalculates center and edge normals
		/// </summary>
		/// <param name="points"></param>
		public void SetPoints(Vector2[] points)
		{
			Points = points;
			RecalculateCenterAndEdgeNormals();

			OriginalPoints = new Vector2[points.Length];
			Array.Copy(points, OriginalPoints, points.Length);
		}

		/// <summary>
		/// recalculates the Polygon centers. This must be called if the points are changed!
		/// </summary>
		public void RecalculateCenterAndEdgeNormals()
		{
			PolygonCenter = FindPolygonCenter(Points);
			AreEdgeNormalsDirty = true;
		}

		/// <summary>
		/// builds the Polygon edge normals. These are lazily created and updated only by the edgeNormals getter
		/// </summary>
		void BuildEdgeNormals()
		{
			// for boxes we only require 2 edges since the other 2 are parallel
			var totalEdges = IsBox ? 2 : Points.Length;
			if (_edgeNormals == null || _edgeNormals.Length != totalEdges)
				_edgeNormals = new Vector2[totalEdges];

			Vector2 p2;
			for (var i = 0; i < totalEdges; i++)
			{
				var p1 = Points[i];
				if (i + 1 >= Points.Length)
					p2 = Points[0];
				else
					p2 = Points[i + 1];

				var perp = Vector2Ext.Perpendicular(ref p1, ref p2);
				Vector2Ext.Normalize(ref perp);
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
		public static Vector2[] BuildSymmetricalPolygon(int vertCount, float radius)
		{
			var verts = new Vector2[vertCount];

			for (var i = 0; i < vertCount; i++)
			{
				var a = 2.0f * MathHelper.Pi * (i / (float) vertCount);
				verts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
			}

			return verts;
		}

		/// <summary>
		/// recenters the points of the polygon
		/// </summary>
		/// <param name="points">Points.</param>
		public static void RecenterPolygonVerts(Vector2[] points)
		{
			var center = FindPolygonCenter(points);
			for (var i = 0; i < points.Length; i++)
				points[i] -= center;
		}

		/// <summary>
		/// finds the center of the Polygon. Note that this will be accurate for regular polygons. Irregular polygons have no center.
		/// </summary>
		/// <returns>The polygon center.</returns>
		/// <param name="points">Points.</param>
		public static Vector2 FindPolygonCenter(Vector2[] points)
		{
			float x = 0, y = 0;

			for (var i = 0; i < points.Length; i++)
			{
				x += points[i].X;
				y += points[i].Y;
			}

			return new Vector2(x / points.Length, y / points.Length);
		}

		// Dont know adjancent vertices so take each vertex
		// If you know adjancent vertices, perform hill climbing algorithm
		public static Vector2 GetFarthestPointInDirection(Vector2[] points, Vector2 direction)
		{
			var index = 0;
			Vector2.Dot(ref points[index], ref direction, out float maxDot);

			for (var i = 1; i < points.Length; i++)
			{
				Vector2.Dot(ref points[i], ref direction, out float dot);
				if (dot > maxDot)
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
		public static Vector2 GetClosestPointOnPolygonToPoint(Vector2[] points, Vector2 point,
		                                                      out float distanceSquared, out Vector2 edgeNormal)
		{
			distanceSquared = float.MaxValue;
			edgeNormal = Vector2.Zero;
			var closestPoint = Vector2.Zero;

			float tempDistanceSquared;
			for (var i = 0; i < points.Length; i++)
			{
				var j = i + 1;
				if (j == points.Length)
					j = 0;

				var closest = ShapeCollisions.ClosestPointOnLine(points[i], points[j], point);
				Vector2.DistanceSquared(ref point, ref closest, out tempDistanceSquared);

				if (tempDistanceSquared < distanceSquared)
				{
					distanceSquared = tempDistanceSquared;
					closestPoint = closest;

					// get the normal of the line
					var line = points[j] - points[i];
					edgeNormal.X = -line.Y;
					edgeNormal.Y = line.X;
				}
			}

			Vector2Ext.Normalize(ref edgeNormal);

			return closestPoint;
		}

		/// <summary>
		/// rotates the originalPoints and copys the rotated values to rotatedPoints
		/// </summary>
		/// <param name="radians">Radians.</param>
		/// <param name="originalPoints">Original points.</param>
		/// <param name="rotatedPoints">Rotated points.</param>
		public static void RotatePolygonVerts(float radians, Vector2[] originalPoints, Vector2[] rotatedPoints)
		{
			var cos = Mathf.Cos(radians);
			var sin = Mathf.Sin(radians);

			for (var i = 0; i < originalPoints.Length; i++)
			{
				var position = originalPoints[i];
				rotatedPoints[i] = new Vector2((position.X * cos + position.Y * -sin),
					(position.X * sin + position.Y * cos));
			}
		}

		#endregion


		#region Shape abstract methods

		public override void RecalculateBounds(Collider collider)
		{
			// if we dont have rotation or dont care about TRS we use localOffset as the center so we'll start with that
			Center = collider.LocalOffset;

			if (collider.ShouldColliderScaleAndRotateWithTransform)
			{
				var hasUnitScale = true;
				Matrix2D tempMat;
				var combinedMatrix = Matrix2D.CreateTranslation(-PolygonCenter);

				if (collider.Entity.Transform.Scale != Vector2.One)
				{
					Matrix2D.CreateScale(collider.Entity.Transform.Scale.X, collider.Entity.Transform.Scale.Y,
						out tempMat);
					Matrix2D.Multiply(ref combinedMatrix, ref tempMat, out combinedMatrix);

					hasUnitScale = false;

					// scale our offset and set it as center. If we have rotation also it will be reset below
					var scaledOffset = collider.LocalOffset * collider.Entity.Transform.Scale;
					Center = scaledOffset;
				}

				if (collider.Entity.Transform.Rotation != 0)
				{
					Matrix2D.CreateRotation(collider.Entity.Transform.Rotation, out tempMat);
					Matrix2D.Multiply(ref combinedMatrix, ref tempMat, out combinedMatrix);

					// to deal with rotation with an offset origin we just move our center in a circle around 0,0 with our offset making the 0 angle
					// we have to deal with scale here as well so we scale our offset to get the proper length first.
					var offsetAngle = Mathf.Atan2(collider.LocalOffset.Y * collider.Entity.Transform.Scale.Y, collider.LocalOffset.X * collider.Entity.Transform.Scale.X) * Mathf.Rad2Deg;
					var offsetLength = hasUnitScale
						? collider._localOffsetLength
						: (collider.LocalOffset * collider.Entity.Transform.Scale).Length();
					Center = Mathf.PointOnCircle(Vector2.Zero, offsetLength,
						collider.Entity.Transform.RotationDegrees + offsetAngle);
				}

				Matrix2D.CreateTranslation(ref PolygonCenter, out tempMat); // translate back center
				Matrix2D.Multiply(ref combinedMatrix, ref tempMat, out combinedMatrix);

				// finaly transform our original points
				Vector2Ext.Transform(OriginalPoints, ref combinedMatrix, Points);

				IsUnrotated = collider.Entity.Transform.Rotation == 0;

				// we only need to rebuild our edge normals if we rotated
				if (collider._isRotationDirty)
					AreEdgeNormalsDirty = true;
			}

			Position = collider.Entity.Transform.Position + Center;
			Bounds = RectangleF.RectEncompassingPoints(Points);
			Bounds.Location += Position;
		}

		public override bool Overlaps(Shape other)
		{
			CollisionResult result;
			if (other is Polygon)
				return ShapeCollisions.PolygonToPolygon(this, other as Polygon, out result);

			if (other is Circle)
			{
				if (ShapeCollisions.CircleToPolygon(other as Circle, this, out result))
				{
					result.InvertResult();
					return true;
				}

				return false;
			}

			throw new NotImplementedException(string.Format("overlaps of Polygon to {0} are not supported", other));
		}

		public override bool CollidesWithShape(Shape other, out CollisionResult result)
		{
			if (other is Polygon)
				return ShapeCollisions.PolygonToPolygon(this, other as Polygon, out result);

			if (other is Circle)
			{
				if (ShapeCollisions.CircleToPolygon(other as Circle, this, out result))
				{
					result.InvertResult();
					return true;
				}

				return false;
			}

			throw new NotImplementedException(string.Format("overlaps of Polygon to {0} are not supported", other));
		}

		public override bool CollidesWithLine(Vector2 start, Vector2 end, out RaycastHit hit)
		{
			hit = new RaycastHit();
			return ShapeCollisions.LineToPoly(start, end, this, out hit);
		}

		/// <summary>
		/// essentially what the algorithm is doing is shooting a ray from point out. If it intersects an odd number of polygon sides
		/// we know it is inside the polygon.
		/// </summary>
		public override bool ContainsPoint(Vector2 point)
		{
			// normalize the point to be in our Polygon coordinate space
			point -= Position;

			var isInside = false;
			for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i++)
			{
				if (((Points[i].Y > point.Y) != (Points[j].Y > point.Y)) &&
				    (point.X < (Points[j].X - Points[i].X) * (point.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) +
				     Points[i].X))
				{
					isInside = !isInside;
				}
			}

			return isInside;
		}

		public override bool PointCollidesWithShape(Vector2 point, out CollisionResult result)
		{
			return ShapeCollisions.PointToPoly(point, this, out result);
		}

		#endregion
	}
}
