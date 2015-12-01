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
			move( deltaX, deltaY );
		}


		void move( float deltaX, float deltaY )
		{
			// TODO: add in remainder stuff from Entity
			deltaX = Mathf.roundToInt( deltaX );
			deltaY = Mathf.roundToInt( deltaY );

			if( deltaX == 0f && deltaY == 0f )
				return;

			// remove ourself from the physics system until after we are done moving
			var oldBounds = entity.collider.bounds;
			Physics.removeCollider( entity.collider, ref oldBounds );

			// TODO: probably query Physics to fetch the actors that we will intersect instead of blindly grabbing them all
			var allActors = getAllActors();
			var ridingActors = getAllRidingActors();

			if( deltaX != 0f )
			{
				// TODO: remainder
				entity.position += new Vector2( deltaX, 0f );
				moveX( deltaX, allActors, ridingActors );
			}

			if( deltaY != 0f )
			{
				// TODO: remainder
				entity.position += new Vector2( 0f, deltaY );
				moveY( deltaY, allActors, ridingActors );
			}

			// let Physics know about our new position
			Physics.addCollider( entity.collider );
		}


		void moveX( float amount, List<Entity> allActors, List<Entity> ridingActors )
		{
			for( var i = 0; i < allActors.Count; i++ )
			{
				var actor = allActors[i];

				if( actor.collider.collidesWith( entity.collider ) )
				{
					// push. deal with moving left/right
					float moveAmount;
					if( amount > 0f )
						moveAmount = entity.collider.bounds.Right - actor.collider.bounds.Left;
					else
						moveAmount = entity.collider.bounds.Left - actor.collider.bounds.Right;

					if( actor.move( moveAmount, 0 ) )
					{
						// collided! squashed!
						entity.scene.removeEntity( actor );
					}
				}
				else if( ridingActors.Contains( actor ) )
				{
					// riding
					actor.move( amount, 0 );
				}
			}
		}


		void moveY( float amount, List<Entity> allActors, List<Entity> ridingActors )
		{
			for( var i = 0; i < allActors.Count; i++ )
			{
				var actor = allActors[i];

				if( actor.collider.collidesWith( entity.collider ) )
				{
					// push. deal with moving up/down
					float moveAmount;
					if( amount > 0f )
						moveAmount = entity.collider.bounds.Bottom - actor.collider.bounds.Top;
					else
						moveAmount = entity.collider.bounds.Top - actor.collider.bounds.Bottom;

					if( actor.move( 0, moveAmount ) )
					{
						// collided! squashed!
						entity.scene.removeEntity( actor );
					}
				}
				else if( ridingActors.Contains( actor ) )
				{
					// riding
					actor.move( 0, amount );
				}
			}
		}


		List<Entity> getAllActors()
		{
			var list = new List<Entity>();

			var entities = entity.scene.findEntitiesByTag( 0 );
			for( var i = 0; i < entities.Count; i++ )
			{
				if( entities[i].collider != entity.collider && entities[i].collider != null )
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
				if( entities[i].collider == entity.collider || entities[i].collider == null )
					continue;
				
				if( entities[i].collider.collidesWithAtPosition( entity.collider, entities[i].position - new Vector2( 0f, -1f ) ) )
					list.Add( entities[i] );
			}

			return list;
		}

	}
}

