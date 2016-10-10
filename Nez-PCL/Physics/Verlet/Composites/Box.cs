using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// creates a simple box with diagonal contraints to keep it rigid
	/// </summary>
	public class Box : Composite
	{
		public Box( Vector2 center, float width, float height, float borderStiffness = 0.2f, float diagonalStiffness = 0.5f )
		{
			var tl = addParticle( new Particle( center + new Vector2( -width / 2, -height / 2 ) ) );
			var tr = addParticle( new Particle( center + new Vector2( width / 2, -height / 2 ) ) );
			var br = addParticle( new Particle( center + new Vector2( width / 2, height / 2 ) ) );
			var bl = addParticle( new Particle( center + new Vector2( -width / 2, height / 2 ) ) );

			// outside edges
			addConstraint( new DistanceConstraint( tl, tr, borderStiffness ) );
			addConstraint( new DistanceConstraint( tr, br, borderStiffness ) );
			addConstraint( new DistanceConstraint( br, bl, borderStiffness ) );
			addConstraint( new DistanceConstraint( bl, tl, borderStiffness ) );

			// inside diagonals
			addConstraint( new DistanceConstraint( tl, br, diagonalStiffness ) )
				.setCollidesWithColliders( false );
			addConstraint( new DistanceConstraint( bl, tr, diagonalStiffness ) )
				.setCollidesWithColliders( false );
		}
	}
}
