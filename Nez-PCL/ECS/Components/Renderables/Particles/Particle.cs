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
		public Color color;
		// stored at particle creation time and used for lerping the color
		Color startColor;
		// stored at particle creation time and used for lerping the color
		Color finishColor;
		public float rotation;
		float rotationDelta;
		float radialAcceleration;
		float tangentialAcceleration;
		float radius;
		float radiusDelta;
		float angle;
		float degreesPerSecond;
		public float particleSize;
		float particleSizeDelta;
		public float timeToLive;
		// stored at particle creation time and used for lerping the color
		float particleLifetime;

		/// <summary>
		/// flag indicating if this particle has already collided so that we know not to move it in the normal fashion
		/// </summary>
		bool _collided;


		public void initialize( ParticleEmitterConfig emitterConfig )
		{
			_collided = false;

			// init the position of the Particle. This is based on the source position of the particle emitter
			// plus a configured variance. The Random.minusOneToOne method allows the number to be both positive
			// and negative
			position.X = emitterConfig.sourcePositionVariance.X * Random.minusOneToOne();
			position.Y = emitterConfig.sourcePositionVariance.Y * Random.minusOneToOne();

			// init the direction of the   The newAngle is calculated using the angle passed in and the
			// angle variance.
			var newAngle = MathHelper.ToRadians( emitterConfig.angle + emitterConfig.angleVariance * Random.minusOneToOne() );

			// create a new Vector2 using the newAngle
			var vector = new Vector2( Mathf.cos( newAngle ), Mathf.sin( newAngle ) );

			// calculate the vectorSpeed using the speed and speedVariance which has been passed in
			var vectorSpeed = emitterConfig.speed + emitterConfig.speedVariance * Random.minusOneToOne();

			// the particles direction vector is calculated by taking the vector calculated above and
			// multiplying that by the speed
			direction = vector * vectorSpeed;

			// calculate the particles life span using the life span and variance passed in
			timeToLive = MathHelper.Max( 0, emitterConfig.particleLifespan + emitterConfig.particleLifespanVariance * Random.minusOneToOne() );
			particleLifetime = timeToLive;

			var startRadius = emitterConfig.maxRadius + emitterConfig.maxRadiusVariance * Random.minusOneToOne();
			var endRadius = emitterConfig.minRadius + emitterConfig.minRadiusVariance * Random.minusOneToOne();

			// set the default diameter of the particle from the source position
			radius = startRadius;
			radiusDelta = (endRadius - startRadius) / timeToLive;
			angle = MathHelper.ToRadians( emitterConfig.angle + emitterConfig.angleVariance * Random.minusOneToOne() );
			degreesPerSecond = MathHelper.ToRadians( emitterConfig.rotatePerSecond + emitterConfig.rotatePerSecondVariance * Random.minusOneToOne() );

			radialAcceleration = emitterConfig.radialAcceleration + emitterConfig.radialAccelVariance * Random.minusOneToOne();
			tangentialAcceleration = emitterConfig.tangentialAcceleration + emitterConfig.tangentialAccelVariance * Random.minusOneToOne();

			// calculate the particle size using the start and finish particle sizes
			var particleStartSize = emitterConfig.startParticleSize + emitterConfig.startParticleSizeVariance * Random.minusOneToOne();
			var particleFinishSize = emitterConfig.finishParticleSize + emitterConfig.finishParticleSizeVariance * Random.minusOneToOne();
			particleSizeDelta = ( particleFinishSize - particleStartSize ) / timeToLive;
			particleSize = MathHelper.Max( 0, particleStartSize );


			// calculate the color the particle should have when it starts its life. All the elements
			// of the start color passed in along with the variance are used to calculate the start color
			startColor = new Color
			(
				(int)( emitterConfig.startColor.R + emitterConfig.startColorVariance.R * Random.minusOneToOne() ),
				(int)( emitterConfig.startColor.G + emitterConfig.startColorVariance.G * Random.minusOneToOne() ),
				(int)( emitterConfig.startColor.B + emitterConfig.startColorVariance.B * Random.minusOneToOne() ),
				(int)( emitterConfig.startColor.A + emitterConfig.startColorVariance.A * Random.minusOneToOne() )
			);
			color = startColor;

			// calculate the color the particle should be when its life is over. This is done the same
			// way as the start color above
			finishColor = new Color
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
			rotationDelta = ( endA - startA ) / timeToLive;
		}


		/// <summary>
		/// updates the particle. Returns true when the particle is no longer alive
		/// </summary>
		/// <param name="emitterConfig">Emitter config.</param>
		public bool update( ParticleEmitterConfig emitterConfig, ref ParticleCollisionConfig collisionConfig, Vector2 rootPosition )
		{
			// FIX 1
			// reduce the life span of the particle
			timeToLive -= Time.deltaTime;

			// if the current particle is alive then update it
			if( timeToLive > 0 )
			{
				// if maxRadius is greater than 0 then the particles are going to spin otherwise they are effected by speed and gravity
				if( emitterConfig.emitterType == ParticleEmitterType.Radial )
				{
					// FIX 2
					// update the angle of the particle from the radius. This is only done if the particles are rotating
					angle += degreesPerSecond * Time.deltaTime;
					radius += radiusDelta * Time.deltaTime;

					Vector2 tmp;
					tmp.X = -Mathf.cos( angle ) * radius;
					tmp.Y = -Mathf.sin( angle ) * radius;

					if( !_collided )
						position = tmp;
				}
				else
				{
					Vector2 tmp, radial, tangential;
					radial = Vector2.Zero;

					if( position.X != 0 || position.Y != 0 )
						Vector2.Normalize( ref position, out radial );

					tangential = radial;
					radial = radial * radialAcceleration;

					var newy = tangential.X;
					tangential.X = -tangential.Y;
					tangential.Y = newy;
					tangential = tangential * tangentialAcceleration;

					tmp = radial + tangential + emitterConfig.gravity;
					tmp = tmp * Time.deltaTime;
					direction = direction + tmp;
					tmp = direction * Time.deltaTime;

					if( !_collided )
						position = position + tmp;
				}

				// update the particles color. we do the lerp from finish-to-start because timeToLive counts from particleLifespan to 0
				var t = ( particleLifetime - timeToLive ) / particleLifetime;
				ColorExt.lerp( ref startColor, ref finishColor, out color, t );

				// update the particle size
				particleSize += particleSizeDelta * Time.deltaTime;
				particleSize = MathHelper.Max( 0, particleSize );

				// update the rotation of the particle
				rotation += rotationDelta * Time.deltaTime;


				if( collisionConfig.enabled )
				{
					// if we already collided we have to handle the collision response
					if( _collided )
					{
						// TODO: handle after collision movement. we need to track velocity for this
						position += new Vector2( 0f, -5f ) + collisionConfig.gravity * Time.deltaTime;
					}
					
					var collider = Physics.overlapCircle( rootPosition + position, particleSize * 0.5f * collisionConfig.radiusScale, collisionConfig.collidesWithLayers );
					if( collider != null )
					{
						_collided = true;
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

	}
}

