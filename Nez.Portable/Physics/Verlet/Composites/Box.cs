using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// creates a simple box with diagonal contraints to keep it rigid
	/// </summary>
	public class Box : Composite
	{
		public Box(Vector2 center, float width, float height, float borderStiffness = 0.2f,
		           float diagonalStiffness = 0.5f)
		{
			var tl = AddParticle(new Particle(center + new Vector2(-width / 2, -height / 2)));
			var tr = AddParticle(new Particle(center + new Vector2(width / 2, -height / 2)));
			var br = AddParticle(new Particle(center + new Vector2(width / 2, height / 2)));
			var bl = AddParticle(new Particle(center + new Vector2(-width / 2, height / 2)));

			// outside edges
			AddConstraint(new DistanceConstraint(tl, tr, borderStiffness));
			AddConstraint(new DistanceConstraint(tr, br, borderStiffness));
			AddConstraint(new DistanceConstraint(br, bl, borderStiffness));
			AddConstraint(new DistanceConstraint(bl, tl, borderStiffness));

			// inside diagonals
			AddConstraint(new DistanceConstraint(tl, br, diagonalStiffness))
				.SetCollidesWithColliders(false);
			AddConstraint(new DistanceConstraint(bl, tr, diagonalStiffness))
				.SetCollidesWithColliders(false);
		}
	}
}