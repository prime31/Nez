using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Particles
{
	public class ParticleEmitterConfig
	{
		public Subtexture subtexture;

		public Blend blendFuncSource;
		public Blend blendFuncDestination;

		public Vector2 sourcePosition, sourcePositionVariance;
		public float speed, speedVariance;
		public float particleLifespan, particleLifespanVariance;
		public float angle, angleVariance;
		public Vector2 gravity;
		public float radialAcceleration, radialAccelVariance;
		public float tangentialAcceleration, tangentialAccelVariance;
		public Color startColor, startColorVariance;
		public Color finishColor, finishColorVariance;
		public uint maxParticles;
		public float startParticleSize, startParticleSizeVariance;
		public float finishParticleSize, finishParticleSizeVariance;
		public float duration;
		public ParticleEmitterType emitterType;

		public float rotationStart, rotationStartVariance;
		public float rotationEnd, rotationEndVariance;
		public float emissionRate;


		//////////////////// Particle ivars only used when a maxRadius value is provided.  These values are used for
		//////////////////// the special purpose of creating the spinning portal emitter
		// Max radius at which particles are drawn when rotating
		public float maxRadius;
		// Variance of the maxRadius
		public float maxRadiusVariance;
		// Radius from source below which a particle dies
		public float minRadius;
		// Variance of the minRadius
		public float minRadiusVariance;
		// Numeber of degress to rotate a particle around the source pos per second
		public float rotatePerSecond;
		// Variance in degrees for rotatePerSecond
		public float rotatePerSecondVariance;

		//public int particleCount;


		public ParticleEmitterConfig()
		{}

	}
}

