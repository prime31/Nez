using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// helper class used by the Movers to manage trigger colliders interactions and calling ITriggerListeners.
	/// </summary>
	public class ColliderTriggerHelper
	{
		Entity _entity;

		/// <summary>
		/// stores all the active intersection pairs that occured in the current frame
		/// </summary>
		HashSet<Pair<Collider>> _activeTriggerIntersections = new HashSet<Pair<Collider>>();

		/// <summary>
		/// stores the previous frames intersection pairs so that we can detect exits after moving this frame
		/// </summary>
		HashSet<Pair<Collider>> _previousTriggerIntersections = new HashSet<Pair<Collider>>();

		List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();


		public ColliderTriggerHelper(Entity entity)
		{
			_entity = entity;
		}


		/// <summary>
		/// update should be called AFTER Entity is moved. It will take care of any ITriggerListeners that the Collider overlaps.
		/// </summary>
		public void Update()
		{
			// 3. do an overlap check of all entity.colliders that are triggers with all broadphase colliders, triggers or not.
			//    Any overlaps result in trigger events.
			var colliders = _entity.GetComponents<Collider>();
			for (var i = 0; i < colliders.Count; i++)
			{
				var collider = colliders[i];

				// fetch anything that we might collide with us at our new position
				var neighbors = Physics.BoxcastBroadphase(collider.Bounds, collider.CollidesWithLayers);
				foreach (var neighbor in neighbors)
				{
					// we need at least one of the colliders to be a trigger
					if (!collider.IsTrigger && !neighbor.IsTrigger)
						continue;

					if (collider.Overlaps(neighbor))
					{
						var pair = new Pair<Collider>(collider, neighbor);

						// if we already have this pair in one of our sets (the previous or current trigger intersections) dont call the enter event
						var shouldReportTriggerEvent = !_activeTriggerIntersections.Contains(pair) &&
						                               !_previousTriggerIntersections.Contains(pair);
						if (shouldReportTriggerEvent)
							NotifyTriggerListeners(pair, true);

						_activeTriggerIntersections.Add(pair);
					} // overlaps
				} // end foreach
			}

			ListPool<Collider>.Free(colliders);

			CheckForExitedColliders();
		}


		void CheckForExitedColliders()
		{
			// remove all the triggers that we did interact with this frame leaving us with the ones we exited
			_previousTriggerIntersections.ExceptWith(_activeTriggerIntersections);

			foreach (var pair in _previousTriggerIntersections)
				NotifyTriggerListeners(pair, false);

			// clear out the previous set cause we are done with it for now
			_previousTriggerIntersections.Clear();

			// add in all the currently active triggers
			_previousTriggerIntersections.UnionWith(_activeTriggerIntersections);

			// clear out the active set in preparation for the next frame
			_activeTriggerIntersections.Clear();
		}


		void NotifyTriggerListeners(Pair<Collider> collisionPair, bool isEntering)
		{
			// call the onTriggerEnter method for any relevant components
			collisionPair.First.Entity.GetComponents(_tempTriggerList);
			for (var i = 0; i < _tempTriggerList.Count; i++)
			{
				if (isEntering)
					_tempTriggerList[i].OnTriggerEnter(collisionPair.Second, collisionPair.First);
				else
					_tempTriggerList[i].OnTriggerExit(collisionPair.Second, collisionPair.First);
			}

			_tempTriggerList.Clear();

			// also call it for the collider we moved onto if it wasn't destroyed by the first
			if (collisionPair.Second.Entity != null)
			{
				collisionPair.Second.Entity.GetComponents(_tempTriggerList);
				for (var i = 0; i < _tempTriggerList.Count; i++)
				{
					if (isEntering)
						_tempTriggerList[i].OnTriggerEnter(collisionPair.First, collisionPair.Second);
					else
						_tempTriggerList[i].OnTriggerExit(collisionPair.First, collisionPair.Second);
				}

				_tempTriggerList.Clear();
			}
		}
	}
}