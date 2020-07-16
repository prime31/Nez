using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// a series of points connected with DistanceConstraints
	/// </summary>
	public class LineSegments : Composite
	{
		public LineSegments(Vector2[] vertices, float stiffness)
		{
			for (var i = 0; i < vertices.Length; i++)
			{
				var p = new Particle(vertices[i]);
				AddParticle(p);

				if (i > 0)
					AddConstraint(new DistanceConstraint(Particles.Buffer[i], Particles.Buffer[i - 1], stiffness));
			}
		}


		/// <summary>
		/// pins the Particle at the given index
		/// </summary>
		/// <param name="index">Index.</param>
		public LineSegments PinParticleAtIndex(int index)
		{
			Particles.Buffer[index].Pin();
			return this;
		}
	}
}