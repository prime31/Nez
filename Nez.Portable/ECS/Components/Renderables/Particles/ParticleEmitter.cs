using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using System.IO;


namespace Nez.Particles
{
	public class ParticleEmitter : RenderableComponent, IUpdatable
	{
		public override float width { get { return 5f; } }
		public override float height { get { return 5f; } }

		public bool isPaused { get { return _isPaused; } }
		public bool isPlaying { get { return _active && !_isPaused; } }
		public bool isStopped { get { return !_active && !_isPaused; } }
		public float elapsedTime { get { return _elapsedTime; } }

		/// <summary>
		/// convenience method for setting ParticleEmitterConfig.simulateInWorldSpace. If true, particles will simulate in world space. ie when the
		/// parent Transform moves it will have no effect on any already active Particles.
		/// </summary>
		public bool simulateInWorldSpace { set { _emitterConfig.simulateInWorldSpace = value; } }

		/// <summary>
		/// config object with various properties to deal with particle collisions
		/// </summary>
		public ParticleCollisionConfig collisionConfig;

		/// <summary>
		/// keeps track of how many particles should be emitted
		/// </summary>
		float _emitCounter;

		/// <summary>
		/// tracks the elapsed time of the emitter
		/// </summary>
		float _elapsedTime;

		bool _active = false;
		bool _isPaused;

		/// <summary>
		/// if the emitter is emitting this will be true. Note that emitting can be false while particles are still alive. emitting gets set
		/// to false and then any live particles are allowed to finish their lifecycle.
		/// </summary>
		bool _emitting;
		List<Particle> _particles;
		bool _playOnAwake;
		ParticleEmitterConfig _emitterConfig;


		public ParticleEmitter( ParticleEmitterConfig emitterConfig, bool playOnAwake = true )
		{
			_emitterConfig = emitterConfig;
			_playOnAwake = playOnAwake;
			_particles = new List<Particle>( (int)_emitterConfig.maxParticles );
			Pool<Particle>.warmCache( (int)_emitterConfig.maxParticles );

			// set some sensible defaults
			collisionConfig.elasticity = 0.5f;
			collisionConfig.friction = 0.5f;
			collisionConfig.collidesWithLayers = Physics.allLayers;
			collisionConfig.gravity = _emitterConfig.gravity;
			collisionConfig.lifetimeLoss = 0f;
			collisionConfig.minKillSpeedSquared = float.MinValue;
			collisionConfig.radiusScale = 0.8f;

			initialize();
		}


		/// <summary>
		/// creates the Batcher and loads the texture if it is available
		/// </summary>
		void initialize()
		{
			// prep our custom BlendState and set the Material with it
			var blendState = new BlendState();
			blendState.ColorSourceBlend = blendState.AlphaSourceBlend = _emitterConfig.blendFuncSource;
			blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend = _emitterConfig.blendFuncDestination;

			material = new Material( blendState );
		}


		#region Component/RenderableComponent

		public override void onAddedToEntity()
		{
			if( _playOnAwake )
				play();
		}


		void IUpdatable.update()
		{
			if( _isPaused )
				return;

			// prep data for the particle.update method
			var rootPosition = entity.transform.position + _localOffset;
			
			// if the emitter is active and the emission rate is greater than zero then emit particles
			if( _active && _emitterConfig.emissionRate > 0 )
			{
				var rate = 1.0f / _emitterConfig.emissionRate;

				if( _particles.Count < _emitterConfig.maxParticles )
					_emitCounter += Time.deltaTime;

				while( _emitting && _particles.Count < _emitterConfig.maxParticles && _emitCounter > rate )
				{
					addParticle( rootPosition );
					_emitCounter -= rate;
				}

				_elapsedTime += Time.deltaTime;

				if( _emitterConfig.duration != -1 && _emitterConfig.duration < _elapsedTime )
				{
					// when we hit our duration we dont emit any more particles
					_emitting = false;

					// once all our particles are done we stop the emitter
					if( _particles.Count == 0 )
						stop();
				}
			}

			// loop through all the particles updating their location and color
			for( var i = _particles.Count - 1; i >= 0; i-- )
			{
				// get the current particle and update it
				var currentParticle = _particles[i];

				// if update returns true that means the particle is done
				if( currentParticle.update( _emitterConfig, ref collisionConfig, rootPosition ) )
				{
					Pool<Particle>.free( currentParticle );
					_particles.RemoveAt( i );
				}
			}
		}


