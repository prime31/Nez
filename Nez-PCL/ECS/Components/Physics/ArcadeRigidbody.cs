using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Note that this is not a full, multi-iteration physics object! This can be used for simple, arcade style physics.
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
			set
			{
				_mass = Mathf.clamp( value, 0, float.MaxValue );

				if( _mass > 0.0001f )
					_inverseMass = 1 / _mass;
				else
					_inverseMass = 0f;
			}
		}
		float _mass = 10f;

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		public float elasticity
		{
			get { return _elasticity; }
			set { _elasticity = Mathf.clamp01( value ); }
		}
		float _elasticity = 0.5f;

		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		public float friction
		{
			get { return _friction; }
			set { _friction = Mathf.clamp01( value ); }
		}
		float _friction = 0.5f;

		/// <summary>
		/// 0 - 3 range.
		/// </summary>
		public float glue
		{
			get { return _glue; }
			set { _glue = Mathf.clamp( value, 0, 3 ); }
		}
		float _glue = 0.01f;

		/// <summary>
		/// velocity of this rigidbody
		/// </summary>
		public Vector2 velocity;

		/// <summary>
		/// rigidbodies with a mass of 0 are considered immovable. Changing velocity and collisions will have no effect on them.
		/// </summary>
		/// <value><c>true</c> if is immovable; otherwise, <c>false</c>.</value>
		public bool isImmovable { get { return _mass < 0.0001f; } }

		float _inverseMass;


		public ArcadeRigidbody()
		{
			_inverseMass = 1 / _mass;
		}


		public void addImpulse( Vector2 force )
		{
			if( !isImmovable )
				velocity += force * ( _inverseMass * Time.deltaTime * Time.deltaTime );
		}


		public void update()
		{
			if( isImmovable )
			{
				velocity = Vector2.Zero;
				return;
			}
			
			entity.colliders.unregisterAllCollidersWithPhysicsSystem();
			entity.transform.position += velocity * Time.deltaTime;

			CollisionResult collisionResult;
			// fetch anything that we might collide with at our new position
			var neighbors = Physics.boxcastBroadphase( entity.colliders.mainCollider.bounds );
			foreach( var neighbor in neighbors )
			{
				if( entity.colliders.mainCollider.collidesWith( neighbor, out collisionResult ) )
				{
					processOverlap( neighbor.entity.getComponent<ArcadeRigidbody>(), collisionResult );

//					var otherRigidbody = neighbor.entity.getComponent<Rigidbody>();
//					var newVel = ( velocity * ( mass - otherRigidbody.mass ) + ( 2 * otherRigidbody.mass * otherRigidbody.velocity ) ) / ( mass + otherRigidbody.mass );
//					var newVel2 = ( otherRigidbody.velocity * ( otherRigidbody.mass - mass ) + ( 2 * mass * velocity ) ) / ( mass + otherRigidbody.mass );
//
//					velocity = newVel;
//					entity.transform.position += velocity * Time.deltaTime;
//
//					otherRigidbody.entity.colliders.unregisterAllCollidersWithPhysicsSystem();
//					otherRigidbody.velocity = newVel2;
//					otherRigidbody.entity.transform.position += otherRigidbody.velocity * Time.deltaTime;
//					otherRigidbody.entity.colliders.registerAllCollidersWithPhysicsSystem();

					break;
				}
			}

			entity.colliders.registerAllCollidersWithPhysicsSystem();
		}


		void processOverlap( ArcadeRigidbody other, CollisionResult collisionResult )
		{
			if( isImmovable )
			{
				other.entity.colliders.unregisterAllCollidersWithPhysicsSystem();
				other.entity.transform.position += collisionResult.minimumTranslationVector;
				other.entity.colliders.registerAllCollidersWithPhysicsSystem();
			}
			else if( other.isImmovable )
			{
				entity.transform.position -= collisionResult.minimumTranslationVector;
			}
			else
			{
				entity.transform.position -= collisionResult.minimumTranslationVector * 0.5f;

				other.entity.colliders.unregisterAllCollidersWithPhysicsSystem();
				other.entity.transform.position += collisionResult.minimumTranslationVector * 0.5f;
				other.entity.colliders.registerAllCollidersWithPhysicsSystem();
			}

			processCollision( other, collisionResult.minimumTranslationVector * -1f );
		}


		void processCollision( ArcadeRigidbody other, Vector2 inverstMTV )
		{
			Vector2 N;
			Vector2.Normalize( ref inverstMTV, out N );

			var relativeVelocity = velocity - other.velocity;
			float n;
			Vector2.Dot( ref relativeVelocity, ref N, out n );

			var Dn = N * n;
			var Dt = relativeVelocity - Dn;

			if( n > 0.0f )
				Dn = Vector2.Zero;

			float dt;
			Vector2.Dot( ref Dt, ref Dt, out dt );
			var CoF = _friction;

			if( dt < _glue * _glue )
				CoF = 1.01f;

			relativeVelocity = -( 1.0f + _elasticity ) * Dn - CoF * Dt;

			var m0 = _inverseMass;
			var m1 = other._inverseMass;
			var m  = m0 + m1;
			var r0 = m0 / m;
			var r1 = m1 / m;

			Debug.log( "D {0}, r0: {1}, r1: {2} --- Dn: {3}, Dt: {4}", relativeVelocity, r0, r1, Dn, Dt );

			velocity += relativeVelocity * r0;
			other.velocity -= relativeVelocity * r1;
		}
	}
}

