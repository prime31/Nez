using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class ArcadeRigidbody : Component, IUpdatable
	{
		public float mass = 10f;

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		public float elasticity = 0.5f;

		/// <summary>
		/// 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		public float friction = 0.5f;

		public float glue = 0.01f;

		public Vector2 velocity;

		public bool isImmovable { get { return mass < 0.0001f; } }

		float inverseMass
		{
			get
			{ 
				if( mass > 0.0001f )
					return 1 / mass;
				return 0f;
			}
		}


		public ArcadeRigidbody()
		{}


		public void addImpulse( Vector2 force )
		{
			if( !isImmovable )
				velocity += force * ( inverseMass * Time.deltaTime * Time.deltaTime );
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
			var CoF = friction;

			if( dt < glue * glue )
				CoF = 1.01f;

			relativeVelocity = -( 1.0f + elasticity ) * Dn - CoF * Dt;

			var m0 = inverseMass;
			var m1 = other.inverseMass;
			var m  = m0 + m1;
			var r0 = m0 / m;
			var r1 = m1 / m;

			Debug.log( "D {0}, r0: {1}, r1: {2} --- Dn: {3}, Dt: {4}", relativeVelocity, r0, r1, Dn, Dt );

			velocity += relativeVelocity * r0;
			other.velocity -= relativeVelocity * r1;
		}
	}
}

