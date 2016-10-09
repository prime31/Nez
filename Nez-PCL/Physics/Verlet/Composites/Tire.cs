using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	public class Tire : Composite
	{
		public Tire( Vector2 origin, float radius, int segments, float spokeStiffness = 1, float treadStiffness = 1 )
		{
			var stride = 2 * MathHelper.Pi / segments;

			// particles
			for( var i = 0; i < segments; i++ )
			{
				var theta = i * stride;
				addParticle( new Particle( new Vector2( origin.X + Mathf.cos( theta ) * radius, origin.Y + Mathf.sin( theta ) * radius ) ) );
			}

			var centerParticle = addParticle( new Particle( origin ) );

			// constraints
			for( var i = 0; i < segments; i++ )
			{
				addConstraint( new DistanceConstraint( particles[i], particles[( i + 1 ) % segments], treadStiffness ) );
				addConstraint( new DistanceConstraint( particles[i], centerParticle, spokeStiffness ) )
					.setCollidesWithColliders( false );
				addConstraint( new DistanceConstraint( particles[i], particles[( i + 5 ) % segments], treadStiffness ) );
			}
		}
	}
}
