using System;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Particles
{
	/// <summary>
	/// the internal fields are required for the ParticleEmitter to be able to render the Particle
	/// </summary>
	public class Particle
	{
		/// <summary>
		/// shared Circle used for collisions checks
		/// </summary>
		static Circle _circleCollisionShape = new Circle( 0 );

		internal Vector2 position;
		internal Vector2 spawnPosition;
		Vector2 _direction;
		internal Color color;
		// stored at particle creation time and used for lerping the color
		Color _startColor;
		// stored at particle creation time and used for lerping the color
		Color _finishColor;
		internal float rotation;
		float _rotationDelta;
		float _radialAcceleration;
		float _tangentialAcceleration;
		float _radius;
		float _radiusDelta;
		float _angle;
		float _degreesPerSecond;
		internal float particleSize;
		float _particleSizeDelta;
		float _timeToLive;
		// stored at particle creation time and used for lerping the color
		float _particleLifetime;

		/// <summary>
		/// flag indicating if this particle has already collided so that we know not to move it in the normal fashion
		/// </summary>
		bool _collided;
		Vector2 _velocity;


		public void initialize( ParticleEmitterConfig emitterConfig, Vector2 spawnPosition )
		{
			_collided = false;

			// init the position of the Particle. This is based on the source position of the particle emitter
			// plus a configured variance. The Random.minusOneToOne method allows the number to be both positive
			// and negative
			position.X = emitterConfig.sourcePositionVariance.X * Random.minusOneToOne();
			position.Y = emitterConfig.sourcePositionVariance.Y * Random.minusOneToOne();

			this.spawnPosition = spawnPosition;

			// init the direction of the   The newAngle is calculated using the angle passed in and the
			// angle variance.
			var newAngle = MathHelper.ToRadians( emitterConfig.angle + emitterConfig.angleVariance * Random.minusOneToOne() );

			// create a new Vector2 using the newAngle
			var vector = new Vector2( Mathf.cos( newAngle ), Mathf.sin( newAngle ) );

			// calculate the vectorSpeed using the speed and speedVariance which has been passed in
			var vectorSpeed = emitterConfig.speed + emitterConfig.speedVariance * Random.minusOneToOne();

			// the particles direction vector is calculated by taking the vector calculated above and
			// multiplying that by the speed
			_direction = vector * vectorSpeed;

			// calculate the particles life span using the life span and variance passed in
			_timeToLive = MathHelper.Max( 0, emitterConfig.particleLifespan + emitterConfig.particleLifespanVariance * Random.minusOneToOne() );
			_particleLifetime = _timeToLive;

			var startRadius = emitterConfig.maxRadius + emitterConfig.maxRadiusVariance * Random.minusOneToOne();
			var endRadius = emitterConfig.minRadius + emitterConfig.minRadiusVariance * Random.minusOneToOne();

			// set the default diameter of the particle from the source position
			_radius = startRadius;
			_radiusDelta = (endRadius - startRadius) / _timeToLive;
			_angle = MathHelper.ToRadians( emitterConfig.angle + emitterConfig.angleVariance * Random.minusOneToOne() );
			_degreesPerSecond = MathHelper.ToRadians( emitterConfig.rotatePerSecond + emitterConfig.rotatePerSecondVariance * Random.minusOneToOne() );

			_radialAcceleration = emitterConfig.radialAcceleration + emitterConfig.radialAccelVariance * Random.minusOneToOne();
			_tangentialAcceleration = emitterConfig.tangentialAcceleration + emitterConfig.tangentialAccelVariance * Random.minusOneToOne();

			// calculate the particle size using the start and finish particle sizes
			var particleStartSize = emitterConfig.startParticleSize + emitterConfig.startParticleSizeVariance * Random.minusOneToOne();
			var particleFinishSize = emitterConfig.finishParticleSize + emitterConfig.finishParticleSizeVariance * Random.minusOneToOne();
			_particleSizeDelta = ( particleFinishSize - particleStartSize ) / _timeToLive;
			particleSize = MathHelper.Max( 0, particleStartSize );


			// calculate the color the particle should have when it starts its life. All the elements
			// of the start color passed in along with the variance are used to calculate the start color
			_startColor = new Color
			(
				(int)( emitterConfig.startColor.R + emitterConfig.startColorVariance.R * Random.minusOneToOne() ),
				(int)( emitterConfig.startColor.G + emitterConfig.startColorVariance.G * Random.minusOneToOne() ),
				(int)( emitterConfig.startColor.B + emitterConfig.startColorVariance.B * Random.minusOneToOne() ),
				(int)( emitterConfig.startColor.A + emitterConfig.startColorVariance.A * Random.minusOneToOne() )
			);
			color = _startColor;

			// calculate the color the particle should be when its life is over. This is done the same
			// way as the start color above
			_finishColor = new Color
			(
				(int)( emitterConfig.finishColor.R + emitterConfig.finishColorVariance.R * Random.minusOneToOne() ),
				(int)( emitterConfig.finishColor.G + emitterConfig.finishColorVariance.G * Random.minusOneToOne() ),
				(int)( emitterConfig.finishColor.B + emitterConfig.finishColorVariance.B * Random.minusOneToOne() ),
				(int)( emitterConfig.finishColor.A + emitterConfig.finishColorVariance.A * Random.minusOneToOne() )
			);

			// calculate the rotation
			var startA = MathHelper.ToRadians( emitterConfig.rotationStart + emitterConfig.rotationStartVariance * Random.minusOneToOne() );
			var endA = MathHelper.ToRadians( emitterConfig.rotationEnd + emitterConfig.rotationEndVariance * Random.minusOneToOne() );
			rotation = startA;
			_rotationDelta = ( endA - startA ) / _timeToLive;
		}


		/// <summary>
		/// updates the particle. Returns true when the particle is no longer alive
		/// </summary>
		/// <param name="emitterConfig">Emitter config.</param>
		public bool update( ParticleEmitterConfig emitterConfig, ref ParticleCollisionConfig collisionConfig, Vector2 rootPosition )
		{
			// PART 1: reduce the life span of the particle
			_timeToLive -= Time.deltaTime;

			// if the current particle is alive then update it
			if( _timeToLive > 0 )
			{
				// only update the particle position if it has not collided. If it has, physics takes over
				if( !_collided )
				{
					// if maxRadius is greater than 0 then the particles are going to spin otherwise they are affected by speed and gravity
					if( emitterConfig.emitterType == ParticleEmitterType.Radial )
					{
						// PART 2: update the angle of the particle from the radius. This is only done if the particles are rotating
						_angle += _degreesPerSecond * Time.deltaTime;
						_radius += _radiusDelta * Time.deltaTime;

						Vector2 tmp;
						tmp.X = -Mathf.cos( _angle ) * _radius;
						tmp.Y = -Mathf.sin( _angle ) * _radius;

						_velocity = tmp - position;
						position = tmp;
					}
					else
					{
						Vector2 tmp, radial, tangential;
						radial = Vector2.Zero;

						if( position.X != 0 || position.Y != 0 )
							Vector2.Normalize( ref position, out radial );

						tangential = radial;
						radial = radial * _radialAcceleration;

						var newy = tangential.X;
						tangential.X = -tangential.Y;
						tangential.Y = newy;
						tangential = tangential * _tangentialAcceleration;

						tmp = radial + tangential + emitterConfig.gravity;
						tmp = tmp * Time.deltaTime;
						_direction = _direction + tmp;
						tmp = _direction * Time.deltaTime;

						_velocity = tmp / Time.deltaTime;
						position = position + tmp;
					}
				}

				// update the particles color. we do the lerp from finish-to-start because timeToLive counts from particleLifespan to 0
				var t = ( _particleLifetime - _timeToLive ) / _particleLifetime;
				ColorExt.lerp( ref _startColor, ref _finishColor, out color, t );

				// update the particle size
				particleSize += _particleSizeDelta * Time.deltaTime;
				particleSize = MathHelper.Max( 0, particleSize );

				// update the rotation of the particle
				rotation += _rotationDelta * Time.deltaTime;


				if( collisionConfig.enabled )
				{
					// if we already collided we have to handle the collision response
					if( _collided )
					{
						// handle after collision movement. we need to track velocity for this
						_velocity += collisionConfig.gravity * Time.deltaTime;
						position += _velocity * Time.deltaTime;

						// if we move too slow we die
						if( _velocity.LengthSquared() < collisionConfig.minKillSpeedSquared )
							return true;
					}

					// should we use our spawnPosition as a reference or the parent Transforms position?
					var pos = emitterConfig.simulateInWorldSpace ? spawnPosition : rootPosition;

					_circleCollisionShape.recalculateBounds( particleSize * 0.5f * collisionConfig.radiusScale, pos + position );
					var neighbors = Physics.boxcastBroadphase( ref _circleCollisionShape.bounds, collisionConfig.collidesWithLayers );
					foreach( var neighbor in neighbors )
					{
						CollisionResult result;
						if( _circleCollisionShape.collidesWithShape( neighbor.shape, out result ) )
						{
							// handle the overlap
							position -= result.minimumTranslationVector;
							calculateCollisionResponseVelocity( collisionConfig.friction, collisionConfig.elasticity, ref result.minimumTranslationVector );

							// handle collision config props
							_timeToLive -= _timeToLive * collisionConfig.lifetimeLoss;
							_collided = true;
						}
					}
				}
			}
			else
			{
				// timeToLive expired. were done
				return true;
			}

			return false;
		}


		/// <summary>
		/// given the relative velocity between the two objects and the MTV this method modifies the relativeVelocity to make it a collision
		/// response.
		/// </summary>
		/// <param name="relativeVelocity">Relative velocity.</param>
		/// <param name="minimumTranslationVector">Minimum translation vector.</param>
		void calculateCollisionResponseVelocity( float friction, float elasticity, ref Vector2 minimumTranslationVector )
		{
			// first, we get the normalized MTV in the opposite direction: the surface normal
			var inverseMTV = minimumTranslationVector * -1f;
			Vector2 normal;
			Vector2.Normalize( ref inverseMTV, out normal );

			// the velocity is decomposed along the normal of the collision and the plane of collision.
			// The elasticity will affect the response along the normal (normalVelocityComponent) and the friction will affect
			// the tangential component of the velocity (tangentialVelocityComponent)
			float n;
			Vector2.Dot( ref _velocity, ref normal, out n );

			var normalVelocityComponent = normal * n;
			var tangentialVelocityComponent = _velocity - normalVelocityComponent;

			if( n > 0.0f )
				normalVelocityComponent = Vector2.Zero;
			
			// elasticity affects the normal component of the velocity and friction affects the tangential component
			var responseVelocity = -( 1.0f + elasticity ) * normalVelocityComponent - friction * tangentialVelocityComponent;
			_velocity += responseVelocity;
		}

	}
}

