using System;
using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class SimpleMovingPlatform : Component, IUpdatable
	{
		float _minX;
		float _maxX;
		float _minY;
		float _maxY;
		float _speedFactor;


		public SimpleMovingPlatform( float minY, float maxY, float speedFactor = 2f )
		{
			_minY = minY;
			_maxY = maxY;
			_speedFactor = speedFactor;
		}


		public override void onAddedToEntity()
		{
			_minX = entity.transform.position.X;
			_maxX = _minX + 100;
		}


		public void update()
		{
			var x = Mathf.pingPong( Time.time, 1f );
			var xToTheSpeedFactor = Mathf.pow( x, _speedFactor );
			var alpha = 1f - xToTheSpeedFactor / xToTheSpeedFactor + Mathf.pow( 1 - x, _speedFactor );

			var deltaY = Nez.Tweens.Lerps.unclampedLerp( _minY, _maxY, alpha ) - entity.transform.position.Y;
			var deltaX = Nez.Tweens.Lerps.unclampedLerp( _minX, _maxX, alpha ) - entity.transform.position.X;

			// TODO: probably query Physics to fetch the actors that we will intersect instead of blindly grabbing them all
			//var allActors = getAllActors();
			var ridingActors = getAllRidingActors();

			moveSolid( new Vector2( deltaX, deltaY ), ridingActors );
		}


		void moveSolid( Vector2 motion, List<Entity> ridingActors )
		{
			if( motion.X == 0 && motion.Y == 0 )
				return;
			
			entity.colliders.unregisterAllCollidersWithPhysicsSystem();

			moveSolidX( motion.X, ridingActors );
			moveSolidY( motion.Y, ridingActors );

			entity.colliders.registerAllCollidersWithPhysicsSystem();
		}


		void moveSolidX( float amount, List<Entity> ridingActors )
		{
			var moved = false;
			entity.transform.position += new Vector2( amount, 0 );

			var colliders = new HashSet<Collider>( Physics.boxcastBroadphase( entity.colliders.mainCollider.bounds ) );
			foreach( var collider in colliders )
			{
				float pushAmount;
				if( amount > 0 )
					pushAmount = entity.colliders.mainCollider.bounds.right - collider.bounds.left;
				else
					pushAmount = entity.colliders.mainCollider.bounds.left - collider.bounds.right;

				// grinding/shearing will have odd results so watch out for them and correct
				if( Math.Abs( pushAmount ) > Math.Abs( amount ) )
					pushAmount = amount;

				var mover = collider.entity.getComponent<Mover>();
				if( mover != null )
				{
					moved = true;
					CollisionResult collisionResult;
					if( mover.move( new Vector2( pushAmount, 0 ), out collisionResult ) )
					{
						collider.entity.destroy();
						return;
					}
				}
				else
				{
					collider.entity.move( new Vector2( pushAmount, 0 ) );
				}
			}


			foreach( var entity in ridingActors )
			{
				if( !moved )
					entity.move( new Vector2( amount, 0 ) );
			}
		}


		void moveSolidY( float amount, List<Entity> ridingActors )
		{
			var moved = false;
			entity.transform.position += new Vector2( 0, amount );

			var colliders = new HashSet<Collider>( Physics.boxcastBroadphase( entity.colliders.mainCollider.bounds ) );
			foreach( var collider in colliders )
			{
				float pushAmount;
				if( amount > 0 )
					pushAmount = entity.colliders.mainCollider.bounds.bottom - collider.bounds.top;
				else
					pushAmount = entity.colliders.mainCollider.bounds.top - collider.bounds.bottom;

				// grinding/shearing will have odd results so watch out for them and correct
				if( Math.Abs( pushAmount ) > Math.Abs( amount ) )
					pushAmount = amount;
				
				var mover = collider.entity.getComponent<Mover>();
				if( mover != null )
				{
					moved = true;
					CollisionResult collisionResult;
					if( mover.move( new Vector2( 0, pushAmount ), out collisionResult ) )
					{
						collider.entity.destroy();
						return;
					}
				}
				else
				{
					collider.entity.move( new Vector2( 0, pushAmount ) );
				}
			}


			foreach( var entity in ridingActors )
			{
				if( !moved )
					entity.move( new Vector2( 0, amount ) );
			}
		}


		List<Entity> getAllActors()
		{
			var list = new List<Entity>();

			var entities = entity.scene.findEntitiesByTag( 0 );
			for( var i = 0; i < entities.Count; i++ )
			{
				if( entities[i].colliders.mainCollider != entity.colliders.mainCollider && entities[i].colliders.mainCollider != null )
					list.Add( entities[i] );
			}

			return list;
		}


		// this should probably be a method (isRidingCollider( Collider )) on the Entity so that it can decide if it is riding the collider or not.
		// The entity could then be riding for other situations such as ledge hanging on a moving platform.
		List<Entity> getAllRidingActors()
		{
			var list = new List<Entity>();

			var entities = entity.scene.findEntitiesByTag( 0 );
			for( var i = 0; i < entities.Count; i++ )
			{
				if( entities[i].colliders.mainCollider == entity.colliders.mainCollider || entities[i].colliders.mainCollider == null )
					continue;

				CollisionResult collisionResult;
				if( entities[i].colliders.mainCollider.collidesWith( entity.colliders.mainCollider, new Vector2( 0f, 1f ), out collisionResult ) )
					list.Add( entities[i] );
			}

			return list;
		}

	}
}

