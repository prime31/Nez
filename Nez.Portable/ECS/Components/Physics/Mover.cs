using Microsoft.Xna.Framework;


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
		ColliderTriggerHelper _triggerHelper;


		public override void onAddedToEntity()
		{
			_triggerHelper = new ColliderTriggerHelper( entity );
		}


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
			if( entity.getComponent<Collider>() == null || _triggerHelper == null )
			{
				entity.transform.position += motion;
				return false;
			}

			// 1. move all non-trigger Colliders and get closest collision
			var colliders = entity.getComponents<Collider>();
			for( var i = 0; i < colliders.Count; i++ )
			{
				var collider = colliders[i];

				// skip triggers for now. we will revisit them after we move.
				if( collider.isTrigger )
					continue;

				// fetch anything that we might collide with at our new position
				var bounds = collider.bounds;
				bounds.x += motion.X;
				bounds.y += motion.Y;
				var neighbors = Physics.boxcastBroadphaseExcludingSelf( collider, ref bounds, collider.collidesWithLayers );

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
			ListPool<Collider>.free( colliders );

			// 2. move entity to its new position if we have a collision else move the full amount. motion is updated when a collision occurs
			entity.transform.position += motion;

			// 3. do an overlap check of all Colliders that are triggers with all broadphase colliders, triggers or not.
			//    Any overlaps result in trigger events.
			_triggerHelper.update();

			return collisionResult.collider != null;
		}
	}
}

