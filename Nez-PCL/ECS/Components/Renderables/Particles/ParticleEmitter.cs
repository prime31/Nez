using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using System.IO;


namespace Nez.Particles
{
	public class ParticleEmitter : Nez.RenderableComponent
	{
		public override float width { get { return 5f; } }
		public override float height { get { return 5f; } }

		int particleCount;
		float emitCounter;
		float elapsedTime;


		//////////////////// Particle Emitter iVars
		bool _active = false;
		bool _emitting;
		// Stores the number of particles that are going to be rendered
		int _particleIndex;
		List<Particle> _particles = new List<Particle>();
		BlendState _blendState;
		SpriteBatch _spriteBatch;
		bool _emitOnAwake;
		ParticleEmitterConfig _emitterConfig;


		public ParticleEmitter( ParticleEmitterConfig particleEmitterConfig, bool emitOnAwake = true )
		{
			_emitterConfig = particleEmitterConfig;
			_emitOnAwake = emitOnAwake;
		}


		/// <summary>
		/// creates the SpriteBatch and loads the texture if it is available
		/// </summary>
		void initialize()
		{
			// if the spriteBatch is null we have to do our initial setup
			if( _spriteBatch == null )
			{
				_spriteBatch = new SpriteBatch( Core.graphicsDevice );
				_blendState = new BlendState();
				_blendState.ColorSourceBlend = _blendState.AlphaSourceBlend = _emitterConfig.blendFuncSource;
				_blendState.ColorDestinationBlend = _blendState.AlphaDestinationBlend = _emitterConfig.blendFuncDestination;
			}
		}


		public override void onAwake()
		{
			if( _emitOnAwake )
				emit();
		}


		public override void onRemovedFromEntity()
		{
			_spriteBatch.Dispose();
			_spriteBatch = null;
			_blendState.Dispose();
			_blendState = null;
		}


