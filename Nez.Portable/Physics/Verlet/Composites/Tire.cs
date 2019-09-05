using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	public class Tire : Composite
	{
		public Tire(Vector2 origin, float radius, int segments, float spokeStiffness = 1, float treadStiffness = 1)
		{
			var stride = 2 * MathHelper.Pi / segments;

			// particles
			for (var i = 0; i < segments; i++)
			{
				var theta = i * stride;
				AddParticle(new Particle(new Vector2(origin.X + Mathf.Cos(theta) * radius,
					origin.Y + Mathf.Sin(theta) * radius)));
			}

			var centerParticle = AddParticle(new Particle(origin));

			// constraints
			for (var i = 0; i < segments; i++)
			{
				AddConstraint(new DistanceConstraint(Particles[i], Particles[(i + 1) % segments], treadStiffness));
				AddConstraint(new DistanceConstraint(Particles[i], centerParticle, spokeStiffness))
					.SetCollidesWithColliders(false);
				AddConstraint(new DistanceConstraint(Particles[i], Particles[(i + 5) % segments], treadStiffness));
			}
		}
	}
}