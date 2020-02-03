using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Verlet
{
	/// <summary>
	/// maintains a specified distance betweeen two Particles. The stiffness adjusts how rigid or springy the constraint will be.
	/// </summary>
	public class DistanceConstraint : Constraint
	{
		/// <summary>
		/// [0-1]. the stiffness of the Constraint. Lower values are more springy and higher are more rigid.
		/// </summary>
		public float Stiffness;

		/// <summary>
		/// the resting distnace of the Constraint. It will always try to get to this distance.
		/// </summary>
		public float RestingDistance;

		/// <summary>
		/// if the ratio of the current distance / restingDistance is greater than tearSensitivity the Constaint will be removed. Values
		/// should be above 1 and higher values mean rupture wont occur until the Constaint is stretched further.
		/// </summary>
		public float TearSensitivity = float.PositiveInfinity;

		/// <summary>
		/// sets whether collisions should be approximated by points. This should be used for Constraints that need to collided on both
		/// sides. SAT only works with single sided collisions.
		/// </summary>
		public bool ShouldApproximateCollisionsWithPoints;

		/// <summary>
		/// if shouldApproximateCollisionsWithPoints is true, this controls how accurate the collisions check will be. Higher numbers mean
		/// more collisions checks.
		/// </summary>
		public int TotalPointsToApproximateCollisionsWith = 5;

		/// <summary>
		/// the first Particle in the Constraint
		/// </summary>
		Particle _particleOne;

		/// <summary>
		/// the second particle in the Constraint
		/// </summary>
		Particle _particleTwo;

		/// <summary>
		/// Polygon shared amongst all DistanceConstraints. Used for collision detection.
		/// </summary>
		static Polygon _polygon = new Polygon(2, 1);


		public DistanceConstraint(Particle first, Particle second, float stiffness, float distance = -1)
		{
			_particleOne = first;
			_particleTwo = second;
			Stiffness = stiffness;

			if (distance > -1)
				RestingDistance = distance;
			else
				RestingDistance = Vector2.Distance(first.Position, second.Position);
		}


		/// <summary>
		/// creates a faux angle constraint by figuring out the required distance from a to c for the given angle
		/// </summary>
		/// <param name="a">The alpha component.</param>
		/// <param name="center">Center.</param>
		/// <param name="c">C.</param>
		/// <param name="stiffness">Stiffness.</param>
		/// <param name="angleInDegrees">Angle in degrees.</param>
		public static DistanceConstraint Create(Particle a, Particle center, Particle c, float stiffness,
		                                        float angleInDegrees)
		{
			var aToCenter = Vector2.Distance(a.Position, center.Position);
			var cToCenter = Vector2.Distance(c.Position, center.Position);
			var distance = Mathf.Sqrt(aToCenter * aToCenter + cToCenter * cToCenter -
			                          (2 * aToCenter * cToCenter * Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)));

			return new DistanceConstraint(a, c, stiffness, distance);
		}


		/// <summary>
		/// sets the tear sensitivity. if the ratio of the current distance / restingDistance is greater than tearSensitivity the
		/// Constaint will be removed
		/// </summary>
		/// <returns>The tear sensitvity.</returns>
		/// <param name="tearSensitivity">Tear sensitivity.</param>
		public DistanceConstraint SetTearSensitivity(float tearSensitivity)
		{
			TearSensitivity = tearSensitivity;
			return this;
		}


		/// <summary>
		/// sets whether this Constraint should collide with standard Colliders
		/// </summary>
		/// <returns>The collides with colliders.</returns>
		/// <param name="collidesWithColliders">If set to <c>true</c> collides with colliders.</param>
		public DistanceConstraint SetCollidesWithColliders(bool collidesWithColliders)
		{
			CollidesWithColliders = collidesWithColliders;
			return this;
		}


		/// <summary>
		/// sets whether collisions should be approximated by points. This should be used for Constraints that need to collided on both
		/// sides. SAT only works with single sided collisions.
		/// </summary>
		/// <returns>The should approximate collisions with points.</returns>
		/// <param name="shouldApproximateCollisionsWithPoints">If set to <c>true</c> should approximate collisions with points.</param>
		public DistanceConstraint SetShouldApproximateCollisionsWithPoints(bool shouldApproximateCollisionsWithPoints)
		{
			ShouldApproximateCollisionsWithPoints = shouldApproximateCollisionsWithPoints;
			return this;
		}


		public override void Solve()
		{
			// calculate the distance between the two Particles
			var diff = _particleOne.Position - _particleTwo.Position;
			var d = diff.Length();

			// find the difference, or the ratio of how far along the restingDistance the actual distance is.
			var difference = (RestingDistance - d) / d;

			// if the distance is more than tearSensitivity we remove the Constraint
			if (d / RestingDistance > TearSensitivity)
			{
				composite.RemoveConstraint(this);
				return;
			}

			// inverse the mass quantities
			var im1 = 1f / _particleOne.Mass;
			var im2 = 1f / _particleTwo.Mass;
			var scalarP1 = (im1 / (im1 + im2)) * Stiffness;
			var scalarP2 = Stiffness - scalarP1;

			// push/pull based on mass
			// heavier objects will be pushed/pulled less than attached light objects
			_particleOne.Position += diff * scalarP1 * difference;
			_particleTwo.Position -= diff * scalarP2 * difference;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void HandleCollisions(int collidesWithLayers)
		{
			if (ShouldApproximateCollisionsWithPoints)
			{
				ApproximateCollisionsWithPoints(collidesWithLayers);
				return;
			}

			// get a proper bounds for our line and update the Polygons bounds
			var minX = Math.Min(_particleOne.Position.X, _particleTwo.Position.X);
			var maxX = Math.Max(_particleOne.Position.X, _particleTwo.Position.X);
			var minY = Math.Min(_particleOne.Position.Y, _particleTwo.Position.Y);
			var maxY = Math.Max(_particleOne.Position.Y, _particleTwo.Position.Y);
			_polygon.bounds = RectangleF.FromMinMax(minX, minY, maxX, maxY);

			Vector2 midPoint;
			PreparePolygonForCollisionChecks(out midPoint);

			var colliders = Physics.BoxcastBroadphase(ref _polygon.bounds, collidesWithLayers);
			foreach (var collider in colliders)
			{
				CollisionResult result;
				if (_polygon.CollidesWithShape(collider.Shape, out result))
				{
					// TODO: do we need this?
					// special care needs to be taken for a Circle. Since our Polygon is only a single segment with a normal off both edges
					// we could end up with a backwards result. We fix that by seeing if the normal is in the direction of the Circle
					// center. If it is, we flip the result.
					//if( collider.shape is Circle )
					//{
					//	var dot = Vector2.Dot( midPoint - collider.shape.position, result.normal );
					//	if( dot < 0 )
					//		result.invertResult();
					//}

					_particleOne.Position -= result.MinimumTranslationVector;
					_particleTwo.Position -= result.MinimumTranslationVector;
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ApproximateCollisionsWithPoints(int collidesWithLayers)
		{
			Vector2 pt;
			for (var j = 0; j < TotalPointsToApproximateCollisionsWith - 1; j++)
			{
				pt = Vector2.Lerp(_particleOne.Position, _particleTwo.Position,
					(j + 1) / (float) TotalPointsToApproximateCollisionsWith);
				var collidedCount = Physics.OverlapCircleAll(pt, 3, VerletWorld._colliders, collidesWithLayers);
				for (var i = 0; i < collidedCount; i++)
				{
					var collider = VerletWorld._colliders[i];
					CollisionResult collisionResult;
					if (collider.Shape.PointCollidesWithShape(pt, out collisionResult))
					{
						_particleOne.Position -= collisionResult.MinimumTranslationVector;
						_particleTwo.Position -= collisionResult.MinimumTranslationVector;
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void PreparePolygonForCollisionChecks(out Vector2 midPoint)
		{
			// set our Polygon points
			midPoint = Vector2.Lerp(_particleOne.Position, _particleTwo.Position, 0.5f);
			_polygon.position = midPoint;
			_polygon.Points[0] = _particleOne.Position - _polygon.position;
			_polygon.Points[1] = _particleTwo.Position - _polygon.position;
			_polygon.RecalculateCenterAndEdgeNormals();
		}


		public override void DebugRender(Batcher batcher)
		{
			batcher.DrawLine(_particleOne.Position, _particleTwo.Position, Debug.Colors.VerletConstraintEdge);
		}
	}
}