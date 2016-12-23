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
		/// A multiplier applied to the size of each particle before collisions are processed.
		/// </summary>
		public float radiusScale;

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		public float elasticity;

		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		public float friction;

		/// <summary>
		/// control which layers this particle system collides with
		/// </summary>
		public int collidesWithLayers;

		/// <summary>
		/// gravity value used for simulation after a collision occurs
		/// </summary>
		public Vector2 gravity;

		/// <summary>
		/// how much a particle's lifetime is reduced after a collision. 0 is none and 1 is all of it.
		/// </summary>
		public float lifetimeLoss;

		/// <summary>
		/// kill particles whose squared speed falls below this threshold, after a collision
		/// </summary>
		public float minKillSpeedSquared;
	}
}

