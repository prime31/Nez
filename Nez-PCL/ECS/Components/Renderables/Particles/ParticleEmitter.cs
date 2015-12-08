using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Experimental
{
	public class ParticleEmitter : Nez.RenderableComponent
	{
		public override float width { get { return 5f; } }
		public override float height { get { return 5f; } }

		/////////////////// Particle iVars
		public int emitterType;
		public Subtexture subtexture;

		public Vector2 sourcePosition, sourcePositionVariance;
		public float angle, angleVariance;
		public float speed, speedVariance;
		public float radialAcceleration, tangentialAcceleration;
		public float radialAccelVariance, tangentialAccelVariance;
		public Vector2 gravity;
		public float particleLifespan, particleLifespanVariance;
		public Color startColor, startColorVariance;
		public Color finishColor, finishColorVariance;
		public float startParticleSize, startParticleSizeVariance;
		public float finishParticleSize, finishParticleSizeVariance;
		public uint maxParticles;
		public int particleCount;
		public float emissionRate; // emissionRate = particleEmitter.maxParticles / particleEmitter.particleLifespan
		public float emitCounter;
		public float elapsedTime;
		public float duration;
		public float rotationStart, rotationStartVariance;
		public float rotationEnd, rotationEndVariance;

		//////////////////// Particle ivars only used when a maxRadius value is provided.  These values are used for
		//////////////////// the special purpose of creating the spinning portal emitter
		// Max radius at which particles are drawn when rotating
		public float maxRadius;
		// Variance of the maxRadius
		public float maxRadiusVariance;
		// The speed at which a particle moves from maxRadius to minRadius
		public float radiusSpeed;
		// Radius from source below which a particle dies
		public float minRadius;
		// Variance of the minRadius
		public float minRadiusVariance;
		// Numeber of degress to rotate a particle around the source pos per second
		public float rotatePerSecond;
		// Variance in degrees for rotatePerSecond
		public float rotatePerSecondVariance;

		public byte[] tempImageData;


		//////////////////// Particle Emitter iVars
		bool _active = true;
		// Stores the number of particles that are going to be rendered
		int _particleIndex;

		List<Particle> _particles = new List<Particle>();
		const int kEmitterTypeGravity = 0;
		const int kEmitterTypeRadial = 1;


		public override void update()
		{
			// if the emitter is active and the emission rate is greater than zero then emit particles
			if( _active && emissionRate > 0 )
			{
				var rate = 1.0f / emissionRate;

				if( particleCount < maxParticles )
					emitCounter += Time.deltaTime;

				while( particleCount < maxParticles && emitCounter > rate )
				{
					addParticle();
					emitCounter -= rate;
				}

				elapsedTime += Time.deltaTime;

				if( duration != -1 && duration < elapsedTime )
					stopParticleEmitter();
			}


			// reset the particle index before updating the particles in this emitter
			_particleIndex = 0;

			// loop through all the particles updating their location and color
			while( _particleIndex < particleCount )
			{
				// get the particle for the current particle index
				var currentParticle = _particles[_particleIndex];

				// FIX 1
				// reduce the life span of the particle
				currentParticle.timeToLive -= Time.deltaTime;

				// if the current particle is alive then update it
				if( currentParticle.timeToLive > 0 )
				{
					// if maxRadius is greater than 0 then the particles are going to spin otherwise they are effected by speed and gravity
					if( emitterType == kEmitterTypeRadial )
					{
						// FIX 2
						// update the angle of the particle from the sourcePosition and the radius.  This is only done of the particles are rotating
						currentParticle.angle += currentParticle.degreesPerSecond * Time.deltaTime;
						currentParticle.radius += currentParticle.radiusDelta * Time.deltaTime;

						Vector2 tmp;
						tmp.X = sourcePosition.X - Mathf.cos( currentParticle.angle ) * currentParticle.radius;
						tmp.Y = sourcePosition.Y - Mathf.sin( currentParticle.angle ) * currentParticle.radius;
						currentParticle.position = tmp;
					}
					else
					{
						Vector2 tmp, radial, tangential;
						radial = Vector2.Zero;

						// by default this emitters particles are moved relative to the emitter node position
						var positionDifference = currentParticle.startPos - Vector2.Zero;
						currentParticle.position = currentParticle.position - positionDifference;

						if( currentParticle.position.X != 0 || currentParticle.position.Y != 0 )
							Vector2.Normalize( ref currentParticle.position, out radial );

						tangential = radial;
						radial = radial * currentParticle.radialAcceleration;

						var newy = tangential.X;
						tangential.X = -tangential.Y;
						tangential.Y = newy;
						tangential = tangential * currentParticle.tangentialAcceleration;

						tmp = radial + tangential + gravity;
						tmp = tmp * Time.deltaTime;
						currentParticle.direction = currentParticle.direction + tmp;
						tmp = currentParticle.direction * Time.deltaTime;
						currentParticle.position = currentParticle.position + tmp;

						// now apply the difference calculated early causing the particles to be relative in position to the emitter position
						currentParticle.position = currentParticle.position + positionDifference;
					}

					// update the particles color. we do the lerp from finish-to-start because timeToLive counts from particleLifespan to 0
					ColorExt.lerp( ref currentParticle.startColor, ref currentParticle.finishColor, out currentParticle.color, currentParticle.particleLifetime - currentParticle.timeToLive );

					// update the particle size
					currentParticle.particleSize += currentParticle.particleSizeDelta * Time.deltaTime;
					currentParticle.particleSize = MathHelper.Max( 0, currentParticle.particleSize );

					// update the rotation of the particle
					currentParticle.rotation += currentParticle.rotationDelta * Time.deltaTime;

					// update the particle and vertex counters
					_particleIndex++;
				}
				else
				{
					// as the particle is not alive anymore replace it with the last active particle 
					// in the array and reduce the count of particles by one. This causes all active particles
					// to be packed together at the start of the array so that a particle which has run out of
					// life will only drop into this clause once
					if( _particleIndex != particleCount - 1 )
						_particles[_particleIndex] = _particles[particleCount - 1];

					particleCount--;
				}
			}

			_particles.RemoveRange( particleCount, _particles.Count - particleCount );
		}


		public void stopParticleEmitter()
		{
			_active = false;
			elapsedTime = 0;
			emitCounter = 0;
		}


		public void reset()
		{
			_active = true;
			elapsedTime = 0;
			for( var i = 0; i < particleCount; i++ )
				_particles[i].timeToLive = 0;

			emitCounter = 0;
			emissionRate = maxParticles / particleLifespan;
		}


		bool addParticle()
		{
			// If we have already reached the maximum number of particles then do nothing
			if( particleCount == maxParticles )
				return false;

			// Take the next particle out of the particle pool we have created and initialize it
			_particles.Add( createParticle() );

			// Increment the particle count
			particleCount++;

			// Return YES to show that a particle has been created
			return true;
		}


		Particle createParticle()
		{
			var particle = new Particle();

			// init the position of the particle.  This is based on the source position of the particle emitter
			// plus a configured variance.  The Random.minusOneToOne macro allows the number to be both positive
			// and negative
			particle.position.X = sourcePosition.X + sourcePositionVariance.X * Random.minusOneToOne();
			particle.position.Y = sourcePosition.Y + sourcePositionVariance.Y * Random.minusOneToOne();
			particle.startPos.X = sourcePosition.X;
			particle.startPos.Y = sourcePosition.Y;

			// init the direction of the particle.  The newAngle is calculated using the angle passed in and the
			// angle variance.
			var newAngle = MathHelper.ToRadians( angle + angleVariance * Random.minusOneToOne() );

			// create a new GLKVector2 using the newAngle
			var vector = new Vector2( Mathf.cos( newAngle ), Mathf.sin( newAngle ) );

			// calculate the vectorSpeed using the speed and speedVariance which has been passed in
			var vectorSpeed = speed + speedVariance * Random.minusOneToOne();

			// the particles direction vector is calculated by taking the vector calculated above and
			// multiplying that by the speed
			particle.direction = vector * vectorSpeed;

			// calculate the particles life span using the life span and variance passed in
			particle.timeToLive = MathHelper.Max( 0, particleLifespan + particleLifespanVariance * Random.minusOneToOne() );
			particle.particleLifetime = particle.timeToLive;

			var startRadius = maxRadius + maxRadiusVariance * Random.minusOneToOne();
			var endRadius = minRadius + minRadiusVariance * Random.minusOneToOne();

			// set the default diameter of the particle from the source position
			particle.radius = startRadius;
			particle.radiusDelta = (endRadius - startRadius) / particle.timeToLive;
			particle.angle = MathHelper.ToRadians( angle + angleVariance * Random.minusOneToOne() );
			particle.degreesPerSecond = MathHelper.ToRadians( rotatePerSecond + rotatePerSecondVariance * Random.minusOneToOne() );

			particle.radialAcceleration = radialAcceleration + radialAccelVariance * Random.minusOneToOne();
			particle.tangentialAcceleration = tangentialAcceleration + tangentialAccelVariance * Random.minusOneToOne();

			// calculate the particle size using the start and finish particle sizes
			var particleStartSize = startParticleSize + startParticleSizeVariance * Random.minusOneToOne();
			var particleFinishSize = finishParticleSize + finishParticleSizeVariance * Random.minusOneToOne();
			particle.particleSizeDelta = ( particleFinishSize - particleStartSize ) / particle.timeToLive;
			particle.particleSize = MathHelper.Max( 0, particleStartSize );


			// calculate the color the particle should have when it starts its life. All the elements
			// of the start color passed in along with the variance are used to calculate the start color
			particle.startColor = new Color
			(
				(int)( startColor.R + startColorVariance.R * Random.minusOneToOne() ),
				(int)( startColor.G + startColorVariance.G * Random.minusOneToOne() ),
				(int)( startColor.B + startColorVariance.B * Random.minusOneToOne() ),
				(int)( startColor.A + startColorVariance.A * Random.minusOneToOne() )
			);
			particle.color = particle.startColor;

			// calculate the color the particle should be when its life is over. This is done the same
			// way as the start color above
			particle.finishColor = new Color
			(
				(int)( finishColor.R + finishColorVariance.R * Random.minusOneToOne() ),
				(int)( finishColor.G + finishColorVariance.G * Random.minusOneToOne() ),
				(int)( finishColor.B + finishColorVariance.B * Random.minusOneToOne() ),
				(int)( finishColor.A + finishColorVariance.A * Random.minusOneToOne() )
			);

			// calculate the rotation
			var startA = rotationStart + rotationStartVariance * Random.minusOneToOne();
			var endA = rotationEnd + rotationEndVariance * Random.minusOneToOne();
			particle.rotation = startA;
			particle.rotationDelta = ( endA - startA ) / particle.timeToLive;

			return particle;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			// reset the particle index before updating the particles in this emitter
			_particleIndex = 0;

			// loop through all the particles updating their location and color
			while( _particleIndex < particleCount )
			{
				var particle = _particles[_particleIndex];

				// TODO: should position be added to entity.position and localPosition
				if( subtexture == null )
					graphics.spriteBatch.Draw( graphics.particleTexture, particle.position, graphics.particleTexture.sourceRect, particle.color, particle.rotation, Vector2.One, particle.particleSize * 0.5f, SpriteEffects.None, layerDepth );
				else
					graphics.spriteBatch.Draw( subtexture, particle.position, subtexture.sourceRect, particle.color, particle.rotation, subtexture.center, particle.particleSize / subtexture.sourceRect.Width, SpriteEffects.None, layerDepth );
				
				_particleIndex++;
			}
		}

	}
}

