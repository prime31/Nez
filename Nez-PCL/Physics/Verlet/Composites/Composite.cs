using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// represents an object in the Verlet world. Consists of Particles and Constraints and handles updating them
	/// </summary>
	public class Composite
	{
		/// <summary>
		/// friction applied to all Particle movement to dampen it. Value should be very close to 1.
		/// </summary>
		public Vector2 friction = new Vector2( 0.98f, 1 );

		/// <summary>
		/// should Particles be rendered when doing a debugRender?
		/// </summary>
		public bool drawParticles = true;

		/// <summary>
		/// should Constraints be rendered when doing a debugRender?
		/// </summary>
		public bool drawConstraints = true;

		/// <summary>
		/// layer mask of all the layers this Collider should collide with when Entity.move methods are used. defaults to all layers.
		/// </summary>
		public int collidesWithLayers = Physics.allLayers;

		public FastList<Particle> particles = new FastList<Particle>();
		FastList<Constraint> _constraints = new FastList<Constraint>();


		#region Particle/Constraint management

		/// <summary>
		/// adds a Particle to the Composite
		/// </summary>
		/// <returns>The particle.</returns>
		/// <param name="particle">Particle.</param>
		public Particle addParticle( Particle particle )
		{
			particles.add( particle );
			return particle;
		}


		/// <summary>
		/// removes the Particle from the Composite
		/// </summary>
		/// <param name="particle">Particle.</param>
		public void removeParticle( Particle particle )
		{
			particles.remove( particle );
		}


		/// <summary>
		/// removes all Particles and Constraints from the Composite
		/// </summary>
		public void removeAll()
		{
			particles.clear();
			_constraints.clear();
		}


		/// <summary>
		/// adds a Constraint to the Composite
		/// </summary>
		/// <returns>The constraint.</returns>
		/// <param name="constraint">Constraint.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T addConstraint<T>( T constraint ) where T : Constraint
		{
			_constraints.add( constraint );
			constraint.composite = this;
			return constraint;
		}


		/// <summary>
		/// removes a Constraint from the Composite
		/// </summary>
		/// <param name="constraint">Constraint.</param>
		public void removeConstraint( Constraint constraint )
		{
			_constraints.remove( constraint );
		}

		#endregion


		/// <summary>
		/// applies a force to all Particles in this Composite
		/// </summary>
		/// <param name="force">Force.</param>
		public void applyForce( Vector2 force )
		{
			for( var j = 0; j < particles.length; j++ )
				particles.buffer[j].applyForce( force );
		}


		/// <summary>
		/// handles solving all Constraints
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void solveConstraints()
		{
			// loop backwards in case any Constraints break and are removed
			for( var i = _constraints.length - 1; i >= 0; i-- )
				_constraints.buffer[i].solve();
		}


		/// <summary>
		/// applies gravity to each Particle and does the verlet integration
		/// </summary>
		/// <param name="deltaTimeSquared">Delta time.</param>
		/// <param name="gravity">Gravity.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void updateParticles( float deltaTimeSquared, Vector2 gravity )
		{
			for( var j = 0; j < particles.length; j++ )
			{
				var p = particles.buffer[j];
				if( p.isPinned )
				{
					p.position = p.pinnedPosition;
					continue;
				}

				p.applyForce( p.mass * gravity );

				// calculate velocity and dampen it with friction
				var vel = ( p.position - p.lastPosition ) * friction;

				// calculate the next position using Verlet Integration
				var nextPos = p.position + vel + 0.5f * p.acceleration * deltaTimeSquared;

				// reset variables
				p.lastPosition = p.position;
				p.position = nextPos;
				p.acceleration.X = p.acceleration.Y = 0;
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void handleConstraintCollisions()
		{
			// loop backwards in case any Constraints break and are removed
			for( var i = _constraints.length - 1; i >= 0; i-- )
			{
				if( _constraints.buffer[i].collidesWithColliders )
					_constraints.buffer[i].handleCollisions( collidesWithLayers );
			}
		}


		public void debugRender( Batcher batcher )
		{
			if( drawConstraints )
			{
				for( var i = 0; i < _constraints.length; i++ )
					_constraints.buffer[i].debugRender( batcher );
			}

			if( drawParticles )
			{
				for( var i = 0; i < particles.length; i++ )
				{
					if( particles.buffer[i].radius == 0 )
						batcher.drawPixel( particles.buffer[i].position, DefaultColors.verletParticle, 4 );
					else
						batcher.drawCircle( particles.buffer[i].position, (int)particles.buffer[i].radius, DefaultColors.verletParticle, 1, 4 );
				}
			}
		}

	}
}
