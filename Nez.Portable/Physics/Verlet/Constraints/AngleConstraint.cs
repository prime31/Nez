using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// constrains 3 particles to an angle
	/// </summary>
	public class AngleConstraint : Constraint
	{
		/// <summary>
		/// [0-1]. the stiffness of the Constraint. Lower values are more springy and higher are more rigid.
		/// </summary>
		public float Stiffness;

		/// <summary>
		/// the angle in radians that the Constraint will attempt to maintain
		/// </summary>
		public float AngleInRadians;

		Particle _particleA;
		Particle _centerParticle;
		Particle _particleC;


		public AngleConstraint(Particle a, Particle center, Particle c, float stiffness)
		{
			_particleA = a;
			_centerParticle = center;
			_particleC = c;
			Stiffness = stiffness;

			// not need for this Constraint to collide. There will be DistanceConstraints to do that if necessary
			CollidesWithColliders = false;

			AngleInRadians = AngleBetweenParticles();
		}


		float AngleBetweenParticles()
		{
			var first = _particleA.Position - _centerParticle.Position;
			var second = _particleC.Position - _centerParticle.Position;

			return Mathf.Atan2(first.X * second.Y - first.Y * second.X, first.X * second.X + first.Y * second.Y);
		}


		public override void Solve()
		{
			var angleBetween = AngleBetweenParticles();
			var diff = angleBetween - AngleInRadians;

			if (diff <= -MathHelper.Pi)
				diff += 2 * MathHelper.Pi;
			else if (diff >= MathHelper.Pi)
				diff -= 2 * MathHelper.Pi;

			diff *= Stiffness;

			_particleA.Position = Mathf.RotateAround(_particleA.Position, _centerParticle.Position, diff);
			_particleC.Position = Mathf.RotateAround(_particleC.Position, _centerParticle.Position, -diff);
			_centerParticle.Position = Mathf.RotateAround(_centerParticle.Position, _particleA.Position, diff);
			_centerParticle.Position = Mathf.RotateAround(_centerParticle.Position, _particleC.Position, -diff);
		}
	}
}