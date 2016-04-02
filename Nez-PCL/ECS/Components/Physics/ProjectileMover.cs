using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// moves taking collision into account only for reporting to any ITriggerListeners. The object will always move the full amount so it is up
	/// to the caller to destroy it on impact if desired.
	/// </summary>
	public class ProjectileMover : Component
	{
		List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();


		/// <summary>
		/// moves the entity taking collisions into account
		/// </summary>
		/// <returns><c>true</c>, if move actor was newed, <c>false</c> otherwise.</returns>
		/// <param name="motion">Motion.</param>
		/// <param name="collisionResult">Collision result.</param>
		public bool move( Vector2 motion )
		{
			var collider = entity.colliders.mainCollider;

			// no collider? just move and forget about it
			if( collider == null )
			{
				entity.transform.position += motion;
				return false;
			}

			// remove ourself from the physics system until after we are done moving
			entity.colliders.unregisterAllCollidersWithPhysicsSystem();
			var didCollide = false;

			// fetch anything that we might collide with at our new position
			entity.transform.position += motion;

			// fetch anything that we might collide with us at our new position
			var neighbors = Physics.boxcastBroadphase( collider.bounds, collider.collidesWithLayers );
			foreach( var neighbor in neighbors )
			{
				if( collider.overlaps( neighbor ) )
				{
					didCollide = true;
					notifyTriggerListeners( collider, neighbor );
				}
			}

			// let Physics know about our new position
			entity.colliders.registerAllCollidersWithPhysicsSystem();

			return didCollide;
		}


		void notifyTriggerListeners( Collider self, Collider other )
		{
			// notify any listeners on the Entity of the Collider that we overlapped
			other.entity.getComponents( _tempTriggerList );
			for( var i = 0; i < _tempTriggerList.Count; i++ )
				_tempTriggerList[i].onTriggerEnter( self, other );

			_tempTriggerList.Clear();

			// notify any listeners on this Entity
			entity.getComponents( _tempTriggerList );
			for( var i = 0; i < _tempTriggerList.Count; i++ )
				_tempTriggerList[i].onTriggerEnter( other, self );

			_tempTriggerList.Clear();
		}
	}
}

