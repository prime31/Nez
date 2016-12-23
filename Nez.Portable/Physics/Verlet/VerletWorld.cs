using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Verlet
{
	/// <summary>
	/// the root of the Verlet simulation. Create a World and call its update method each frame.
	/// </summary>
	public class VerletWorld
	{
		/// <summary>
		/// gravity for the simulation
		/// </summary>
		public Vector2 gravity = new Vector2( 0, 980f );

		/// <summary>
		/// number of iterations that will be used for Constraint solving
		/// </summary>
		public int constraintIterations = 3;

		/// <summary>
		/// max number of iterations for the simulation as a whole
		/// </summary>
		public int maximumStepIterations = 5;

		/// <summary>
		/// Bounds of the Verlet World. Particles will be confined to this space if set.
		/// </summary>
		public Rectangle? simulationBounds;

		/// <summary>
		/// should Particles be allowed to be dragged?
		/// </summary>
		public bool allowDragging = true;

		/// <summary>
		/// squared selection radius of the mouse pointer
		/// </summary>
		public float selectionRadiusSquared = 20 * 20;

		Particle _draggedParticle;

		FastList<Composite> _composites = new FastList<Composite>();

		// collision helpers
		internal static Collider[] _colliders = new Collider[4];
		Circle _tempCircle = new Circle( 1 );

		// timing
		float _leftOverTime;
		float _fixedDeltaTime = 1f / 60;
		int _iterationSteps;
		float _fixedDeltaTimeSq;


		public VerletWorld( Rectangle? simulationBounds = null )
		{
			this.simulationBounds = simulationBounds;
			_fixedDeltaTimeSq = Mathf.pow( _fixedDeltaTime, 2 );
		}


		#region verlet simulation

		public void update()
		{
			updateTiming();

			if( allowDragging )
				handleDragging();

			for( var iteration = 1; iteration <= _iterationSteps; iteration++ )
			{
				for( var i = _composites.length - 1; i >= 0; i-- )
				{
					var composite = _composites.buffer[i];

					// solve constraints
					for( var s = 0; s < constraintIterations; s++ )
						composite.solveConstraints();

					// do the verlet integration
					composite.updateParticles( _fixedDeltaTimeSq, gravity );

					// handle collisions with Nez Colliders
					composite.handleConstraintCollisions();

					for( var j = 0; j < composite.particles.length; j++ )
					{
						var p = composite.particles.buffer[j];

						// optinally constrain to bounds
						if( simulationBounds.HasValue )
							constrainParticleToBounds( p );

						// optionally handle collisions with Nez Colliders
						if( p.collidesWithColliders )
							handleCollisions( p, composite.collidesWithLayers );
					}
				}
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void constrainParticleToBounds( Particle p )
		{
			var tempPos = p.position;
			var bounds = simulationBounds.Value;

			if( p.radius == 0 )
			{
				if( tempPos.Y > bounds.Height )
					tempPos.Y = bounds.Height;
				else if( tempPos.Y < bounds.Y )
					tempPos.Y = bounds.Y;

				if( tempPos.X < bounds.X )
					tempPos.X = bounds.X;
				else if( tempPos.X > bounds.Width )
					tempPos.X = bounds.Width;
			}
			else
			{
				// special care for larger particles
				if( tempPos.Y < bounds.Y + p.radius )
					tempPos.Y = 2f * ( bounds.Y + p.radius ) - tempPos.Y;
				if( tempPos.Y > bounds.Height - p.radius )
					tempPos.Y = 2f * ( bounds.Height - p.radius ) - tempPos.Y;
				if( tempPos.X > bounds.Width - p.radius )
					tempPos.X = 2f * ( bounds.Width - p.radius ) - tempPos.X;
				if( tempPos.X < bounds.X + p.radius )
					tempPos.X = 2f * ( bounds.X + p.radius ) - tempPos.X;
			}

			p.position = tempPos;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void handleCollisions( Particle p, int collidesWithLayers )
		{
			var collidedCount = Physics.overlapCircleAll( p.position, p.radius, _colliders, collidesWithLayers );
			for( var i = 0; i < collidedCount; i++ )
			{
				var collider = _colliders[i];
				if( collider.isTrigger )
					continue;
				
				CollisionResult collisionResult;

				// if we have a large enough Particle radius use a Circle for the collision check else fall back to a point
				if( p.radius < 2 )
				{
					if( collider.shape.pointCollidesWithShape( p.position, out collisionResult ) )
					{
						// TODO: add a Dictionary of Collider,float that lets Colliders be setup as force volumes. The float can then be
						// multiplied by the mtv here. It should be very small values, like 0.002f for example.
						p.position -= collisionResult.minimumTranslationVector;
					}
				}
				else
				{
					_tempCircle.radius = p.radius;
					_tempCircle.position = p.position;

					if( _tempCircle.collidesWithShape( collider.shape, out collisionResult ) )
					{
						p.position -= collisionResult.minimumTranslationVector;
					}
				}
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void updateTiming()
		{
			_leftOverTime += Time.deltaTime;
			_iterationSteps = Mathf.truncateToInt( _leftOverTime / _fixedDeltaTime );
			_leftOverTime -= (float)_iterationSteps * _fixedDeltaTime;

			_iterationSteps = System.Math.Min( _iterationSteps, maximumStepIterations );
		}

		#endregion


		#region Composite management

		/// <summary>
		/// adds a Composite to the simulation
		/// </summary>
		/// <returns>The composite.</returns>
		/// <param name="composite">Composite.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T addComposite<T>( T composite ) where T : Composite
		{
			_composites.add( composite );
			return composite;
		}


		/// <summary>
		/// removes a Composite from the simulation
		/// </summary>
		/// <param name="composite">Composite.</param>
		public void removeComposite( Composite composite )
		{
			_composites.remove( composite );
		}

		#endregion


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void handleDragging()
		{
			if( Input.leftMouseButtonPressed )
			{
				_draggedParticle = getNearestParticle( Input.mousePosition );
			}
			else if( Input.leftMouseButtonDown )
			{
				if( _draggedParticle != null )
					_draggedParticle.position = Input.mousePosition;
			}
			else if( Input.leftMouseButtonReleased )
			{
				if( _draggedParticle != null )
					_draggedParticle.position = Input.mousePosition;
				_draggedParticle = null;
			}
		}


		/// <summary>
		/// gets the nearest Particle to the position. Uses the selectionRadiusSquared to determine if a Particle is near enough for consideration.
		/// </summary>
		/// <returns>The nearest particle.</returns>
		/// <param name="position">Position.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Particle getNearestParticle( Vector2 position )
		{
			// less than 64 and we count it
			var nearestSquaredDistance = selectionRadiusSquared;
			Particle particle = null;

			// find nearest point
			for( var j = 0; j < _composites.length; j++ )
			{
				var particles = _composites.buffer[j].particles;
				for( var i = 0; i < particles.length; i++ )
				{
					var p = particles.buffer[i];
					var squaredDistanceToParticle = Vector2.DistanceSquared( p.position, position );
					if( squaredDistanceToParticle <= selectionRadiusSquared && ( particle == null || squaredDistanceToParticle < nearestSquaredDistance ) )
					{
						particle = p;
						nearestSquaredDistance = squaredDistanceToParticle;
					}
				}
			}

			return particle;
		}


		public void debugRender( Batcher batcher )
		{
			for( var i = 0; i < _composites.length; i++ )
				_composites.buffer[i].debugRender( batcher );

			if( allowDragging )
			{
				if( _draggedParticle != null )
				{
					batcher.drawCircle( _draggedParticle.position, 8, Color.White );
				}
				else
				{
					// Highlight the nearest particle within the selection radius
					var particle = getNearestParticle( Input.mousePosition );
					if( particle != null )
						batcher.drawCircle( particle.position, 8, Color.White * 0.4f );
				}
			}
		}

	}
}
