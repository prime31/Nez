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
		Collider _collider;


		public override void OnAddedToEntity()
		{
			_collider = Entity.GetComponent<Collider>();
			Debug.WarnIf(_collider == null, "ProjectileMover has no Collider. ProjectilMover requires a Collider!");
		}


		/// <summary>
		/// moves the entity taking collisions into account
		/// </summary>
		/// <returns><c>true</c>, if move actor was newed, <c>false</c> otherwise.</returns>
		/// <param name="motion">Motion.</param>
		public bool Move(Vector2 motion)
		{
			if (_collider == null)
				return false;

			var didCollide = false;

			// fetch anything that we might collide with at our new position
			Entity.Transform.Position += motion;

			// fetch anything that we might collide with us at our new position
			var neighbors = Physics.BoxcastBroadphase(_collider.Bounds, _collider.CollidesWithLayers);
			foreach (var neighbor in neighbors)
			{
				if (_collider.Overlaps(neighbor))
				{
					didCollide = true;
					NotifyTriggerListeners(_collider, neighbor);
				}
			}

			return didCollide;
		}

		void NotifyTriggerListeners(Collider self, Collider other)
		{
			// notify any listeners on the Entity of the Collider that we overlapped
			other.Entity.GetComponents(_tempTriggerList);
			for (var i = 0; i < _tempTriggerList.Count; i++)
				_tempTriggerList[i].OnTriggerEnter(self, other);

			_tempTriggerList.Clear();

			// notify any listeners on this Entity
			Entity.GetComponents(_tempTriggerList);
			for (var i = 0; i < _tempTriggerList.Count; i++)
				_tempTriggerList[i].OnTriggerEnter(other, self);

			_tempTriggerList.Clear();
		}
	}
}