using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes.BETA
{
	public static class RealtimeCollisions
	{
		public static bool IntersectMovingCircleBox(Circle s, Box b, Vector2 movement, out float time)
		{
			// compute the AABB resulting from expanding b by sphere radius r
			var e = b.Bounds;
			e.Inflate(s.Radius, s.Radius);

			// Intersect ray against expanded expanded Rectangle e. Exit with no intersection if ray
			// misses e, else get intersection point p and time t as result
			var ray = new Ray2D(s.Position - movement, s.Position);
			if (!e.RayIntersects(ref ray, out time) && time > 1.0f)
				return false;

			// get the intersection point
			var point = ray.Start + ray.Direction * time;

			// compute which min and max faces of b the intersection point p lies outside of. Note, u and v cannot have the
			// same bits set and they must have at least one bit set among them.
			int u = 0, v = 0;
			if (point.X < b.Bounds.Left)
				u |= 1;
			if (point.X > b.Bounds.Right)
				v |= 1;
			if (point.Y < b.Bounds.Top)
				u |= 2;
			if (point.Y > b.Bounds.Bottom)
				v |= 2;

			// 'or' all set bits together into a bitmask (note u + v == u | v)
			var m = u + v;

			// if all 3 bits are set then point is in a vertex region
			if (m == 3)
			{
				// must now intersect segment against the capsules of the two edges meeting at the vert and return the best time,
				// if one or more hit
				// https://play.google.com/books/reader?printsec=frontcover&output=reader&id=VSoIBwAAAEAJ&pg=GBS.PA267
				// https://github.com/noonat/hello/blob/580b986f3bb27b93645087441d2744eeb99d6d35/hello/collisions/Collision.hx#L675
				//throw new NotImplementedException();
				Debug.Log("m == 3. corner {0}", Time.FrameCount);
			}

			// if only one bit is set in m then point is in a face region
			if ((m & (m - 1)) == 0)
			{
				Debug.DrawHollowBox(point, 4, Color.Black, 0.4f);

				// do nothing. time from the expanded rect intersection is the correct time
				return true;
			}

			// point is on an edge region. intersect against the capsule at the edge.

			return true;
		}


		/// <summary>
		/// support function that returns the rectangle vert with index n
		/// </summary>
		/// <param name="b">The blue component.</param>
		/// <param name="n">N.</param>
		static Vector2 Corner(Rectangle b, int n)
		{
			var p = new Vector2();
			p.X = (n & 1) == 0 ? b.Right : b.Left;
			p.Y = (n & 1) == 0 ? b.Bottom : b.Top;
			return p;
		}


		public static bool IntersectMovingCircleCircle(Circle s0, Circle s1, Vector2 movement, out float time)
		{
			time = -1;
			var s = s1.Position - s0.Position; // vector between circles
			var r = s1.Radius + s0.Radius; // sum of radii

			float c;
			Vector2.Dot(ref s, ref s, out c);
			c -= r * r;
			if (c < 0)
			{
				// circles already overlapping
				time = 0;
				return true;
			}

			float a;
			Vector2.Dot(ref movement, ref movement, out a);
			if (a < Mathf.Epsilon) // circles not moving
				return false;

			float b;
			Vector2.Dot(ref movement, ref s, out b);
			if (b >= 0) // circles not moving toward each other
				return false;

			var d = b * b - a * c;
			if (d < 0) // no real-valuded root. circles do not intersect.
				return false;

			time = (-b * Mathf.Sqrt(d)) / a;
			return true;
		}


		public static bool IntersectMovingCircleCircleTwo(Circle first, Circle second, Vector2 movement, out float time)
		{
			// this algorithm shrinks first down to a point, expands second by first.radius and uses a linecast to check for intersection
			time = -1;
			second.Radius += first.Radius;

			RaycastHit hit;
			var result = ShapeCollisions.LineToCircle(first.Position, first.Position + movement, second, out hit);
			time = hit.Fraction;

			// undo the radius addition
			second.Radius -= first.Radius;

			return result;
		}


		/// <summary>
		/// checks to see if circle overlaps box and returns (via out param) the point of intersection
		/// </summary>
		/// <returns><c>true</c>, if circle box was tested, <c>false</c> otherwise.</returns>
		/// <param name="circle">Circle.</param>
		/// <param name="box">Box.</param>
		/// <param name="point">Point.</param>
		public static bool TestCircleBox(Circle circle, Box box, out Vector2 point)
		{
			// find the point closest to the sphere center
			point = box.Bounds.GetClosestPointOnRectangleFToPoint(circle.Position);

			// circle and box intersect if sqr distance from circle center to point is less than the circle sqr radius
			var v = point - circle.Position;
			float dist;
			Vector2.Dot(ref v, ref v, out dist);

			return dist <= circle.Radius * circle.Radius;
		}
	}
}