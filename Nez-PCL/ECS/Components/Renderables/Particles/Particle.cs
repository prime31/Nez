using System;
using Microsoft.Xna.Framework;


namespace Nez.Particles
{
	/// <summary>
	/// the internal fields are calculated at particle creation time based on the random variance. We store them because we need
	/// them later for calculating values during the particles lifetime.
	/// </summary>
	public class Particle
	{
		public Vector2 position;
		public Vector2 direction;
		public Vector2 startPos;
		public Color color;
		// stored at particle creation time and used for lerping the color
		internal Color startColor;
		// stored at particle creation time and used for lerping the color
		internal Color finishColor;
		public float rotation;
		public float rotationDelta;
		public float radialAcceleration;
		public float tangentialAcceleration;
		public float radius;
		public float radiusDelta;
		public float angle;
		public float degreesPerSecond;
		public float particleSize;
		public float particleSizeDelta;
		public float timeToLive;
		// stored at particle creation time and used for lerping the color
		internal float particleLifetime;


		public void update()
		{
			
		}

	}
}

