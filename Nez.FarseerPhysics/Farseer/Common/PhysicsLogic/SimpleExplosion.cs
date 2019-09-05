using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Common.PhysicsLogic
{
	/// <summary>
	/// Creates a simple explosion that ignores other bodies hiding behind static bodies.
	/// </summary>
	public sealed class SimpleExplosion : PhysicsLogic
	{
		/// <summary>
		/// This is the power used in the power function. A value of 1 means the force
		/// applied to bodies in the explosion is linear. A value of 2 means it is exponential.
		/// </summary>
		public float Power;


		public SimpleExplosion(World world) : base(world, PhysicsLogicType.Explosion)
		{
			Power = 1; //linear
		}


		/// <summary>
		/// Activate the explosion at the specified position.
		/// </summary>
		/// <param name="pos">The position (center) of the explosion.</param>
		/// <param name="radius">The radius of the explosion.</param>
		/// <param name="force">The force applied</param>
		/// <param name="maxForce">A maximum amount of force. When force gets over this value, it will be equal to maxForce</param>
		/// <returns>A list of bodies and the amount of force that was applied to them.</returns>
		public Dictionary<Body, Vector2> Activate(Vector2 pos, float radius, float force,
		                                          float maxForce = float.MaxValue)
		{
			var affectedBodies = new HashSet<Body>();

			AABB aabb;
			aabb.LowerBound = pos - new Vector2(radius);
			aabb.UpperBound = pos + new Vector2(radius);
			var radiusSquared = radius * radius;

			// Query the world for bodies within the radius.
			World.QueryAABB(fixture =>
			{
				if (Vector2.DistanceSquared(fixture.Body.Position, pos) <= radiusSquared)
					affectedBodies.Add(fixture.Body);

				return true;
			}, ref aabb);

			return ApplyImpulse(pos, radius, force, maxForce, affectedBodies);
		}


		Dictionary<Body, Vector2> ApplyImpulse(Vector2 pos, float radius, float force, float maxForce,
		                                       HashSet<Body> overlappingBodies)
		{
			Dictionary<Body, Vector2> forces = new Dictionary<Body, Vector2>(overlappingBodies.Count);

			foreach (Body overlappingBody in overlappingBodies)
			{
				if (IsActiveOn(overlappingBody))
				{
					var distance = Vector2.Distance(pos, overlappingBody.Position);
					var forcePercent = GetPercent(distance, radius);

					var forceVector = pos - overlappingBody.Position;
					forceVector *=
						1f / (float) Math.Sqrt(forceVector.X * forceVector.X + forceVector.Y * forceVector.Y);
					forceVector *= MathHelper.Min(force * forcePercent, maxForce);
					forceVector *= -1;

					overlappingBody.ApplyLinearImpulse(forceVector);
					forces.Add(overlappingBody, forceVector);
				}
			}

			return forces;
		}


		float GetPercent(float distance, float radius)
		{
			//(1-(distance/radius))^power-1
			float percent = (float) Math.Pow(1 - ((distance - radius) / radius), Power) - 1;

			if (float.IsNaN(percent))
				return 0f;

			return MathHelper.Clamp(percent, 0f, 1f);
		}
	}
}