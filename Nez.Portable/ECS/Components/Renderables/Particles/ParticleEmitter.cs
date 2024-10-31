using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace Nez.Particles
{
	public class ParticleEmitter : RenderableComponent, IUpdatable
	{
		public override RectangleF Bounds => _bounds;

		public bool IsPaused => _isPaused;
		public bool IsPlaying => _active && !_isPaused;
		public bool IsStopped => !_active && !_isPaused;
		public bool IsEmitting => _emitting;
		public float ElapsedTime => _elapsedTime;


		/// <summary>
		/// convenience method for setting ParticleEmitterConfig.simulateInWorldSpace. If true, particles will simulate in world space. ie when the
		/// parent Transform moves it will have no effect on any already active Particles.
		/// </summary>
		public bool SimulateInWorldSpace
		{
			set => _emitterConfig.SimulateInWorldSpace = value;
		}

		/// <summary>
		/// config object with various properties to deal with particle collisions
		/// </summary>
		public ParticleCollisionConfig CollisionConfig;

		/// <summary>
		/// event that's going to be called when particles count becomes 0 after stopping emission.
		/// emission can stop after either we stop it manually or when we run for entire duration specified in ParticleEmitterConfig.
		/// </summary>
		public event Action<ParticleEmitter> OnAllParticlesExpired;

		/// <summary>
		/// event that's going to be called when emission is stopped due to reaching duration specified in ParticleEmitterConfig
		/// </summary>
		public event Action<ParticleEmitter> OnEmissionDurationReached;

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
		[Inspectable] ParticleEmitterConfig _emitterConfig;


		public ParticleEmitter() : this(new ParticleEmitterConfig())
		{
		}

		public ParticleEmitter(ParticleEmitterConfig emitterConfig, bool playOnAwake = true)
		{
			_emitterConfig = emitterConfig;
			_playOnAwake = playOnAwake;
			_particles = new List<Particle>((int) _emitterConfig.MaxParticles);
			Pool<Particle>.WarmCache((int) _emitterConfig.MaxParticles);

			// set some sensible defaults
			CollisionConfig.Elasticity = 0.5f;
			CollisionConfig.Friction = 0.5f;
			CollisionConfig.CollidesWithLayers = Physics.AllLayers;
			CollisionConfig.Gravity = _emitterConfig.Gravity;
			CollisionConfig.LifetimeLoss = 0f;
			CollisionConfig.MinKillSpeedSquared = float.MinValue;
			CollisionConfig.RadiusScale = 0.8f;

			Init();
		}


		/// <summary>
		/// creates the Batcher and loads the texture if it is available
		/// </summary>
		void Init()
		{
			// prep our custom BlendState and set the Material with it
			var blendState = new BlendState();
			blendState.ColorSourceBlend = blendState.AlphaSourceBlend = _emitterConfig.BlendFuncSource;
			blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend = _emitterConfig.BlendFuncDestination;

			Material = new Material(blendState);
		}


		#region Component/RenderableComponent

		public override void OnAddedToEntity()
		{
			if (_playOnAwake)
				Play();
		}


		public virtual void Update()
		{
			if (_isPaused)
				return;

			// prep data for the particle.update method
			var rootPosition = Entity.Transform.Position + _localOffset;

			// if the emitter is active and the emission rate is greater than zero then emit particles
			if (_active && _emitterConfig.EmissionRate > 0)
			{
				if (_emitting)
				{
					var rate = 1.0f / _emitterConfig.EmissionRate;

					if (_particles.Count < _emitterConfig.MaxParticles)
						_emitCounter += Time.DeltaTime;

					while (_particles.Count < _emitterConfig.MaxParticles && _emitCounter > rate)
					{
						AddParticle(rootPosition);
						_emitCounter -= rate;
					}

					_elapsedTime += Time.DeltaTime;

					if (_emitterConfig.Duration != -1 && _emitterConfig.Duration < _elapsedTime)
					{
						// when we hit our duration we dont emit any more particles
						_emitting = false;

						if (OnEmissionDurationReached != null)
							OnEmissionDurationReached(this);
					}
				}

				// once all our particles are done we stop the emitter
				if (!_emitting && _particles.Count == 0)
				{
					Stop();

					if (OnAllParticlesExpired != null)
						OnAllParticlesExpired(this);
				}
			}

			var min = new Vector2(float.MaxValue, float.MaxValue);
			var max = new Vector2(float.MinValue, float.MinValue);
			var maxParticleSize = float.MinValue;

			// loop through all the particles updating their location and color
			for (var i = _particles.Count - 1; i >= 0; i--)
			{
				// get the current particle and update it
				var currentParticle = _particles[i];

				// if update returns true that means the particle is done
				if (currentParticle.Update(_emitterConfig, ref CollisionConfig, rootPosition))
				{
					Pool<Particle>.Free(currentParticle);
					_particles.RemoveAt(i);
				}
				else
				{
					// particle is good. collect min/max positions for the bounds
					var pos = _emitterConfig.SimulateInWorldSpace ? currentParticle.SpawnPosition : rootPosition;
					pos += currentParticle.Position;
					Vector2.Min(ref min, ref pos, out min);
					Vector2.Max(ref max, ref pos, out max);
					maxParticleSize = Math.Max(maxParticleSize, currentParticle.ParticleSize);
				}
			}

			_bounds.Location = min;
			_bounds.Width = max.X - min.X;
			_bounds.Height = max.Y - min.Y;

			if (_emitterConfig.Sprite == null)
			{
				_bounds.Inflate(1 * maxParticleSize, 1 * maxParticleSize);
			}
			else
			{
				maxParticleSize /= _emitterConfig.Sprite.SourceRect.Width;
				_bounds.Inflate(_emitterConfig.Sprite.SourceRect.Width * maxParticleSize,
					_emitterConfig.Sprite.SourceRect.Height * maxParticleSize);
			}
		}


		public override void Render(Batcher batcher, Camera camera)
		{
			// we still render when we are paused
			if (!_active && !_isPaused)
				return;

			var rootPosition = Entity.Transform.Position + _localOffset;

			// loop through all the particles updating their location and color
			for (var i = 0; i < _particles.Count; i++)
			{
				var currentParticle = _particles[i];
				var pos = _emitterConfig.SimulateInWorldSpace ? currentParticle.SpawnPosition : rootPosition;

				if (_emitterConfig.Sprite == null)
					batcher.Draw(Graphics.Instance.PixelTexture, pos + currentParticle.Position, currentParticle.Color,
						currentParticle.Rotation, Vector2.One, currentParticle.ParticleSize * 0.5f, SpriteEffects.None,
						LayerDepth);
				else
					batcher.Draw(_emitterConfig.Sprite, pos + currentParticle.Position,
						currentParticle.Color, currentParticle.Rotation, _emitterConfig.Sprite.Center,
						currentParticle.ParticleSize / _emitterConfig.Sprite.SourceRect.Width, SpriteEffects.None,
						LayerDepth);
			}
		}

		#endregion


		/// <summary>
		/// removes all particles from the particle emitter
		/// </summary>
		public void Clear()
		{
			for (var i = 0; i < _particles.Count; i++)
				Pool<Particle>.Free(_particles[i]);
			_particles.Clear();
		}

		/// <summary>
		/// plays the particle emitter
		/// </summary>
		public void Play()
		{
			// if we are just unpausing, we only toggle flags and we dont mess with any other parameters
			if (_isPaused)
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
		public void Stop()
		{
			_active = false;
			_isPaused = false;
			_elapsedTime = 0;
			_emitCounter = 0;
			Clear();
		}

		/// <summary>
		/// pauses the particle emitter
		/// </summary>
		public void Pause()
		{
			_isPaused = true;
			_active = false;
		}

		/// <summary>
		/// resumes emission of particles.
		/// this is possible only if stop() wasn't called and emission wasn't stopped due to duration
		/// </summary>
		public void ResumeEmission()
		{
			if (IsStopped || (_emitterConfig.Duration != -1 && _emitterConfig.Duration < _elapsedTime))
				return;

			_emitting = true;
		}

		/// <summary>
		/// pauses emission of particles while allowing existing particles to expire
		/// </summary>
		public void PauseEmission()
		{
			_emitting = false;
		}

		/// <summary>
		/// manually emit some particles
		/// </summary>
		/// <param name="count">Count.</param>
		public void Emit(int count)
		{
			var rootPosition = Entity.Transform.Position + _localOffset;

			Init();
			_active = true;
			for (var i = 0; i < count; i++)
				AddParticle(rootPosition);
		}

		/// <summary>
		/// adds a Particle to the emitter
		/// </summary>
		void AddParticle(Vector2 position)
		{
			// take the next particle out of the particle pool we have created and initialize it
			var particle = Pool<Particle>.Obtain();
			particle.Initialize(_emitterConfig, position);
			_particles.Add(particle);
		}
	}
}