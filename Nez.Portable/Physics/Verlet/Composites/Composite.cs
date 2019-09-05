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
		public Vector2 Friction = new Vector2(0.98f, 1);

		/// <summary>
		/// should Particles be rendered when doing a debugRender?
		/// </summary>
		public bool DrawParticles = true;

		/// <summary>
		/// should Constraints be rendered when doing a debugRender?
		/// </summary>
		public bool DrawConstraints = true;

		/// <summary>
		/// layer mask of all the layers this Collider should collide with when Entity.move methods are used. defaults to all layers.
		/// </summary>
		public int CollidesWithLayers = Physics.AllLayers;

		public FastList<Particle> Particles = new FastList<Particle>();
		FastList<Constraint> _constraints = new FastList<Constraint>();


		#region Particle/Constraint management

		/// <summary>
		/// adds a Particle to the Composite
		/// </summary>
		/// <returns>The particle.</returns>
		/// <param name="particle">Particle.</param>
		public Particle AddParticle(Particle particle)
		{
			Particles.Add(particle);
			return particle;
		}


		/// <summary>
		/// removes the Particle from the Composite
		/// </summary>
		/// <param name="particle">Particle.</param>
		public void RemoveParticle(Particle particle)
		{
			Particles.Remove(particle);
		}


		/// <summary>
		/// removes all Particles and Constraints from the Composite
		/// </summary>
		public void RemoveAll()
		{
			Particles.Clear();
			_constraints.Clear();
		}


		/// <summary>
		/// adds a Constraint to the Composite
		/// </summary>
		/// <returns>The constraint.</returns>
		/// <param name="constraint">Constraint.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddConstraint<T>(T constraint) where T : Constraint
		{
			_constraints.Add(constraint);
			constraint.composite = this;
			return constraint;
		}


		/// <summary>
		/// removes a Constraint from the Composite
		/// </summary>
		/// <param name="constraint">Constraint.</param>
		public void RemoveConstraint(Constraint constraint)
		{
			_constraints.Remove(constraint);
		}

		#endregion


		/// <summary>
		/// applies a force to all Particles in this Composite
		/// </summary>
		/// <param name="force">Force.</param>
		public void ApplyForce(Vector2 force)
		{
			for (var j = 0; j < Particles.Length; j++)
				Particles.Buffer[j].ApplyForce(force);
		}


		/// <summary>
		/// handles solving all Constraints
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SolveConstraints()
		{
			// loop backwards in case any Constraints break and are removed
			for (var i = _constraints.Length - 1; i >= 0; i--)
				_constraints.Buffer[i].Solve();
		}


		/// <summary>
		/// applies gravity to each Particle and does the verlet integration
		/// </summary>
		/// <param name="deltaTimeSquared">Delta time.</param>
		/// <param name="gravity">Gravity.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateParticles(float deltaTimeSquared, Vector2 gravity)
		{
			for (var j = 0; j < Particles.Length; j++)
			{
				var p = Particles.Buffer[j];
				if (p.isPinned)
				{
					p.Position = p.pinnedPosition;
					continue;
				}

				p.ApplyForce(p.Mass * gravity);

				// calculate velocity and dampen it with friction
				var vel = (p.Position - p.LastPosition) * Friction;

				// calculate the next position using Verlet Integration
				var nextPos = p.Position + vel + 0.5f * p.acceleration * deltaTimeSquared;

				// reset variables
				p.LastPosition = p.Position;
				p.Position = nextPos;
				p.acceleration.X = p.acceleration.Y = 0;
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void HandleConstraintCollisions()
		{
			// loop backwards in case any Constraints break and are removed
			for (var i = _constraints.Length - 1; i >= 0; i--)
			{
				if (_constraints.Buffer[i].CollidesWithColliders)
					_constraints.Buffer[i].HandleCollisions(CollidesWithLayers);
			}
		}


		public void DebugRender(Batcher batcher)
		{
			if (DrawConstraints)
			{
				for (var i = 0; i < _constraints.Length; i++)
					_constraints.Buffer[i].DebugRender(batcher);
			}

			if (DrawParticles)
			{
				for (var i = 0; i < Particles.Length; i++)
				{
					if (Particles.Buffer[i].Radius == 0)
						batcher.DrawPixel(Particles.Buffer[i].Position, Debug.Colors.VerletParticle, 4);
					else
						batcher.DrawCircle(Particles.Buffer[i].Position, (int) Particles.Buffer[i].Radius,
							Debug.Colors.VerletParticle, 1, 4);
				}
			}
		}
	}
}