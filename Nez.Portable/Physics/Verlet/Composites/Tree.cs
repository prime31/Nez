using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// fractal tree. Converted from https://github.com/subprotocol/verlet-js/blob/master/examples/tree.html
	/// </summary>
	public class Tree : Composite
	{
		public Tree( Vector2 origin, int depth = 5, float branchLength = 70, float theta = 0.4f, float segmentCoef = 0.95f )
		{
			var root = addParticle( new Particle( origin ) ).pin();
			var trunk = addParticle( new Particle( origin + new Vector2( 0, 10 ) ) ).pin();

			var firstBranch = createTreeBranch( root, 0, depth, segmentCoef, new Vector2( 0, -1 ), branchLength, theta );
			addConstraint( new AngleConstraint( trunk, root, firstBranch, 3 ) );

			// animates the tree at the beginning
			var noise = 3;
			for( var i = 0; i < particles.length; ++i )
				particles.buffer[i].position += ( new Vector2( Mathf.floor( Random.nextFloat() * noise ), Mathf.floor( Random.nextFloat() * noise ) ) );
		}


		Particle createTreeBranch( Particle parent, int i, int nMax, float segmentCoef, Vector2 normal, float branchLength, float theta )
		{
			var particle = new Particle( parent.position + ( normal * ( branchLength * segmentCoef ) ) );
			addParticle( particle );

			addConstraint( new DistanceConstraint( parent, particle, 0.7f ) );

			if( i < nMax )
			{
				var aRot = Mathf.rotateAround( normal, Vector2.Zero, -theta * Mathf.rad2Deg );
				var bRot = Mathf.rotateAround( normal, Vector2.Zero, theta * Mathf.rad2Deg );
				var a = createTreeBranch( particle, i + 1, nMax, segmentCoef * segmentCoef, aRot, branchLength, theta );
				var b = createTreeBranch( particle, i + 1, nMax, segmentCoef * segmentCoef, bRot, branchLength, theta );

				var jointStrength = Mathf.lerp( 0.9f, 0, (float)i / nMax );

				addConstraint( new AngleConstraint( parent, particle, a, jointStrength ) );
				addConstraint( new AngleConstraint( parent, particle, b, jointStrength ) );
			}

			return particle;
		}

	}
}
