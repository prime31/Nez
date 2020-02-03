using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// fractal tree. Converted from https://github.com/subprotocol/verlet-js/blob/master/examples/tree.html
	/// </summary>
	public class Tree : Composite
	{
		public Tree(Vector2 origin, int depth = 5, float branchLength = 70, float theta = 0.4f,
		            float segmentCoef = 0.95f)
		{
			var root = AddParticle(new Particle(origin)).Pin();
			var trunk = AddParticle(new Particle(origin + new Vector2(0, 10))).Pin();

			var firstBranch = CreateTreeBranch(root, 0, depth, segmentCoef, new Vector2(0, -1), branchLength, theta);
			AddConstraint(new AngleConstraint(trunk, root, firstBranch, 3));

			// animates the tree at the beginning
			var noise = 3;
			for (var i = 0; i < Particles.Length; ++i)
				Particles.Buffer[i].Position += (new Vector2(Mathf.Floor(Random.NextFloat() * noise),
					Mathf.Floor(Random.NextFloat() * noise)));
		}


		Particle CreateTreeBranch(Particle parent, int i, int nMax, float segmentCoef, Vector2 normal,
		                          float branchLength, float theta)
		{
			var particle = new Particle(parent.Position + (normal * (branchLength * segmentCoef)));
			AddParticle(particle);

			AddConstraint(new DistanceConstraint(parent, particle, 0.7f));

			if (i < nMax)
			{
				var aRot = Mathf.RotateAround(normal, Vector2.Zero, -theta * Mathf.Rad2Deg);
				var bRot = Mathf.RotateAround(normal, Vector2.Zero, theta * Mathf.Rad2Deg);
				var a = CreateTreeBranch(particle, i + 1, nMax, segmentCoef * segmentCoef, aRot, branchLength, theta);
				var b = CreateTreeBranch(particle, i + 1, nMax, segmentCoef * segmentCoef, bRot, branchLength, theta);

				var jointStrength = Mathf.Lerp(0.9f, 0, (float) i / nMax);

				AddConstraint(new AngleConstraint(parent, particle, a, jointStrength));
				AddConstraint(new AngleConstraint(parent, particle, b, jointStrength));
			}

			return particle;
		}
	}
}