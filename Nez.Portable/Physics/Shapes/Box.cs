using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	/// <summary>
	/// special case of a Polygon. When doing SAT collision checks we only need to check 2 axes instead of 8
	/// </summary>
	public class Box : Polygon
	{
		public float Width;
		public float Height;


		public Box(float width, float height) : base(BuildBox(width, height), true)
		{
			isBox = true;
			Width = width;
			Height = height;
		}


		/// <summary>
		/// helper that builds the points a Polygon needs in the shape of a box
		/// </summary>
		/// <returns>The box.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		static Vector2[] BuildBox(float width, float height)
		{
			// we create our points around a center of 0,0
			var halfWidth = width / 2;
			var halfHeight = height / 2;
			var verts = new Vector2[4];

			verts[0] = new Vector2(-halfWidth, -halfHeight);
			verts[1] = new Vector2(halfWidth, -halfHeight);
			verts[2] = new Vector2(halfWidth, halfHeight);
			verts[3] = new Vector2(-halfWidth, halfHeight);

			return verts;
		}


		/// <summary>
		/// updates the Box points, recalculates the center and sets the width/height
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void UpdateBox(float width, float height)
		{
			Width = width;
			Height = height;

			// we create our points around a center of 0,0
			var halfWidth = width / 2;
			var halfHeight = height / 2;

			Points[0] = new Vector2(-halfWidth, -halfHeight);
			Points[1] = new Vector2(halfWidth, -halfHeight);
			Points[2] = new Vector2(halfWidth, halfHeight);
			Points[3] = new Vector2(-halfWidth, halfHeight);

			for (var i = 0; i < Points.Length; i++)
				_originalPoints[i] = Points[i];
		}


		#region Shape abstract methods

		public override bool Overlaps(Shape other)
		{
			// special, high-performance cases. otherwise we fall back to Polygon.
			if (IsUnrotated)
			{
				if (other is Box && (other as Box).IsUnrotated)
					return bounds.Intersects(ref (other as Box).bounds);

				if (other is Circle)
					return Collisions.RectToCircle(ref bounds, other.position, (other as Circle).Radius);
			}

			// fallthrough to standard cases
			return base.Overlaps(other);
		}


		public override bool CollidesWithShape(Shape other, out CollisionResult result)
		{
			// special, high-performance cases. otherwise we fall back to Polygon.
			if (IsUnrotated && other is Box && (other as Box).IsUnrotated)
				return ShapeCollisions.BoxToBox(this, other as Box, out result);

			// TODO: get Minkowski working for circle to box
			//if( other is Circle )

			// fallthrough to standard cases
			return base.CollidesWithShape(other, out result);
		}


		public override bool ContainsPoint(Vector2 point)
		{
			if (IsUnrotated)
				return bounds.Contains(point);

			return base.ContainsPoint(point);
		}


		public override bool PointCollidesWithShape(Vector2 point, out CollisionResult result)
		{
			if (IsUnrotated)
				return ShapeCollisions.PointToBox(point, this, out result);

			return base.PointCollidesWithShape(point, out result);
		}

		#endregion
	}
}