using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Note that this is not a full, multi-iteration physics system! This can be used for simple, arcade style physics.
	/// Based on http://elancev.name/oliver/2D%20polygon.htm#tut5
	/// </summary>
	public class ArcadeRigidbody : Component, IUpdatable
	{
		/// <summary>
		/// mass of this rigidbody. A 0 mass will make this an immovable object.
		/// </summary>
		/// <value>The mass.</value>
		public float mass
		{
			get { return _mass; }
			set { setMass( value ); }
		}

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		public float elasticity
		{
			get { return _elasticity; }
			set { setElasticity( value ); }
		}

		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		public float friction
		{
			get { return _friction; }
			set { setFriction( value ); }
		}

		/// <summary>
		/// 0 - 9 range. When a collision occurs and it has risidual motion along the surface of collision if its square magnitude is less
		/// than glue friction will be set to the maximum for the collision resolution.
		/// </summary>
		public float glue
		{
			get { return _glue; }
			set { setGlue( value );  }
		}

		/// <summary>
		/// if true, Physics.gravity will be taken into account each frame
		/// </summary>
		public bool shouldUseGravity = true;

		/// <summary>
		/// velocity of this rigidbody
		/// </summary>
		public Vector2 velocity;

		/// <summary>
		/// rigidbodies with a mass of 0 are considered immovable. Changing velocity and collisions will have no effect on them.
		/// </summary>
		/// <value><c>true</c> if is immovable; otherwise, <c>false</c>.</value>
		public bool isImmovable { get { return _mass < 0.0001f; } }

		float _mass = 10f;
		float _elasticity = 0.5f;
		float _friction = 0.5f;
		float _glue = 0.01f;
		float _inverseMass;
		Collider _collider;


		public ArcadeRigidbody()
		{
			_inverseMass = 1 / _mass;
		}


		#region Fluent setters

		/// <summary>
		/// mass of this rigidbody. A 0 mass will make this an immovable object.
		/// </summary>
		/// <returns>The mass.</returns>
		/// <param name="mass">Mass.</param>
		public ArcadeRigidbody setMass( float mass )
		{
			_mass = Mathf.clamp( mass, 0, float.MaxValue );

			if( _mass > 0.0001f )
				_inverseMass = 1 / _mass;
			else
				_inverseMass = 0f;
			return this;
		}


		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		/// <returns>The elasticity.</returns>
		/// <param name="value">Value.</param>
		public ArcadeRigidbody setElasticity( float value )
		{
			_elasticity = Mathf.clamp01( value );
			return this;
		}


		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		/// <returns>The friction.</returns>
		/// <param name="value">Value.</param>
		public ArcadeRigidbody setFriction( float value )
		{
			_friction = Mathf.clamp01( value );
			return this;
		}


		/// <summary>
		/// 0 - 9 range. When a collision occurs and it has risidual motion along the surface of collision if its square magnitude is less
		/// than glue friction will be set to the maximum for the collision resolution.
		/// </summary>
		/// <returns>The glue.</returns>
		/// <param name="value">Value.</param>
		public ArcadeRigidbody setGlue( float value )
		{
			_glue = Mathf.clamp( value, 0, 10 );
			return this;
		}


		/// <summary>
		/// velocity of this rigidbody
		/// </summary>
		/// <returns>The velocity.</returns>
		/// <param name="velocity">Velocity.</param>
		public ArcadeRigidbody setVelocity( Vector2 velocity )
		{
			this.velocity = velocity;
			return this;
		}

		#endregion


		/// <summary>
		/// add an instant force impulse to the rigidbody using its mass. force is an acceleration in pixels per second per second. The
		/// force is multiplied by 100000 to make the values more reasonable to use.
		/// </summary>
		/// <param name="force">Force.</param>
		public void addImpulse( Vector2 force )
		{
			if( !isImmovable )
				velocity += force * 100000 * ( _inverseMass * Time.deltaTime * Time.deltaTime );
		}


		public override void onAddedToEntity()
		{
			_collider = entity.getComponent<Collider>();
		}


		void IUpdatable.update()
		{
			if( isImmovable )
			{
				velocity = Vector2.Zero;
				return;
			}

			if( shouldUseGravity )
				velocity += Physics.gravity * Time.deltaTime;
			
			entity.transform.position += velocity * Time.deltaTime;

			CollisionResult collisionResult;
			// fetch anything that we might collide with at our new position
			var neighbors = Physics.boxcastBroadphaseExcludingSelf( _collider, _collider.collidesWithLayers );
			foreach( var neighbor in neighbors )
			{
				// if the neighbor collider is of the same entity, ignore it
				if( neighbor.entity == entity )
				{
					continue;
				}

				if( _collider.collidesWith( neighbor, out collisionResult ) )
				{
					// if the neighbor has an ArcadeRigidbody we handle full collision response. If not, we calculate things based on the
					// neighbor being immovable.
					var neighborRigidbody = neighbor.entity.getComponent<ArcadeRigidbody>();
					if( neighborRigidbody != null )
					{
						processOverlap( neighborRigidbody, ref collisionResult.minimumTranslationVector );
						processCollision( neighborRigidbody, ref collisionResult.minimumTranslationVector );
					}
					else
					{
						// neighbor has no ArcadeRigidbody so we assume its immovable and only move ourself
						entity.transform.position -= collisionResult.minimumTranslationVector;
						var relativeVelocity = velocity;
						calculateResponseVelocity( ref relativeVelocity, ref collisionResult.minimumTranslationVector, out relativeVelocity );
						velocity += relativeVelocity;
					}
				}
			}
		}


		/// <summary>
		/// separates two overlapping rigidbodies. Handles the case of either being immovable as well.
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="minimumTranslationVector"></param>
		void processOverlap( ArcadeRigidbody other, ref Vector2 minimumTranslationVector )
		{
			if( isImmovable )
			{
				other.entity.transform.position += minimumTranslationVector;
			}
			else if( other.isImmovable )
			{
				entity.transform.position -= minimumTranslationVector;
			}
			else
			{
				entity.transform.position -= minimumTranslationVector * 0.5f;
				other.entity.transform.position += minimumTranslationVector * 0.5f;
			}
		}


		/// <summary>
		/// handles the collision of two non-overlapping rigidbodies. New velocities will be assigned to each rigidbody as appropriate.
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="inverseMTV">Inverse MT.</param>
		void processCollision( ArcadeRigidbody other, ref Vector2 minimumTranslationVector )
		{
			// we compute a response for the two colliding objects. The calculations are based on the relative velocity of the objects
			// which gets reflected along the collided surface normal. Then a part of the response gets added to each object based on mass.
			var relativeVelocity = velocity - other.velocity;

			calculateResponseVelocity( ref relativeVelocity, ref minimumTranslationVector, out relativeVelocity );

			// now we use the masses to linearly scale the response on both rigidbodies
			var totalInverseMass = _inverseMass + other._inverseMass;
			var ourResponseFraction = _inverseMass / totalInverseMass;
			var otherResponseFraction = other._inverseMass / totalInverseMass;

			velocity += relativeVelocity * ourResponseFraction;
			other.velocity -= relativeVelocity * otherResponseFraction;
		}


		/// <summary>
		/// given the relative velocity between the two objects and the MTV this method modifies the relativeVelocity to make it a collision
		/// response.
		/// </summary>
		/// <param name="relativeVelocity">Relative velocity.</param>
		/// <param name="minimumTranslationVector">Minimum translation vector.</param>
		void calculateResponseVelocity( ref Vector2 relativeVelocity, ref Vector2 minimumTranslationVector, out Vector2 responseVelocity )
		{
			// first, we get the normalized MTV in the opposite direction: the surface normal
			var inverseMTV = minimumTranslationVector * -1f;
			Vector2 normal;
			Vector2.Normalize( ref inverseMTV, out normal );

			// the velocity is decomposed along the normal of the collision and the plane of collision.
			// The elasticity will affect the response along the normal (normalVelocityComponent) and the friction will affect
			// the tangential component of the velocity (tangentialVelocityComponent)
			float n;
			Vector2.Dot( ref relativeVelocity, ref normal, out n );

			var normalVelocityComponent = normal * n;
			var tangentialVelocityComponent = relativeVelocity - normalVelocityComponent;

			if( n > 0.0f )
				normalVelocityComponent = Vector2.Zero;

			// if the squared magnitude of the tangential component is less than glue then we bump up the friction to the max
			var coefficientOfFriction = _friction;
			if( tangentialVelocityComponent.LengthSquared() < _glue )
				coefficientOfFriction = 1.01f;

			// elasticity affects the normal component of the velocity and friction affects the tangential component
			responseVelocity = -( 1.0f + _elasticity ) * normalVelocityComponent - coefficientOfFriction * tangentialVelocityComponent;
		}

	}
}

