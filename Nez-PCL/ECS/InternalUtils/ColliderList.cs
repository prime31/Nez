using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class ColliderList : IEnumerable<Collider>, IEnumerable
	{
		/// <summary>
		/// the first collider added is considered the main collider. If there is a collider add pending it will be returned.
		/// </summary>
		/// <value>The main collider.</value>
		public Collider mainCollider
		{
			get
			{
				if( _colliders.Count == 0 )
					return null;
				return _colliders[0];
			}
		}

		Entity _entity;
		List<Collider> _colliders = new List<Collider>();
		bool _isEntityAddedToScene;

		/// <summary>
		/// The list of colliders that were added this frame. Used to group the colliders so we can process them simultaneously
		/// </summary>
		internal List<Collider> _collidersToAdd = new List<Collider>();


		public ColliderList( Entity entity )
		{
			this._entity = entity;
		}


		/// <summary>
		/// adds a Collider to the list and registers it with the Physics system
		/// </summary>
		/// <param name="collider">Collider.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T add<T>( T collider ) where T : Collider
		{
			_collidersToAdd.Add( collider );
			collider.entity = _entity;
			return collider;
		}


		/// <summary>
		/// removes the Collider and unregisters it from the Pysics system
		/// </summary>
		/// <param name="collider">Collider.</param>
		public void remove( Collider collider )
		{
			Assert.isTrue( _colliders.Contains( collider ), "Collider {0} is not in the ColliderList", collider );
			removeAt( _colliders.IndexOf( collider ) );
		}


		/// <summary>
		/// removes all Colliders from the Entity
		/// </summary>
		public void removeAllColliders()
		{
			for( var i = 0; i < _colliders.Count; i++ )
			{
				_colliders[i].unregisterColliderWithPhysicsSystem();
				_colliders[i].entity = null;
			}

			_colliders.Clear();
		}


		/// <summary>
		/// removes the Collider and unregisters it from the Pysics system
		/// </summary>
		/// <param name="index">Index.</param>
		public void removeAt( int index )
		{
			var collider = _colliders[index];
			collider.unregisterColliderWithPhysicsSystem();
			collider.entity = null;
			_colliders.RemoveAt( index );
		}


		internal void onEntityAddedToScene()
		{
			_isEntityAddedToScene = true;
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].onEntityAddedToScene();
		}


		internal void onEntityRemovedFromScene()
		{
			_isEntityAddedToScene = false;
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].onEntityRemovedFromScene();
		}


		internal void onEntityTransformChanged()
		{
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].onEntityTransformChanged();
		}


		internal void onEntityEnabled()
		{
			registerAllCollidersWithPhysicsSystem();
		}


		internal void onEntityDisabled()
		{
			unregisterAllCollidersWithPhysicsSystem();
		}


		internal void debugRender( Graphics graphics )
		{
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].debugRender( graphics );
		}


		public void registerAllCollidersWithPhysicsSystem()
		{
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].registerColliderWithPhysicsSystem();
		}


		public void unregisterAllCollidersWithPhysicsSystem()
		{
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].unregisterColliderWithPhysicsSystem();
		}


		internal void updateLists()
		{
			// handle additions
			if( _collidersToAdd.Count > 0 )
			{
				for( var i = 0; i < _collidersToAdd.Count; i++ )
				{
					var collider = _collidersToAdd[i];
					collider.entity = _entity;
					collider.registerColliderWithPhysicsSystem();
					_colliders.Add( collider );

					// if we are already added to the scene make sure we let the component know
					if( _isEntityAddedToScene )
						collider.onEntityAddedToScene();
				}

				_collidersToAdd.Clear();
			}
		}


		public int Count
		{
			get { return _colliders.Count; }
		}


		public T getCollider<T>( bool onlyReturnInitializedColliders = false ) where T : Collider
		{
			for( var i = 0; i < _colliders.Count; i++ )
			{
				var component = _colliders[i];
				if( component is T )
					return component as T;
			}

			// we optionally check the pending components just in case addComponent and getComponent are called in the same frame
			if( !onlyReturnInitializedColliders )
			{
				for( var i = 0; i < _collidersToAdd.Count; i++ )
				{
					var component = _collidersToAdd[i];
					if( component is T )
						return component as T;
				}
			}

			return null;
		}


		#region IEnumerable and array access

		public Collider this[int index]
		{
			get
			{
				return _colliders[index];
			}
		}


		public IEnumerator<Collider> GetEnumerator()
		{
			return _colliders.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _colliders.GetEnumerator();
		}

		#endregion

	}
}

