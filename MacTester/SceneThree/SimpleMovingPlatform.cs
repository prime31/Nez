using System;
using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class SimpleMovingPlatform : Component
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


		public override void onAwake()
		{
			_minX = entity.position.X;
			_maxX = _minX + 100;
		}


		public override void update()
		{
			var x = Mathf.pingPong( Time.time, 1f );
			var xToTheSpeedFactor = Mathf.pow( x, _speedFactor );
			var alpha = 1f - xToTheSpeedFactor / xToTheSpeedFactor + Mathf.pow( 1 - x, _speedFactor );

			var deltaY = Nez.Tweens.Lerps.unclampedLerp( _minY, _maxY, alpha ) - entity.position.Y;
			var deltaX = Nez.Tweens.Lerps.unclampedLerp( _minX, _maxX, alpha ) - entity.position.X;

			// TODO: probably query Physics to fetch the actors that we will intersect instead of blindly grabbing them all
			//var allActors = getAllActors();
			//var ridingActors = getAllRidingActors();

			// TODO: recreate moveSolid
			entity.moveActor( new Vector2( deltaX, deltaY ) );
			//entity.moveSolid( deltaX, deltaY, allActors, ridingActors );
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
				
				//if( entities[i].colliders.mainCollider.collidesWithAtPosition( entity.colliders.mainCollider, entities[i].position - new Vector2( 0f, -1f ) ) )
				//	list.Add( entities[i] );
			}

			return list;
		}

	}
}

