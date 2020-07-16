using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	public class Cloth : Composite
	{
		/// <summary>
		/// creates a Cloth. If connectHorizontalParticles is false it will not link horizontal Particles and create a hair-like cloth
		/// </summary>
		/// <param name="topLeftPosition">Top left position.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="segments">Segments.</param>
		/// <param name="stiffness">Stiffness.</param>
		/// <param name="tearSensitivity">Tear sensitivity.</param>
		/// <param name="connectHorizontalParticles">If set to <c>true</c> connect horizontal particles.</param>
		public Cloth(Vector2 topLeftPosition, float width, float height, int segments = 20, float stiffness = 0.25f,
		             float tearSensitivity = 5, bool connectHorizontalParticles = true)
		{
			var xStride = width / segments;
			var yStride = height / segments;

			for (var y = 0; y < segments; y++)
			{
				for (var x = 0; x < segments; x++)
				{
					var px = topLeftPosition.X + x * xStride;
					var py = topLeftPosition.Y + y * yStride;
					var particle = AddParticle(new Particle(new Vector2(px, py)));

					// remove this constraint to make only vertical constaints for a hair-like cloth
					if (connectHorizontalParticles && x > 0)
						AddConstraint(new DistanceConstraint(Particles[y * segments + x],
								Particles[y * segments + x - 1], stiffness))
							.SetTearSensitivity(tearSensitivity)
							.SetCollidesWithColliders(false);

					if (y > 0)
						AddConstraint(new DistanceConstraint(Particles[y * segments + x],
								Particles[(y - 1) * segments + x], stiffness))
							.SetTearSensitivity(tearSensitivity)
							.SetCollidesWithColliders(false);

					if (y == 0)
						particle.Pin();
				}
			}
		}
	}
}