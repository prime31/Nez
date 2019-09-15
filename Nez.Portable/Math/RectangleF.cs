using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Describes a 2D-rectangle. 
	/// </summary>
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct RectangleF : IEquatable<RectangleF>
	{
		static RectangleF emptyRectangle = new RectangleF();

		/// <summary>
		/// The x coordinate of the top-left corner of this <see cref="RectangleF"/>.
		/// </summary>
		public float X;

		/// <summary>
		/// The y coordinate of the top-left corner of this <see cref="RectangleF"/>.
		/// </summary>
		public float Y;

		/// <summary>
		/// The width of this <see cref="RectangleF"/>.
		/// </summary>
		public float Width;

		/// <summary>
		/// The height of this <see cref="RectangleF"/>.
		/// </summary>
		public float Height;


		#region Public Properties

		/// <summary>
		/// Returns a <see cref="RectangleF"/> with X=0, Y=0, Width=0, Height=0.
		/// </summary>
		public static RectangleF Empty => emptyRectangle;

		/// <summary>
		/// returns a RectangleF of float.Min/Max values
		/// </summary>
		/// <value>The max rect.</value>
		public static RectangleF MaxRect => new RectangleF(float.MinValue / 2, float.MinValue / 2, float.MaxValue, float.MaxValue);

		/// <summary>
		/// Returns the x coordinate of the left edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Left => X;

		/// <summary>
		/// Returns the x coordinate of the right edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Right => (X + Width);

		/// <summary>
		/// Returns the y coordinate of the top edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Top => Y;

		/// <summary>
		/// Returns the y coordinate of the bottom edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float Bottom => (Y + Height);

		/// <summary>
		/// gets the max point of the rectangle, the bottom-right corner
		/// </summary>
		/// <value>The max.</value>
		public Vector2 Max => new Vector2(Right, Bottom);

		/// <summary>
		/// Whether or not this <see cref="RectangleF"/> has a <see cref="Width"/> and
		/// <see cref="Height"/> of 0, and a <see cref="Location"/> of (0, 0).
		/// </summary>
		public bool IsEmpty => ((((Width == 0) && (Height == 0)) && (X == 0)) && (Y == 0));

		/// <summary>
		/// The top-left coordinates of this <see cref="RectangleF"/>.
		/// </summary>
		public Vector2 Location
		{
			get => new Vector2(X, Y);
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		/// <summary>
		/// The width-height coordinates of this <see cref="RectangleF"/>.
		/// </summary>
		public Vector2 Size
		{
			get => new Vector2(Width, Height);
			set
			{
				Width = value.X;
				Height = value.Y;
			}
		}

		/// <summary>
		/// A <see cref="Point"/> located in the center of this <see cref="RectangleF"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="Width"/> or <see cref="Height"/> is an odd number,
		/// the center point will be rounded down.
		/// </remarks>
		public Vector2 Center => new Vector2(X + (Width / 2), Y + (Height / 2));

		#endregion

		// temp Matrixes used for bounds calculation
		static Matrix2D _tempMat, _transformMat;


		internal string DebugDisplayString =>
			string.Concat(
				X, "  ",
				Y, "  ",
				Width, "  ",
				Height
			);


		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
		/// position, width, and height.
		/// </summary>
		/// <param name="x">The x coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="y">The y coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="width">The width of the created <see cref="RectangleF"/>.</param>
		/// <param name="height">The height of the created <see cref="RectangleF"/>.</param>
		public RectangleF(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}


		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
		/// location and size.
		/// </summary>
		/// <param name="location">The x and y coordinates of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="size">The width and height of the created <see cref="RectangleF"/>.</param>
		public RectangleF(Vector2 location, Vector2 size)
		{
			X = location.X;
			Y = location.Y;
			Width = size.X;
			Height = size.Y;
		}


		/// <summary>
		/// creates a RectangleF given min/max points (top-left, bottom-right points)
		/// </summary>
		/// <returns>The minimum max points.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static RectangleF FromMinMax(Vector2 min, Vector2 max)
		{
			return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
		}


		/// <summary>
		/// creates a RectangleF given min/max points (top-left, bottom-right points)
		/// </summary>
		/// <returns>The minimum max points.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static RectangleF FromMinMax(float minX, float minY, float maxX, float maxY)
		{
			return new RectangleF(minX, minY, maxX - minX, maxY - minY);
		}


		/// <summary>
		/// given the points of a polygon calculates the bounds
		/// </summary>
		/// <returns>The from polygon points.</returns>
		/// <param name="points">Points.</param>
		public static RectangleF RectEncompassingPoints(Vector2[] points)
		{
			// we need to find the min/max x/y values
			var minX = float.PositiveInfinity;
			var minY = float.PositiveInfinity;
			var maxX = float.NegativeInfinity;
			var maxY = float.NegativeInfinity;

			for (var i = 0; i < points.Length; i++)
			{
				var pt = points[i];

				if (pt.X < minX)
					minX = pt.X;
				if (pt.X > maxX)
					maxX = pt.X;

				if (pt.Y < minY)
					minY = pt.Y;
				if (pt.Y > maxY)
					maxY = pt.Y;
			}

			return FromMinMax(minX, minY, maxX, maxY);
		}


		#region Public Methods

		/// <summary>
		/// gets the position of the specified edge
		/// </summary>
		/// <returns>The side.</returns>
		/// <param name="edge">Side.</param>
		public float GetSide(Edge edge)
		{
			switch (edge)
			{
				case Edge.Top:
					return Top;
				case Edge.Bottom:
					return Bottom;
				case Edge.Left:
					return Left;
				case Edge.Right:
					return Right;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(int x, int y)
		{
			return ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(float x, float y)
		{
			return ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Point value)
		{
			return ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) &&
			        (value.Y < (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Point value, out bool result)
		{
			result = ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) &&
			          (value.Y < (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(Vector2 value)
		{
			return ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) &&
			        (value.Y < (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref Vector2 value, out bool result)
		{
			result = ((((X <= value.X) && (value.X < (X + Width))) && (Y <= value.Y)) &&
			          (value.Y < (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool Contains(RectangleF value)
		{
			return ((((X <= value.X) && ((value.X + value.Width) <= (X + Width))) &&
			         (Y <= value.Y)) && ((value.Y + value.Height) <= (Y + Height)));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void Contains(ref RectangleF value, out bool result)
		{
			result = ((((X <= value.X) && ((value.X + value.Width) <= (X + Width))) &&
			           (Y <= value.Y)) && ((value.Y + value.Height) <= (Y + Height)));
		}


		/// <summary>
		/// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts. 
		/// </summary>
		/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
		/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
		public void Inflate(int horizontalAmount, int verticalAmount)
		{
			X -= horizontalAmount;
			Y -= verticalAmount;
			Width += horizontalAmount * 2;
			Height += verticalAmount * 2;
		}


		/// <summary>
		/// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts. 
		/// </summary>
		/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
		/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
		public void Inflate(float horizontalAmount, float verticalAmount)
		{
			X -= horizontalAmount;
			Y -= verticalAmount;
			Width += horizontalAmount * 2;
			Height += verticalAmount * 2;
		}


		/// <summary>
		/// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <returns><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
		public bool Intersects(RectangleF value)
		{
			return value.Left < Right &&
			       Left < value.Right &&
			       value.Top < Bottom &&
			       Top < value.Bottom;
		}


		/// <summary>
		/// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <param name="result"><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
		public void Intersects(ref RectangleF value, out bool result)
		{
			result = value.Left < Right &&
			         Left < value.Right &&
			         value.Top < Bottom &&
			         Top < value.Bottom;
		}


		/// <summary>
		/// returns true if other intersects rect
		/// </summary>
		/// <param name="other">other.</param>
		public bool Intersects(ref RectangleF other)
		{
			bool result;
			Intersects(ref other, out result);
			return result;
		}


		public bool RayIntersects(ref Ray2D ray, out float distance)
		{
			distance = 0f;
			var maxValue = float.MaxValue;

			if (Math.Abs(ray.Direction.X) < 1E-06f)
			{
				if ((ray.Start.X < X) || (ray.Start.X > X + Width))
					return false;
			}
			else
			{
				var num11 = 1f / ray.Direction.X;
				var num8 = (X - ray.Start.X) * num11;
				var num7 = (X + Width - ray.Start.X) * num11;
				if (num8 > num7)
				{
					var num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				distance = MathHelper.Max(num8, distance);
				maxValue = MathHelper.Min(num7, maxValue);
				if (distance > maxValue)
					return false;
			}

			if (Math.Abs(ray.Direction.Y) < 1E-06f)
			{
				if ((ray.Start.Y < Y) || (ray.Start.Y > Y + Height))
				{
					return false;
				}
			}
			else
			{
				var num10 = 1f / ray.Direction.Y;
				var num6 = (Y - ray.Start.Y) * num10;
				var num5 = (Y + Height - ray.Start.Y) * num10;
				if (num6 > num5)
				{
					var num13 = num6;
					num6 = num5;
					num5 = num13;
				}

				distance = MathHelper.Max(num6, distance);
				maxValue = MathHelper.Min(num5, maxValue);
				if (distance > maxValue)
					return false;
			}

			return true;
		}


		public float? RayIntersects(Ray ray)
		{
			var num = 0f;
			var maxValue = float.MaxValue;

			if (Math.Abs(ray.Direction.X) < 1E-06f)
			{
				if ((ray.Position.X < Left) || (ray.Position.X > Right))
					return null;
			}
			else
			{
				float num11 = 1f / ray.Direction.X;
				float num8 = (Left - ray.Position.X) * num11;
				float num7 = (Right - ray.Position.X) * num11;
				if (num8 > num7)
				{
					float num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				num = MathHelper.Max(num8, num);
				maxValue = MathHelper.Min(num7, maxValue);
				if (num > maxValue)
					return null;
			}

			if (Math.Abs(ray.Direction.Y) < 1E-06f)
			{
				if ((ray.Position.Y < Top) || (ray.Position.Y > Bottom))
					return null;
			}
			else
			{
				float num10 = 1f / ray.Direction.Y;
				float num6 = (Top - ray.Position.Y) * num10;
				float num5 = (Bottom - ray.Position.Y) * num10;
				if (num6 > num5)
				{
					float num13 = num6;
					num6 = num5;
					num5 = num13;
				}

				num = MathHelper.Max(num6, num);
				maxValue = MathHelper.Min(num5, maxValue);
				if (num > maxValue)
					return null;
			}

			return new float?(num);
		}


		public Vector2 GetClosestPointOnBoundsToOrigin()
		{
			var max = Max;
			var minDist = Math.Abs(Location.X);
			var boundsPoint = new Vector2(Location.X, 0);

			if (Math.Abs(max.X) < minDist)
			{
				minDist = Math.Abs(max.X);
				boundsPoint.X = max.X;
				boundsPoint.Y = 0f;
			}

			if (Math.Abs(max.Y) < minDist)
			{
				minDist = Math.Abs(max.Y);
				boundsPoint.X = 0f;
				boundsPoint.Y = max.Y;
			}

			if (Math.Abs(Location.Y) < minDist)
			{
				minDist = Math.Abs(Location.Y);
				boundsPoint.X = 0;
				boundsPoint.Y = Location.Y;
			}

			return boundsPoint;
		}


		/// <summary>
		/// returns the closest point that is in or on the RectangleF to the given point
		/// </summary>
		/// <returns>The closest point on rectangle to point.</returns>
		/// <param name="point">Point.</param>
		public Vector2 GetClosestPointOnRectangleFToPoint(Vector2 point)
		{
			// for each axis, if the point is outside the box clamp it to the box else leave it alone
			var res = new Vector2();
			res.X = MathHelper.Clamp(point.X, Left, Right);
			res.Y = MathHelper.Clamp(point.Y, Top, Bottom);

			return res;
		}


		/// <summary>
		/// gets the closest point that is on the rectangle border to the given point
		/// </summary>
		/// <returns>The closest point on rectangle border to point.</returns>
		/// <param name="point">Point.</param>
		public Vector2 GetClosestPointOnRectangleBorderToPoint(Vector2 point, out Vector2 edgeNormal)
		{
			edgeNormal = Vector2.Zero;

			// for each axis, if the point is outside the box clamp it to the box else leave it alone
			var res = new Vector2();
			res.X = MathHelper.Clamp(point.X, Left, Right);
			res.Y = MathHelper.Clamp(point.Y, Top, Bottom);

			// if point is inside the rectangle we need to push res to the border since it will be inside the rect
			if (Contains(res))
			{
				var dl = res.X - Left;
				var dr = Right - res.X;
				var dt = res.Y - Top;
				var db = Bottom - res.Y;

				var min = Mathf.MinOf(dl, dr, dt, db);
				if (min == dt)
				{
					res.Y = Top;
					edgeNormal.Y = -1;
				}
				else if (min == db)
				{
					res.Y = Bottom;
					edgeNormal.Y = 1;
				}
				else if (min == dl)
				{
					res.X = Left;
					edgeNormal.X = -1;
				}
				else
				{
					res.X = Right;
					edgeNormal.X = 1;
				}
			}
			else
			{
				if (res.X == Left)
					edgeNormal.X = -1;
				if (res.X == Right)
					edgeNormal.X = 1;
				if (res.Y == Top)
					edgeNormal.Y = -1;
				if (res.Y == Bottom)
					edgeNormal.Y = 1;
			}

			return res;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>Overlapping region of the two rectangles.</returns>
		public static RectangleF Intersect(RectangleF value1, RectangleF value2)
		{
			RectangleF rectangle;
			Intersect(ref value1, ref value2, out rectangle);
			return rectangle;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
		public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
		{
			if (value1.Intersects(value2))
			{
				var right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
				var left_side = Math.Max(value1.X, value2.X);
				var top_side = Math.Max(value1.Y, value2.Y);
				var bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
				result = new RectangleF(left_side, top_side, right_side - left_side, bottom_side - top_side);
			}
			else
			{
				result = new RectangleF(0, 0, 0, 0);
			}
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
		public void Offset(int offsetX, int offsetY)
		{
			X += offsetX;
			Y += offsetY;
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
		public void Offset(float offsetX, float offsetY)
		{
			X += offsetX;
			Y += offsetY;
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
		public void Offset(Point amount)
		{
			X += amount.X;
			Y += amount.Y;
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
		public void Offset(Vector2 amount)
		{
			X += amount.X;
			Y += amount.Y;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>The union of the two rectangles.</returns>
		public static RectangleF Union(RectangleF value1, RectangleF value2)
		{
			var x = Math.Min(value1.X, value2.X);
			var y = Math.Min(value1.Y, value2.Y);
			return new RectangleF(x, y,
				Math.Max(value1.Right, value2.Right) - x,
				Math.Max(value1.Bottom, value2.Bottom) - y);
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">The union of the two rectangles as an output parameter.</param>
		public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
		{
			result.X = Math.Min(value1.X, value2.X);
			result.Y = Math.Min(value1.Y, value2.Y);
			result.Width = Math.Max(value1.Right, value2.Right) - result.X;
			result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> where the rectangles overlap.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>The overlap of the two rectangles.</returns>
		public static RectangleF Overlap(RectangleF value1, RectangleF value2)
		{
			var x = Math.Max(Math.Max(value1.X, value2.X), 0);
			var y = Math.Max(Math.Max(value1.Y, value2.Y), 0);
			return new RectangleF(x, y,
				Math.Max(Math.Min(value1.Right, value2.Right) - x, 0),
				Math.Max(Math.Min(value1.Bottom, value2.Bottom) - y, 0));
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> where the rectangles overlap.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">The overlap of the two rectangles as an output parameter.</param>
		public static void Overlap(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
		{
			result.X = Math.Max(Math.Max(value1.X, value2.X), 0);
			result.Y = Math.Max(Math.Max(value1.Y, value2.Y), 0);
			result.Width = Math.Max(Math.Min(value1.Right, value2.Right) - result.X, 0);
			result.Height = Math.Max(Math.Min(value1.Bottom, value2.Bottom) - result.Y, 0);
		}


		public void CalculateBounds(Vector2 parentPosition, Vector2 position, Vector2 origin, Vector2 scale,
		                            float rotation, float width, float height)
		{
			if (rotation == 0f)
			{
				X = parentPosition.X + position.X - origin.X * scale.X;
				Y = parentPosition.Y + position.Y - origin.Y * scale.Y;
				Width = width * scale.X;
				Height = height * scale.Y;
			}
			else
			{
				// special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
				var worldPosX = parentPosition.X + position.X;
				var worldPosY = parentPosition.Y + position.Y;

				// set the reference point to world reference taking origin into account
				Matrix2D.CreateTranslation(-worldPosX - origin.X, -worldPosY - origin.Y, out _transformMat);
				Matrix2D.CreateScale(scale.X, scale.Y, out _tempMat); // scale ->
				Matrix2D.Multiply(ref _transformMat, ref _tempMat, out _transformMat);
				Matrix2D.CreateRotation(rotation, out _tempMat); // rotate ->
				Matrix2D.Multiply(ref _transformMat, ref _tempMat, out _transformMat);
				Matrix2D.CreateTranslation(worldPosX, worldPosY, out _tempMat); // translate back
				Matrix2D.Multiply(ref _transformMat, ref _tempMat, out _transformMat);

				// TODO: this is a bit silly. we can just leave the worldPos translation in the Matrix and avoid this
				// get all four corners in world space
				var topLeft = new Vector2(worldPosX, worldPosY);
				var topRight = new Vector2(worldPosX + width, worldPosY);
				var bottomLeft = new Vector2(worldPosX, worldPosY + height);
				var bottomRight = new Vector2(worldPosX + width, worldPosY + height);

				// transform the corners into our work space
				Vector2Ext.Transform(ref topLeft, ref _transformMat, out topLeft);
				Vector2Ext.Transform(ref topRight, ref _transformMat, out topRight);
				Vector2Ext.Transform(ref bottomLeft, ref _transformMat, out bottomLeft);
				Vector2Ext.Transform(ref bottomRight, ref _transformMat, out bottomRight);

				// find the min and max values so we can concoct our bounding box
				var minX = Mathf.MinOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
				var maxX = Mathf.MaxOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
				var minY = Mathf.MinOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);
				var maxY = Mathf.MaxOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);

				Location = new Vector2(minX, minY);
				Width = maxX - minX;
				Height = maxY - minY;
			}
		}


		/// <summary>
		/// returns a RectangleF that spans the current rect and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public RectangleF GetSweptBroadphaseBounds(float deltaX, float deltaY)
		{
			var broadphasebox = Empty;

			broadphasebox.X = deltaX > 0 ? X : X + deltaX;
			broadphasebox.Y = deltaY > 0 ? Y : Y + deltaY;
			broadphasebox.Width = deltaX > 0 ? deltaX + Width : Width - deltaX;
			broadphasebox.Height = deltaY > 0 ? deltaY + Height : Height - deltaY;

			return broadphasebox;
		}


		/// <summary>
		/// returns true if the boxes are colliding
		/// moveX and moveY will return the movement that b1 must move to avoid the collision
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="moveX">Move x.</param>
		/// <param name="moveY">Move y.</param>
		public bool CollisionCheck(ref RectangleF other, out float moveX, out float moveY)
		{
			moveX = moveY = 0.0f;

			var l = other.X - (X + Width);
			var r = (other.X + other.Width) - X;
			var t = other.Y - (Y + Height);
			var b = (other.Y + other.Height) - Y;

			// check that there was a collision
			if (l > 0 || r < 0 || t > 0 || b < 0)
				return false;

			// find the offset of both sides
			moveX = Math.Abs(l) < r ? l : r;
			moveY = Math.Abs(t) < b ? t : b;

			// only use whichever offset is the smallest
			if (Math.Abs(moveX) < Math.Abs(moveY))
				moveY = 0.0f;
			else
				moveX = 0.0f;

			return true;
		}


		/// <summary>
		/// Calculates the signed depth of intersection between two rectangles.
		/// </summary>
		/// <returns>
		/// The amount of overlap between two intersecting rectangles. These depth values can be negative depending on which sides the rectangles
		/// intersect. This allows callers to determine the correct direction to push objects in order to resolve collisions.
		/// If the rectangles are not intersecting, Vector2.Zero is returned.
		/// </returns>
		public static Vector2 GetIntersectionDepth(ref RectangleF rectA, ref RectangleF rectB)
		{
			// calculate half sizes
			var halfWidthA = rectA.Width / 2.0f;
			var halfHeightA = rectA.Height / 2.0f;
			var halfWidthB = rectB.Width / 2.0f;
			var halfHeightB = rectB.Height / 2.0f;

			// calculate centers
			var centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
			var centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

			// calculate current and minimum-non-intersecting distances between centers
			var distanceX = centerA.X - centerB.X;
			var distanceY = centerA.Y - centerB.Y;
			var minDistanceX = halfWidthA + halfWidthB;
			var minDistanceY = halfHeightA + halfHeightB;

			// if we are not intersecting at all, return (0, 0)
			if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
				return Vector2.Zero;

			// calculate and return intersection depths
			var depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
			var depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

			return new Vector2(depthX, depthY);
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return (obj is RectangleF) && this == ((RectangleF) obj);
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="other">The <see cref="RectangleF"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(RectangleF other)
		{
			return this == other;
		}


		/// <summary>
		/// Gets the hash code of this <see cref="RectangleF"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="RectangleF"/>.</returns>
		public override int GetHashCode()
		{
			return ((int) X ^ (int) Y ^ (int) Width ^ (int) Height);
		}


		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="RectangleF"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="RectangleF"/>.</returns>
		public override string ToString()
		{
			return string.Format("X:{0}, Y:{1}, Width: {2}, Height: {3}", X, Y, Width, Height);
		}

		#endregion


		#region Operators

		/// <summary>
		/// Compares whether two <see cref="RectangleF"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="RectangleF"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="RectangleF"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(RectangleF a, RectangleF b)
		{
			return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
		}


		/// <summary>
		/// Compares whether two <see cref="RectangleF"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="RectangleF"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="RectangleF"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(RectangleF a, RectangleF b)
		{
			return !(a == b);
		}


		public static implicit operator Rectangle(RectangleF self)
		{
			return RectangleExt.FromFloats(self.X, self.Y, self.Width, self.Height);
		}

		#endregion
	}
}