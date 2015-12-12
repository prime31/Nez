using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public struct ParticleCollisionConfig
	{
		/// <summary>
		/// enable/disable particle collision
		/// </summary>
		public bool enabled;

		/// <summary>
		/// how much force is applied to each particle after a collision.
		/// </summary>
		public float bounce;

		/// <summary>
		/// control which layers this particle system collides with
		/// </summary>
		public int collidesWithLayers;

		/// <summary>
		/// gravity value used for simulation after a collision occurs
		/// </summary>
		public Vector2 gravity;

		/// <summary>
		/// how much speed is lost from each particle after a collision
		/// </summary>
		public float dampen;

		/// <summary>
		/// how much a particle's lifetime is reduced after a collision. 0 is none and 1 is all of it.
		/// </summary>
		public float lifetimeLoss;

		/// <summary>
		/// kill particles whose speed falls below this threshold, after a collision
		/// </summary>
		public float minKillSpeed;

		/// <summary>
		/// A multiplier applied to the size of each particle before collisions are processed.
		/// </summary>
		public float radiusScale;
	}
}

