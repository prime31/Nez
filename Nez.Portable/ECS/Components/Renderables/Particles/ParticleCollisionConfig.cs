using Microsoft.Xna.Framework;


namespace Nez
{
	public struct ParticleCollisionConfig
	{
		/// <summary>
		/// enable/disable particle collision
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// A multiplier applied to the size of each particle before collisions are processed.
		/// </summary>
		public float RadiusScale;

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		public float Elasticity;

		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		public float Friction;

		/// <summary>
		/// control which layers this particle system collides with
		/// </summary>
		public int CollidesWithLayers;

		/// <summary>
		/// gravity value used for simulation after a collision occurs
		/// </summary>
		public Vector2 Gravity;

		/// <summary>
		/// how much a particle's lifetime is reduced after a collision. 0 is none and 1 is all of it.
		/// </summary>
		public float LifetimeLoss;

		/// <summary>
		/// kill particles whose squared speed falls below this threshold, after a collision
		/// </summary>
		public float MinKillSpeedSquared;
	}
}