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

		public bool isPaused { get { return _isPaused; } }
		public bool isPlaying { get { return _active && !_isPaused; } }
		public bool isStopped { get { return !_active && !_isPaused; } }
		public float elapsedTime { get { return _elapsedTime; } }

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
		BlendState _blendState;
		SpriteBatch _spriteBatch;
		bool _playOnAwake;
		ParticleEmitterConfig _emitterConfig;


		public ParticleEmitter( ParticleEmitterConfig emitterConfig, bool playOnAwake = true )
		{
			_emitterConfig = emitterConfig;
			_playOnAwake = playOnAwake;
			_particles = new List<Particle>( (int)_emitterConfig.maxParticles );
			QuickCache<Particle>.warmCache( (int)_emitterConfig.maxParticles );

			initialize();
		}


		/// <summary>
		/// creates the SpriteBatch and loads the texture if it is available
		/// </summary>
		void initialize()
		{
			_spriteBatch = new SpriteBatch( Core.graphicsDevice );
			_blendState = new BlendState();
			_blendState.ColorSourceBlend = _blendState.AlphaSourceBlend = _emitterConfig.blendFuncSource;
			_blendState.ColorDestinationBlend = _blendState.AlphaDestinationBlend = _emitterConfig.blendFuncDestination;
		}

		#region Component/RenderableComponent

		public override void onAwake()
		{
			if( _playOnAwake )
				play();
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
			if( _isPaused )
				return;
			
			// if the emitter is active and the emission rate is greater than zero then emit particles
			if( _active && _emitterConfig.emissionRate > 0 )
			{
				var rate = 1.0f / _emitterConfig.emissionRate;

				if( _particles.Count < _emitterConfig.maxParticles )
					_emitCounter += Time.deltaTime;

				while( _emitting && _particles.Count < _emitterConfig.maxParticles && _emitCounter > rate )
				{
					addParticle();
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
				if( currentParticle.update( _emitterConfig ) )
				{
					QuickCache<Particle>.push( currentParticle );
					_particles.RemoveAt( i );
				}
			}
		}


		public override void render( Graphics graphics, Camera camera )
		{
			// we still render when we are paused
			if( !_active && !_isPaused )
				return;

			var rootPosition = renderPosition;
			_spriteBatch.Begin( blendState:_blendState, samplerState:SamplerState.LinearClamp, transformMatrix:entity.scene.camera.transformMatrix );

			// loop through all the particles updating their location and color
			for( var i = 0; i < _particles.Count; i++ )
			{
				var currentParticle = _particles[i];

				if( _emitterConfig.subtexture == null )
					_spriteBatch.Draw( graphics.particleTexture, rootPosition + currentParticle.position, graphics.particleTexture.sourceRect, currentParticle.color, currentParticle.rotation, Vector2.One, currentParticle.particleSize * 0.5f, SpriteEffects.None, layerDepth );
				else
					_spriteBatch.Draw( _emitterConfig.subtexture, rootPosition + currentParticle.position, _emitterConfig.subtexture.sourceRect, currentParticle.color, currentParticle.rotation, _emitterConfig.subtexture.center, currentParticle.particleSize / _emitterConfig.subtexture.sourceRect.Width, SpriteEffects.None, layerDepth );
			}

			_spriteBatch.End();
		}

		#endregion


		/// <summary>
		/// removes all particles from the particle emitter
		/// </summary>
		public void clear()
		{
			for( var i = 0; i < _particles.Count; i++ )
				QuickCache<Particle>.push( _particles[i] );
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
			initialize();
			_active = true;
			for( var i = 0; i < count; i++ )
				addParticle();
		}


		/// <summary>
		/// adds a Particle to the emitter
		/// </summary>
		void addParticle()
		{
			// take the next particle out of the particle pool we have created and initialize it
			var particle = QuickCache<Particle>.pop();
			particle.initialize( _emitterConfig );
			_particles.Add( particle );
		}

	}
}