		public override void update()
		{
			// if the emitter is active and the emission rate is greater than zero then emit particles
			if( _active && _emitterConfig.emissionRate > 0 )
			{
				var rate = 1.0f / _emitterConfig.emissionRate;

				if( particleCount < _emitterConfig.maxParticles )
					emitCounter += Time.deltaTime;

				while( _emitting && particleCount < _emitterConfig.maxParticles && emitCounter > rate )
				{
					addParticle();
					emitCounter -= rate;
				}

				elapsedTime += Time.deltaTime;

				if( _emitterConfig.duration != -1 && _emitterConfig.duration < elapsedTime )
				{
					_emitting = false;

					// when we hit our duration we dont emit any more particles. once all our particles are done we stop the emitter
					if( _particleIndex == 0 )
						stopParticleEmitter();
				}
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
					if( _emitterConfig.emitterType == ParticleEmitterType.Radial )
					{
						// FIX 2
						// update the angle of the particle from the sourcePosition and the radius.  This is only done of the particles are rotating
						currentParticle.angle += currentParticle.degreesPerSecond * Time.deltaTime;
						currentParticle.radius += currentParticle.radiusDelta * Time.deltaTime;

						Vector2 tmp;
						tmp.X = _emitterConfig.sourcePosition.X - Mathf.cos( currentParticle.angle ) * currentParticle.radius;
						tmp.Y = _emitterConfig.sourcePosition.Y - Mathf.sin( currentParticle.angle ) * currentParticle.radius;
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

						tmp = radial + tangential + _emitterConfig.gravity;
						tmp = tmp * Time.deltaTime;
						currentParticle.direction = currentParticle.direction + tmp;
						tmp = currentParticle.direction * Time.deltaTime;
						currentParticle.position = currentParticle.position + tmp;

						// now apply the difference calculated early causing the particles to be relative in position to the emitter position
						currentParticle.position = currentParticle.position + positionDifference;
					}

					// update the particles color. we do the lerp from finish-to-start because timeToLive counts from particleLifespan to 0
					var t = ( currentParticle.particleLifetime - currentParticle.timeToLive ) / currentParticle.particleLifetime;
					ColorExt.lerp( ref currentParticle.startColor, ref currentParticle.finishColor, out currentParticle.color, t );

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
			_emitting = true;
			elapsedTime = 0;

			for( var i = 0; i < particleCount; i++ )
				_particles[i].timeToLive = 0;

			emitCounter = 0;
		}


		public void emit()
		{
			initialize();
			reset();
		}


		bool addParticle()
		{
			// If we have already reached the maximum number of particles then do nothing
			if( particleCount == _emitterConfig.maxParticles )
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

			// init the position of the particle. This is based on the source position of the particle emitter
			// plus a configured variance. The Random.minusOneToOne method allows the number to be both positive
			// and negative
			particle.position.X = _emitterConfig.sourcePosition.X + _emitterConfig.sourcePositionVariance.X * Random.minusOneToOne();
			particle.position.Y = _emitterConfig.sourcePosition.Y + _emitterConfig.sourcePositionVariance.Y * Random.minusOneToOne();
			particle.startPos.X = _emitterConfig.sourcePosition.X;
			particle.startPos.Y = _emitterConfig.sourcePosition.Y;

			// init the direction of the particle.  The newAngle is calculated using the angle passed in and the
			// angle variance.
			var newAngle = MathHelper.ToRadians( _emitterConfig.angle + _emitterConfig.angleVariance * Random.minusOneToOne() );

			// create a new Vector2 using the newAngle
			var vector = new Vector2( Mathf.cos( newAngle ), Mathf.sin( newAngle ) );

			// calculate the vectorSpeed using the speed and speedVariance which has been passed in
			var vectorSpeed = _emitterConfig.speed + _emitterConfig.speedVariance * Random.minusOneToOne();

			// the particles direction vector is calculated by taking the vector calculated above and
			// multiplying that by the speed
			particle.direction = vector * vectorSpeed;

			// calculate the particles life span using the life span and variance passed in
			particle.timeToLive = MathHelper.Max( 0, _emitterConfig.particleLifespan + _emitterConfig.particleLifespanVariance * Random.minusOneToOne() );
			particle.particleLifetime = particle.timeToLive;

			var startRadius = _emitterConfig.maxRadius + _emitterConfig.maxRadiusVariance * Random.minusOneToOne();
			var endRadius = _emitterConfig.minRadius + _emitterConfig.minRadiusVariance * Random.minusOneToOne();

			// set the default diameter of the particle from the source position
			particle.radius = startRadius;
			particle.radiusDelta = (endRadius - startRadius) / particle.timeToLive;
			particle.angle = MathHelper.ToRadians( _emitterConfig.angle + _emitterConfig.angleVariance * Random.minusOneToOne() );
			particle.degreesPerSecond = MathHelper.ToRadians( _emitterConfig.rotatePerSecond + _emitterConfig.rotatePerSecondVariance * Random.minusOneToOne() );

			particle.radialAcceleration = _emitterConfig.radialAcceleration + _emitterConfig.radialAccelVariance * Random.minusOneToOne();
			particle.tangentialAcceleration = _emitterConfig.tangentialAcceleration + _emitterConfig.tangentialAccelVariance * Random.minusOneToOne();

			// calculate the particle size using the start and finish particle sizes
			var particleStartSize = _emitterConfig.startParticleSize + _emitterConfig.startParticleSizeVariance * Random.minusOneToOne();
			var particleFinishSize = _emitterConfig.finishParticleSize + _emitterConfig.finishParticleSizeVariance * Random.minusOneToOne();
			particle.particleSizeDelta = ( particleFinishSize - particleStartSize ) / particle.timeToLive;
			particle.particleSize = MathHelper.Max( 0, particleStartSize );


			// calculate the color the particle should have when it starts its life. All the elements
			// of the start color passed in along with the variance are used to calculate the start color
			particle.startColor = new Color
			(
				(int)( _emitterConfig.startColor.R + _emitterConfig.startColorVariance.R * Random.minusOneToOne() ),
				(int)( _emitterConfig.startColor.G + _emitterConfig.startColorVariance.G * Random.minusOneToOne() ),
				(int)( _emitterConfig.startColor.B + _emitterConfig.startColorVariance.B * Random.minusOneToOne() ),
				(int)( _emitterConfig.startColor.A + _emitterConfig.startColorVariance.A * Random.minusOneToOne() )
			);
			particle.color = particle.startColor;

			// calculate the color the particle should be when its life is over. This is done the same
			// way as the start color above
			particle.finishColor = new Color
			(
				(int)( _emitterConfig.finishColor.R + _emitterConfig.finishColorVariance.R * Random.minusOneToOne() ),
				(int)( _emitterConfig.finishColor.G + _emitterConfig.finishColorVariance.G * Random.minusOneToOne() ),
				(int)( _emitterConfig.finishColor.B + _emitterConfig.finishColorVariance.B * Random.minusOneToOne() ),
				(int)( _emitterConfig.finishColor.A + _emitterConfig.finishColorVariance.A * Random.minusOneToOne() )
			);

			// calculate the rotation
			var startA = MathHelper.ToRadians( _emitterConfig.rotationStart + _emitterConfig.rotationStartVariance * Random.minusOneToOne() );
			var endA = MathHelper.ToRadians( _emitterConfig.rotationEnd + _emitterConfig.rotationEndVariance * Random.minusOneToOne() );
			particle.rotation = startA;
			particle.rotationDelta = ( endA - startA ) / particle.timeToLive;

			return particle;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( !_active )
				return;
			
			// reset the particle index before updating the particles in this emitter
			_particleIndex = 0;

			_spriteBatch.Begin( blendState:_blendState, samplerState:SamplerState.LinearClamp, transformMatrix:entity.scene.camera.transformMatrix );

			// loop through all the particles updating their location and color
			while( _particleIndex < particleCount )
			{
				var particle = _particles[_particleIndex];

				// TODO: should position be added to entity.position and localPosition
				if( _emitterConfig.subtexture == null )
					_spriteBatch.Draw( graphics.particleTexture, particle.position, graphics.particleTexture.sourceRect, particle.color, particle.rotation, Vector2.One, particle.particleSize * 0.5f, SpriteEffects.None, layerDepth );
				else
					_spriteBatch.Draw( _emitterConfig.subtexture, particle.position, _emitterConfig.subtexture.sourceRect, particle.color, particle.rotation, _emitterConfig.subtexture.center, particle.particleSize / _emitterConfig.subtexture.sourceRect.Width, SpriteEffects.None, layerDepth );
				
				_particleIndex++;
			}

			_spriteBatch.End();
		}

	}
}

