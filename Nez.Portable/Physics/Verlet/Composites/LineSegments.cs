using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// a series of points connected with DistanceConstraints
	/// </summary>
	public class LineSegments : Composite
	{
		public LineSegments( Vector2[] vertices, float stiffness )
		{
			for( var i = 0; i < vertices.Length; i++ )
			{
				var p = new Particle( vertices[i] );
				addParticle( p );

				if( i > 0 )
					addConstraint( new DistanceConstraint( particles.buffer[i], particles.buffer[i - 1], stiffness ) );
			}
		}


		/// <summary>
		/// pins the Particle at the given index
		/// </summary>
		/// <param name="index">Index.</param>
		public LineSegments pinParticleAtIndex( int index )
		{
			particles.buffer[index].pin();
			return this;
		}

	}
}
