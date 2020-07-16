﻿using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public class Circle : Shape
	{
		public float Radius;
		internal float _originalRadius;


		public Circle(float radius)
		{
			Radius = radius;
			_originalRadius = radius;
		}


		#region Shape abstract methods

		/// <summary>
		/// internal hack used by Particles so they can reuse a Circle for all collision checks
		/// </summary>
		/// <param name="radius">Radius.</param>
		/// <param name="position">Position.</param>
		internal void RecalculateBounds(float radius, Vector2 position)
		{
			_originalRadius = radius;
			Radius = radius;
			this.position = position;
			bounds = new RectangleF(position.X - radius, position.Y - radius, radius * 2f, radius * 2f);
		}


		internal override void RecalculateBounds(Collider collider)
		{
			// if we dont have rotation or dont care about TRS we use localOffset as the center so we'll start with that
			center = collider.LocalOffset;

			if (collider.ShouldColliderScaleAndRotateWithTransform)
			{
				// we only scale lineraly being a circle so we'll use the max value
				var scale = collider.Entity.Transform.Scale;
				var hasUnitScale = scale.X == 1 && scale.Y == 1;
				var maxScale = Math.Max(scale.X, scale.Y);
				Radius = _originalRadius * maxScale;

				if (collider.Entity.Transform.Rotation != 0)
				{
					// to deal with rotation with an offset origin we just move our center in a circle around 0,0 with our offset making the 0 angle
					var offsetAngle = Mathf.Atan2(collider.LocalOffset.Y, collider.LocalOffset.X) * Mathf.Rad2Deg;
					var offsetLength = hasUnitScale
						? collider._localOffsetLength
						: (collider.LocalOffset * collider.Entity.Transform.Scale).Length();
					center = Mathf.PointOnCircle(Vector2.Zero, offsetLength,
						collider.Entity.Transform.RotationDegrees + offsetAngle);
				}
			}

			position = collider.Entity.Transform.Position + center;
			bounds = new RectangleF(position.X - Radius, position.Y - Radius, Radius * 2f, Radius * 2f);
		}


		public override bool Overlaps(Shape other)
		{
			CollisionResult result;

			// Box is only optimized for unrotated
			if (other is Box && (other as Box).IsUnrotated)
				return Collisions.RectToCircle(ref other.bounds, position, Radius);

			if (other is Circle)
				return Collisions.CircleToCircle(position, Radius, other.position, (other as Circle).Radius);

			if (other is Polygon)
				return ShapeCollisions.CircleToPolygon(this, other as Polygon, out result);

			throw new NotImplementedException(string.Format("overlaps of Circle to {0} are not supported", other));
		}


		public override bool CollidesWithShape(Shape other, out CollisionResult result)
		{
			if (other is Box && (other as Box).IsUnrotated)
				return ShapeCollisions.CircleToBox(this, other as Box, out result);

			if (other is Circle)
				return ShapeCollisions.CircleToCircle(this, other as Circle, out result);

			if (other is Polygon)
				return ShapeCollisions.CircleToPolygon(this, other as Polygon, out result);

			throw new NotImplementedException(string.Format("Collisions of Circle to {0} are not supported", other));
		}


		public override bool CollidesWithLine(Vector2 start, Vector2 end, out RaycastHit hit)
		{
			hit = new RaycastHit();
			return ShapeCollisions.LineToCircle(start, end, this, out hit);
		}


		/// <summary>
		/// Gets whether or not the provided point lie within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="point">the point</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public override bool ContainsPoint(Vector2 point)
		{
			return ((point - position).LengthSquared() <= Radius * Radius);
		}

		#endregion


		/// <summary>
		/// Gets the point at the edge of this <see cref="Circle"/> from the provided angle
		/// </summary>
		/// <param name="angle">an angle in radians</param>
		/// <returns><see cref="Vector2"/> representing the point on this <see cref="Circle"/>'s surface at the specified angle</returns>
		public Vector2 GetPointAlongEdge(float angle)
		{
			return new Vector2(position.X + (Radius * Mathf.Cos(angle)), position.Y + (Radius * Mathf.Sin(angle)));
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="Circle"/>; <c>false</c> otherwise.</returns>
		public bool ContainsPoint(float x, float y)
		{
			return ContainsPoint(new Vector2(x, y));
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="Circle"/>.
		/// </summary>
		/// <param name="point">Point.</param>
		public bool ContainsPoint(ref Vector2 point)
		{
			return (point - position).LengthSquared() <= Radius * Radius;
		}


		public override bool PointCollidesWithShape(Vector2 point, out CollisionResult result)
		{
			return ShapeCollisions.PointToCircle(point, this, out result);
		}
	}
}