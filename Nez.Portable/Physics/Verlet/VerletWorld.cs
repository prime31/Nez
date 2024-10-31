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
		public Vector2 Gravity = new Vector2(0, 980f);

		/// <summary>
		/// number of iterations that will be used for Constraint solving
		/// </summary>
		public int ConstraintIterations = 3;

		/// <summary>
		/// max number of iterations for the simulation as a whole
		/// </summary>
		public int MaximumStepIterations = 5;

		/// <summary>
		/// Bounds of the Verlet World. Particles will be confined to this space if set.
		/// </summary>
		public Rectangle? SimulationBounds;

		/// <summary>
		/// should Particles be allowed to be dragged?
		/// </summary>
		public bool AllowDragging = true;

		/// <summary>
		/// squared selection radius of the mouse pointer
		/// </summary>
		public float SelectionRadiusSquared = 20 * 20;

		Particle _draggedParticle;

		FastList<Composite> _composites = new FastList<Composite>();

		// collision helpers
		internal static Collider[] _colliders = new Collider[4];
		Circle _tempCircle = new Circle(1);

		// timing
		float _leftOverTime;
		float _fixedDeltaTime = 1f / 60;
		int _iterationSteps;
		float _fixedDeltaTimeSq;


		public VerletWorld(Rectangle? simulationBounds = null)
		{
			SimulationBounds = simulationBounds;
			_fixedDeltaTimeSq = Mathf.Pow(_fixedDeltaTime, 2);
		}


		#region verlet simulation

		public void Update()
		{
			UpdateTiming();

			if (AllowDragging)
				HandleDragging();

			for (var iteration = 1; iteration <= _iterationSteps; iteration++)
			{
				for (var i = _composites.Length - 1; i >= 0; i--)
				{
					var composite = _composites.Buffer[i];

					// solve constraints
					for (var s = 0; s < ConstraintIterations; s++)
						composite.SolveConstraints();

					// do the verlet integration
					composite.UpdateParticles(_fixedDeltaTimeSq, Gravity);

					// handle collisions with Nez Colliders
					composite.HandleConstraintCollisions();

					for (var j = 0; j < composite.Particles.Length; j++)
					{
						var p = composite.Particles.Buffer[j];

						// optinally constrain to bounds
						if (SimulationBounds.HasValue)
							ConstrainParticleToBounds(p);

						// optionally handle collisions with Nez Colliders
						if (p.CollidesWithColliders)
							HandleCollisions(p, composite.CollidesWithLayers);
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ConstrainParticleToBounds(Particle p)
		{
			var tempPos = p.Position;
			var bounds = SimulationBounds.Value;

			if (p.Radius == 0)
			{
				if (tempPos.Y > bounds.Height)
					tempPos.Y = bounds.Height;
				else if (tempPos.Y < bounds.Y)
					tempPos.Y = bounds.Y;

				if (tempPos.X < bounds.X)
					tempPos.X = bounds.X;
				else if (tempPos.X > bounds.Width)
					tempPos.X = bounds.Width;
			}
			else
			{
				// special care for larger particles
				if (tempPos.Y < bounds.Y + p.Radius)
					tempPos.Y = 2f * (bounds.Y + p.Radius) - tempPos.Y;
				if (tempPos.Y > bounds.Height - p.Radius)
					tempPos.Y = 2f * (bounds.Height - p.Radius) - tempPos.Y;
				if (tempPos.X > bounds.Width - p.Radius)
					tempPos.X = 2f * (bounds.Width - p.Radius) - tempPos.X;
				if (tempPos.X < bounds.X + p.Radius)
					tempPos.X = 2f * (bounds.X + p.Radius) - tempPos.X;
			}

			p.Position = tempPos;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void HandleCollisions(Particle p, int collidesWithLayers)
		{
			var collidedCount = Physics.OverlapCircleAll(p.Position, p.Radius, _colliders, collidesWithLayers);
			for (var i = 0; i < collidedCount; i++)
			{
				var collider = _colliders[i];
				if (collider.IsTrigger)
					continue;

				CollisionResult collisionResult;

				// if we have a large enough Particle radius use a Circle for the collision check else fall back to a point
				if (p.Radius < 2)
				{
					if (collider.Shape.PointCollidesWithShape(p.Position, out collisionResult))
					{
						// TODO: add a Dictionary of Collider,float that lets Colliders be setup as force volumes. The float can then be
						// multiplied by the mtv here. It should be very small values, like 0.002f for example.
						p.Position -= collisionResult.MinimumTranslationVector;
					}
				}
				else
				{
					_tempCircle.Radius = p.Radius;
					_tempCircle.Position = p.Position;

					if (_tempCircle.CollidesWithShape(collider.Shape, out collisionResult))
					{
						p.Position -= collisionResult.MinimumTranslationVector;
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void UpdateTiming()
		{
			_leftOverTime += Time.DeltaTime;
			_iterationSteps = Mathf.TruncateToInt(_leftOverTime / _fixedDeltaTime);
			_leftOverTime -= (float) _iterationSteps * _fixedDeltaTime;

			_iterationSteps = System.Math.Min(_iterationSteps, MaximumStepIterations);
		}

		#endregion


		#region Composite management

		/// <summary>
		/// adds a Composite to the simulation
		/// </summary>
		/// <returns>The composite.</returns>
		/// <param name="composite">Composite.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddComposite<T>(T composite) where T : Composite
		{
			_composites.Add(composite);
			return composite;
		}


		/// <summary>
		/// removes a Composite from the simulation
		/// </summary>
		/// <param name="composite">Composite.</param>
		public void RemoveComposite(Composite composite)
		{
			_composites.Remove(composite);
		}

		#endregion


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void HandleDragging()
		{
			if (Input.LeftMouseButtonPressed)
			{
				_draggedParticle = GetNearestParticle(Input.MousePosition);
			}
			else if (Input.LeftMouseButtonDown)
			{
				if (_draggedParticle != null)
					_draggedParticle.Position = Input.MousePosition;
			}
			else if (Input.LeftMouseButtonReleased)
			{
				if (_draggedParticle != null)
					_draggedParticle.Position = Input.MousePosition;
				_draggedParticle = null;
			}
		}


		/// <summary>
		/// gets the nearest Particle to the position. Uses the selectionRadiusSquared to determine if a Particle is near enough for consideration.
		/// </summary>
		/// <returns>The nearest particle.</returns>
		/// <param name="position">Position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Particle GetNearestParticle(Vector2 position)
		{
			// less than 64 and we count it
			var nearestSquaredDistance = SelectionRadiusSquared;
			Particle particle = null;

			// find nearest point
			for (var j = 0; j < _composites.Length; j++)
			{
				var particles = _composites.Buffer[j].Particles;
				for (var i = 0; i < particles.Length; i++)
				{
					var p = particles.Buffer[i];
					var squaredDistanceToParticle = Vector2.DistanceSquared(p.Position, position);
					if (squaredDistanceToParticle <= SelectionRadiusSquared &&
					    (particle == null || squaredDistanceToParticle < nearestSquaredDistance))
					{
						particle = p;
						nearestSquaredDistance = squaredDistanceToParticle;
					}
				}
			}

			return particle;
		}


		public void DebugRender(Batcher batcher)
		{
			for (var i = 0; i < _composites.Length; i++)
				_composites.Buffer[i].DebugRender(batcher);

			if (AllowDragging)
			{
				if (_draggedParticle != null)
				{
					batcher.DrawCircle(_draggedParticle.Position, 8, Color.White);
				}
				else
				{
					// Highlight the nearest particle within the selection radius
					var particle = GetNearestParticle(Input.MousePosition);
					if (particle != null)
						batcher.DrawCircle(particle.Position, 8, Color.White * 0.4f);
				}
			}
		}
	}
}