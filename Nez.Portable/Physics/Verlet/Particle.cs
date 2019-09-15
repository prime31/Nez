using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	public class Particle
	{
		/// <summary>
		/// the current position of the Particle
		/// </summary>
		public Vector2 Position;

		/// <summary>
		/// the position of the Particle prior to its latest move
		/// </summary>
		public Vector2 LastPosition;

		/// <summary>
		/// the mass of the Particle. Taken into account for all forces and constraints
		/// </summary>
		public float Mass = 1;

		/// <summary>
		/// the radius of the Particle
		/// </summary>
		public float Radius;

		/// <summary>
		/// if true, the Particle will collide with standard Nez Colliders
		/// </summary>
		public bool CollidesWithColliders = true;

		internal bool isPinned;
		internal Vector2 acceleration;
		internal Vector2 pinnedPosition;


		public Particle(Vector2 position)
		{
			Position = position;
			LastPosition = position;
		}


		public Particle(float x, float y) : this(new Vector2(x, y))
		{
		}


		/// <summary>
		/// applies a force taking mass into account to the Particle
		/// </summary>
		/// <param name="force">Force.</param>
		public void ApplyForce(Vector2 force)
		{
			// acceleration = (1 / mass) * force
			acceleration += force / Mass;
		}


		/// <summary>
		/// pins the Particle to its current position
		/// </summary>
		public Particle Pin()
		{
			isPinned = true;
			pinnedPosition = Position;
			return this;
		}


		/// <summary>
		/// pins the particle to the specified position
		/// </summary>
		/// <param name="position">Position.</param>
		public Particle PinTo(Vector2 position)
		{
			isPinned = true;
			pinnedPosition = position;
			Position = pinnedPosition;
			return this;
		}


		/// <summary>
		/// unpins the particle setting it free like the wind
		/// </summary>
		public Particle Unpin()
		{
			isPinned = false;
			return this;
		}
	}
}