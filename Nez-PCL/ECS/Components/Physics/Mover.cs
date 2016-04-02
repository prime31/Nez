using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// helper class illustrating one way to handle movement taking into account all Collisions including triggers. The ITriggerListener
	/// interface is used to manage callbacks to any triggers that are breached while moving. An object must move only via the Mover.move
	/// method for triggers to be properly reported. Note that multiple Movers interacting with each other will end up calling ITriggerListener
	/// multiple times.
	/// </summary>
	public class Mover : Component
	{
		/// <summary>
		/// stores all the active intersection pairs that occured in the current frame
		/// </summary>
		HashSet<Pair<Collider>> _activeTriggerIntersections = new HashSet<Pair<Collider>>();

		/// <summary>
		/// stores the previous frames intersection pairs so that we can detect exits after moving this frame
		/// </summary>
		HashSet<Pair<Collider>> _previousTriggerIntersections = new HashSet<Pair<Collider>>();

		List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();


		/// <summary>
		/// moves the entity taking collisions into account
		/// </summary>
		/// <returns><c>true</c>, if move actor was newed, <c>false</c> otherwise.</returns>
		/// <param name="motion">Motion.</param>
		/// <param name="collisionResult">Collision result.</param>
		public bool move( Vector2 motion, out CollisionResult collisionResult )
		{
			collisionResult = new CollisionResult();

			// no collider? just move and forget about it
			if( entity.colliders.Count == 0 )
			{
				entity.transform.position += motion;
				return false;
			}

			// remove ourself from the physics system until after we are done moving
			entity.colliders.unregisterAllCollidersWithPhysicsSystem();

			// 1. move all non-trigger entity.colliders and get closest collision
			for( var i = 0; i < entity.colliders.Count; i++ )
			{
				var collider = entity.colliders[i];

				// skip triggers for now. we will revisit them after we move.
				if( collider.isTrigger )
					continue;

				// fetch anything that we might collide with at our new position
				var bounds = collider.bounds;
				bounds.x += motion.X;
				bounds.y += motion.Y;
				var neighbors = Physics.boxcastBroadphase( ref bounds, collider.collidesWithLayers );

				foreach( var neighbor in neighbors )
				{
					// skip triggers for now. we will revisit them after we move.
					if( neighbor.isTrigger )
						continue;

					if( collider.collidesWith( neighbor, motion, out collisionResult ) )
					{
						// hit. back off our motion
						motion -= collisionResult.minimumTranslationVector;
					}
				}
			}

			// 2. move entity to its new position if we have a collision else move the full amount. motion is updated when a collision occurs
			entity.transform.position += motion;

			// 3. do an overlap check of all entity.colliders that are triggers with all broadphase colliders, triggers or not.
			//    Any overlaps result in trigger events.
			for( var i = 0; i < entity.colliders.Count; i++ )
			{
				var collider = entity.colliders[i];

				// fetch anything that we might collide with us at our new position
				var neighbors = Physics.boxcastBroadphase( collider.bounds, collider.collidesWithLayers );
				foreach( var neighbor in neighbors )
				{
					// we need at least one of the colliders to be a trigger
					if( !collider.isTrigger && !neighbor.isTrigger )
						continue;

					if( collider.overlaps( neighbor ) )
					{
						var pair = new Pair<Collider>( collider, neighbor );

						// if we already have this pair in one of our sets (the previous or current trigger intersections) dont call the enter event
						var shouldReportTriggerEvent = !_activeTriggerIntersections.Contains( pair ) && !_previousTriggerIntersections.Contains( pair );
						if( shouldReportTriggerEvent )
							notifyTriggerListeners( pair, true );

						_activeTriggerIntersections.Add( pair );
					} // overlaps
				} // end foreach
			}

			// let Physics know about our new position
			entity.colliders.registerAllCollidersWithPhysicsSystem();

			checkForExitedColliders();

			return collisionResult.collider != null;
		}


		void checkForExitedColliders()
		{
			// remove all the triggers that we did interact with this frame leaving us with the ones we exited
			_previousTriggerIntersections.ExceptWith( _activeTriggerIntersections );

			foreach( var pair in _previousTriggerIntersections )
				notifyTriggerListeners( pair, false );

			// clear out the previous set cause we are done with it for now
			_previousTriggerIntersections.Clear();

			// add in all the currently active triggers
			_previousTriggerIntersections.UnionWith( _activeTriggerIntersections );

			// clear out the active set in preparation for the next frame
			_activeTriggerIntersections.Clear();
		}


		void notifyTriggerListeners( Pair<Collider> collisionPair, bool isEntering )
		{
			// call the onTriggerEnter method for any relevant components
			collisionPair.first.entity.getComponents( _tempTriggerList );
			for( var i = 0; i < _tempTriggerList.Count; i++ )
			{
				if( isEntering )
					_tempTriggerList[i].onTriggerEnter( collisionPair.second, collisionPair.first );
				else
					_tempTriggerList[i].onTriggerExit( collisionPair.second, collisionPair.first );
			}

			_tempTriggerList.Clear();

			// also call it for the collider we moved onto
			collisionPair.second.entity.getComponents( _tempTriggerList );
			for( var i = 0; i < _tempTriggerList.Count; i++ )
			{
				if( isEntering )
					_tempTriggerList[i].onTriggerEnter( collisionPair.first, collisionPair.second );
				else
					_tempTriggerList[i].onTriggerExit( collisionPair.first, collisionPair.second );
			}

			_tempTriggerList.Clear();
		}
	
	}
}

