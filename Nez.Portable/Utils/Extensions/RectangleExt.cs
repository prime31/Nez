using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class RectangleExt
	{
		/// <summary>
		/// gets the position of the specified side
		/// </summary>
		/// <returns>The side.</returns>
		/// <param name="edge">Side.</param>
		public static int GetSide(this Rectangle rect, Edge edge)
		{
			switch (edge)
			{
				case Edge.Top:
					return rect.Top;
				case Edge.Bottom:
					return rect.Bottom;
				case Edge.Left:
					return rect.Left;
				case Edge.Right:
					return rect.Right;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static Rectangle GetHalfRect(this Rectangle rect, Edge edge)
		{
			switch (edge)
			{
				case Edge.Top:
					return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2);
				case Edge.Bottom:
					return new Rectangle(rect.X, rect.Y + rect.Height / 2, rect.Width, rect.Height / 2);
				case Edge.Left:
					return new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height);
				case Edge.Right:
					return new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// gets a portion of the Rectangle with a width/height of size that is on the Edge of the Rectangle but still contained within it.
		/// </summary>
		/// <returns>The rect edge portion.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="edge">Edge.</param>
		/// <param name="size">Size.</param>
		public static Rectangle GetRectEdgePortion(this Rectangle rect, Edge edge, int size = 1)
		{
			switch (edge)
			{
				case Edge.Top:
					return new Rectangle(rect.X, rect.Y, rect.Width, size);
				case Edge.Bottom:
					return new Rectangle(rect.X, rect.Y + rect.Height - size, rect.Width, size);
				case Edge.Left:
					return new Rectangle(rect.X, rect.Y, size, rect.Height);
				case Edge.Right:
					return new Rectangle(rect.X + rect.Width - size, rect.Y, size, rect.Height);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void ExpandSide(ref Rectangle rect, Edge edge, int amount)
		{
			// ensure we have a positive value
			amount = Math.Abs(amount);

			switch (edge)
			{
				case Edge.Top:
					rect.Y -= amount;
					rect.Height += amount;
					break;
				case Edge.Bottom:
					rect.Height += amount;
					break;
				case Edge.Left:
					rect.X -= amount;
					rect.Width += amount;
					break;
				case Edge.Right:
					rect.Width += amount;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void Contract(ref Rectangle rect, int horizontalAmount, int verticalAmount)
		{
			rect.X += horizontalAmount;
			rect.Y += verticalAmount;
			rect.Width -= horizontalAmount * 2;
			rect.Height -= verticalAmount * 2;
		}

		/// <summary>
		/// returns a rectangle from the passed in floats
		/// </summary>
		/// <returns>The floats.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static Rectangle FromFloats(float x, float y, float width, float height)
		{
			return new Rectangle((int) x, (int) y, (int) width, (int) height);
		}

		/// <summary>
		/// creates a Rectangle given min/max points (top-left, bottom-right points)
		/// </summary>
		/// <returns>The minimum max points.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static Rectangle FromMinMaxPoints(Point min, Point max)
		{
			return new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
		}

		/// <summary>
		/// calculates the union of the two Rectangles. The result will be a rectangle that encompasses the other two.
		/// </summary>
		public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
		{
			result.X = Math.Min(value1.X, value2.X);
			result.Y = Math.Min(value1.Y, value2.Y);
			result.Width = Math.Max(value1.Right, value2.Right) - result.X;
			result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
		}

		/// <summary>
		/// Update first to be the union of first and point
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="point">Point.</param>
		/// <param name="result">Result.</param>
		public static void Union(ref Rectangle first, ref Point point, out Rectangle result)
		{
			var rect = new Rectangle(point.X, point.Y, 0, 0);
			Union(ref first, ref rect, out result);
		}

		/// <summary>
		/// given the points of a polygon calculates the bounds
		/// </summary>
		/// <returns>The from polygon points.</returns>
		/// <param name="points">Points.</param>
		public static Rectangle BoundsFromPolygonPoints(Vector2[] points)
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

			return FromMinMaxPoints(new Point((int) minX, (int) minY), new Point((int) maxX, (int) maxY));
		}

		public static void CalculateBounds(ref Rectangle rect, Vector2 parentPosition, Vector2 position, Vector2 origin,
		                                   Vector2 scale, float rotation, float width, float height)
		{
			if (rotation == 0f)
			{
				rect.X = (int) (parentPosition.X + position.X - origin.X * scale.X);
				rect.Y = (int) (parentPosition.Y + position.Y - origin.Y * scale.Y);
				rect.Width = (int) (width * scale.X);
				rect.Height = (int) (height * scale.Y);
			}
			else
			{
				// special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
				var worldPosX = parentPosition.X + position.X;
				var worldPosY = parentPosition.Y + position.Y;

				Matrix2D tempMat;

				// set the reference point to world reference taking origin into account
				var transformMatrix = Matrix2D.CreateTranslation(-worldPosX - origin.X, -worldPosY - origin.Y);
				Matrix2D.CreateScale(scale.X, scale.Y, out tempMat); // scale ->
				Matrix2D.Multiply(ref transformMatrix, ref tempMat, out transformMatrix);
				Matrix2D.CreateRotation(rotation, out tempMat); // rotate ->
				Matrix2D.Multiply(ref transformMatrix, ref tempMat, out transformMatrix);
				Matrix2D.CreateTranslation(worldPosX, worldPosY, out tempMat); // translate back
				Matrix2D.Multiply(ref transformMatrix, ref tempMat, out transformMatrix);

				// TODO: this is a bit silly. we can just leave the worldPos translation in the Matrix and avoid this
				// get all four corners in world space
				var topLeft = new Vector2(worldPosX, worldPosY);
				var topRight = new Vector2(worldPosX + width, worldPosY);
				var bottomLeft = new Vector2(worldPosX, worldPosY + height);
				var bottomRight = new Vector2(worldPosX + width, worldPosY + height);

				// transform the corners into our work space
				Vector2Ext.Transform(ref topLeft, ref transformMatrix, out topLeft);
				Vector2Ext.Transform(ref topRight, ref transformMatrix, out topRight);
				Vector2Ext.Transform(ref bottomLeft, ref transformMatrix, out bottomLeft);
				Vector2Ext.Transform(ref bottomRight, ref transformMatrix, out bottomRight);

				// find the min and max values so we can concoct our bounding box
				var minX = (int) Mathf.MinOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
				var maxX = (int) Mathf.MaxOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
				var minY = (int) Mathf.MinOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);
				var maxY = (int) Mathf.MaxOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);

				rect.Location = new Point(minX, minY);
				rect.Width = (int) (maxX - minX);
				rect.Height = (int) (maxY - minY);
			}
		}

		/// <summary>
		/// clones and returns a new Rectangle with the same data as the current rectangle
		/// </summary>
		/// <param name="rect">Rect.</param>
		public static Rectangle Clone(this Rectangle rect) => new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);


		/// <summary>
		/// scales the rect
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="scale">Scale.</param>
		public static void Scale(ref Rectangle rect, Vector2 scale)
		{
			rect.X = (int) (rect.X * scale.X);
			rect.Y = (int) (rect.Y * scale.Y);
			rect.Width = (int) (rect.Width * scale.X);
			rect.Height = (int) (rect.Height * scale.Y);
		}

		public static void Translate(ref Rectangle rect, Vector2 vec) => rect.Location += vec.ToPoint();

		public static bool RayIntersects(ref Rectangle rect, ref Ray2D ray, out float distance)
		{
			distance = 0f;
			var maxValue = float.MaxValue;

			if (Math.Abs(ray.Direction.X) < 1E-06f)
			{
				if ((ray.Start.X < rect.X) || (ray.Start.X > rect.X + rect.Width))
					return false;
			}
			else
			{
				var num11 = 1f / ray.Direction.X;
				var num8 = (rect.X - ray.Start.X) * num11;
				var num7 = (rect.X + rect.Width - ray.Start.X) * num11;
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
				if ((ray.Start.Y < rect.Y) || (ray.Start.Y > rect.Y + rect.Height))
				{
					return false;
				}
			}
			else
			{
				var num10 = 1f / ray.Direction.Y;
				var num6 = (rect.Y - ray.Start.Y) * num10;
				var num5 = (rect.Y + rect.Height - ray.Start.Y) * num10;
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

		public static float? RayIntersects(this Rectangle rectangle, Ray ray)
		{
			var num = 0f;
			var maxValue = float.MaxValue;

			if (Math.Abs(ray.Direction.X) < 1E-06f)
			{
				if ((ray.Position.X < rectangle.Left) || (ray.Position.X > rectangle.Right))
					return null;
			}
			else
			{
				float num11 = 1f / ray.Direction.X;
				float num8 = (rectangle.Left - ray.Position.X) * num11;
				float num7 = (rectangle.Right - ray.Position.X) * num11;
				if (num8 > num7)
				{
					float num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				num = MathHelper.Max(num8, num);
				maxValue = MathHelper.Min(num7, maxValue);
				if (num > maxValue)
				{
					return null;
				}
			}

			if (Math.Abs(ray.Direction.Y) < 1E-06f)
			{
				if ((ray.Position.Y < rectangle.Top) || (ray.Position.Y > rectangle.Bottom))
				{
					return null;
				}
			}
			else
			{
				float num10 = 1f / ray.Direction.Y;
				float num6 = (rectangle.Top - ray.Position.Y) * num10;
				float num5 = (rectangle.Bottom - ray.Position.Y) * num10;
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

		/// <summary>
		/// returns a Bounds the spans the current bounds and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		public static Rectangle GetSweptBroadphaseBounds(ref Rectangle rect, float deltaX, float deltaY) => GetSweptBroadphaseBounds(ref rect, (int)deltaX, (int)deltaY);

		/// <summary>
		/// returns a Bounds the spans the current bounds and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		public static Rectangle GetSweptBroadphaseBounds(ref Rectangle rect, int deltaX, int deltaY)
		{
			var broadphasebox = Rectangle.Empty;

			broadphasebox.X = deltaX > 0 ? rect.X : rect.X + deltaX;
			broadphasebox.Y = deltaY > 0 ? rect.Y : rect.Y + deltaY;
			broadphasebox.Width = deltaX > 0 ? deltaX + rect.Width : rect.Width - deltaX;
			broadphasebox.Height = deltaY > 0 ? deltaY + rect.Height : rect.Height - deltaY;

			return broadphasebox;
		}

		/// <summary>
		/// returns true if rect1 intersects rect2
		/// </summary>
		public static bool Intersect(ref Rectangle rect1, ref Rectangle rect2)
		{
			bool result;
			rect1.Intersects(ref rect2, out result);
			return result;
		}

		/// <summary>
		/// returns true if the boxes are colliding
		/// moveX and moveY will return the movement that b1 must move to avoid the collision
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="moveX">Move x.</param>
		/// <param name="moveY">Move y.</param>
		public static bool CollisionCheck(ref Rectangle rect, ref Rectangle other, out float moveX, out float moveY)
		{
			moveX = moveY = 0.0f;

			var l = other.X - (rect.X + rect.Width);
			var r = (other.X + other.Width) - rect.X;
			var t = other.Y - (rect.Y + rect.Height);
			var b = (other.Y + other.Height) - rect.Y;

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
		public static Vector2 GetIntersectionDepth(ref Rectangle rectA, ref Rectangle rectB)
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

		public static Vector2 GetClosestPointOnBoundsToOrigin(ref Rectangle rect)
		{
			var max = GetMax(ref rect);
			var minDist = Math.Abs(rect.Location.X);
			var boundsPoint = new Vector2(rect.Location.X, 0);

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

			if (Math.Abs(rect.Location.Y) < minDist)
			{
				minDist = Math.Abs(rect.Location.Y);
				boundsPoint.X = 0;
				boundsPoint.Y = rect.Location.Y;
			}

			return boundsPoint;
		}

		/// <summary>
		/// returns the closest point that is in or on the Rectangle to the given point
		/// </summary>
		/// <returns>The closest point on rectangle to point.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="point">Point.</param>
		public static Vector2 GetClosestPointOnRectangleToPoint(ref Rectangle rect, Vector2 point)
		{
			// for each axis, if the point is outside the box clamp it to the box else leave it alone
			var res = new Vector2();
			res.X = MathHelper.Clamp(point.X, rect.Left, rect.Right);
			res.Y = MathHelper.Clamp(point.Y, rect.Top, rect.Bottom);

			return res;
		}

		/// <summary>
		/// gets the closest point that is on the rectangle border to the given point
		/// </summary>
		/// <returns>The closest point on rectangle border to point.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="point">Point.</param>
		public static Point GetClosestPointOnRectangleBorderToPoint(ref Rectangle rect, Vector2 point)
		{
			// for each axis, if the point is outside the box clamp it to the box else leave it alone
			var res = new Point();
			res.X = Mathf.Clamp((int) point.X, rect.Left, rect.Right);
			res.Y = Mathf.Clamp((int) point.Y, rect.Top, rect.Bottom);

			// if point is inside the rectangle we need to push res to the border since it will be inside the rect
			if (rect.Contains(res))
			{
				var dl = res.X - rect.Left;
				var dr = rect.Right - res.X;
				var dt = res.Y - rect.Top;
				var db = rect.Bottom - res.Y;

				var min = Mathf.MinOf(dl, dr, dt, db);
				if (min == dt)
					res.Y = rect.Top;
				else if (min == db)
					res.Y = rect.Bottom;
				else if (min == dl)
					res.X = rect.Left;
				else
					res.X = rect.Right;
			}

			return res;
		}

		/// <summary>
		/// gets the center point of the rectangle as a Vector2
		/// </summary>
		/// <returns>The center.</returns>
		/// <param name="rect">Rect.</param>
		public static Vector2 GetCenter(ref Rectangle rect) => new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

		/// <summary>
		/// gets the center point of the rectangle as a Vector2
		/// </summary>
		/// <returns>The center.</returns>
		/// <param name="rect">Rect.</param>
		public static Vector2 GetCenter(this Rectangle rect) => new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

		/// <summary>
		/// gets the half size of the rect
		/// </summary>
		/// <returns>The half size.</returns>
		/// <param name="rect">Rect.</param>
		public static Vector2 GetHalfSize(this Rectangle rect) => new Vector2(rect.Width * 0.5f, rect.Height * 0.5f);

		/// <summary>
		/// gets the max point of the rectangle, the bottom-right corner
		/// </summary>
		/// <returns>The max.</returns>
		/// <param name="rect">Rect.</param>
		public static Point GetMax(ref Rectangle rect) => new Point(rect.Right, rect.Bottom);

		/// <summary>
		/// Gets the size of the Rectangle
		/// </summary>
		public static Point GetSize(this Rectangle rect) => new Point(rect.Width, rect.Height);

		/// <summary>
		/// gets the position of the rectangle as a Vector2
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="rect">Rect.</param>
		public static Vector2 GetPosition(ref Rectangle rect) => new Vector2(rect.X, rect.Y);
	}
}