		public override void render( Graphics graphics, Camera camera )
		{
			// we still render when we are paused
			if( !_active && !_isPaused )
				return;

			var rootPosition = entity.transform.position + _localOffset;

			// loop through all the particles updating their location and color
			for( var i = 0; i < _particles.Count; i++ )
			{
				var currentParticle = _particles[i];
				var pos = _emitterConfig.simulateInWorldSpace ? currentParticle.spawnPosition : rootPosition;

				if( _emitterConfig.subtexture == null )
					graphics.batcher.draw( graphics.pixelTexture, pos + currentParticle.position, graphics.pixelTexture.sourceRect, currentParticle.color, currentParticle.rotation, Vector2.One, currentParticle.particleSize * 0.5f, SpriteEffects.None, layerDepth );
				else
					graphics.batcher.draw( _emitterConfig.subtexture, pos + currentParticle.position, _emitterConfig.subtexture.sourceRect, currentParticle.color, currentParticle.rotation, _emitterConfig.subtexture.center, currentParticle.particleSize / _emitterConfig.subtexture.sourceRect.Width, SpriteEffects.None, layerDepth );
			}
		}


		public override void debugRender( Graphics graphics )
		{
			// we still render when we are paused
			if( !_active && !_isPaused )
				return;

			var rootPosition = entity.transform.position + _localOffset;

			// loop through all the particles updating their location and color
			for( var i = 0; i < _particles.Count; i++ )
			{
				var currentParticle = _particles[i];
				var pos = _emitterConfig.simulateInWorldSpace ? currentParticle.spawnPosition : rootPosition;

				graphics.batcher.drawHollowRect( pos + currentParticle.position - new Vector2( currentParticle.particleSize * 0.5f, currentParticle.particleSize * 0.5f ), currentParticle.particleSize, currentParticle.particleSize, Color.IndianRed );
			}
		}

		#endregion


		/// <summary>
		/// removes all particles from the particle emitter
		/// </summary>
		public void clear()
		{
			for( var i = 0; i < _particles.Count; i++ )
				Pool<Particle>.free( _particles[i] );
			_particles.Clear();
		}


		/// <summary>
		/// plays the particle emitter
		/// </summary>
		public void play()
		{
			// if we are just unpausing, we only toggle flags and we dont mess with any other parameters
			if( _isPaused )
			{
				_active = true;
				_isPaused = false;
				return;
			}

			_active = true;
			_emitting = true;
			_elapsedTime = 0;
			_emitCounter = 0;
		}


		/// <summary>
		/// stops the particle emitter
		/// </summary>
		public void stop()
		{
			_active = false;
			_isPaused = false;
			_elapsedTime = 0;
			_emitCounter = 0;
			clear();
		}


		/// <summary>
		/// pauses the particle emitter
		/// </summary>
		public void pause()
		{
			_isPaused = true;
			_active = false;
		}


		/// <summary>
		/// manually emit some particles
		/// </summary>
		/// <param name="count">Count.</param>
		public void emit( int count )
		{
			var rootPosition = entity.transform.position + _localOffset;

			initialize();
			_active = true;
			for( var i = 0; i < count; i++ )
				addParticle( rootPosition );
		}


		/// <summary>
		/// adds a Particle to the emitter
		/// </summary>
		void addParticle( Vector2 position )
		{
			// take the next particle out of the particle pool we have created and initialize it
			var particle = Pool<Particle>.obtain();
			particle.initialize( _emitterConfig, position );
			_particles.Add( particle );
		}

	}
}

