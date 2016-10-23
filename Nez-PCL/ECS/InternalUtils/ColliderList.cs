using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;


namespace Nez
{
	public class ColliderList : IEnumerable<Collider>, IEnumerable
	{
		/// <summary>
		/// the first collider added is considered the main collider
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
			_entity = entity;
		}


		#region array access

		public int Count { get { return _colliders.Count; } }

		public Collider this[int index] { get { return _colliders[index]; } }

		#endregion


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


		/// <summary>
		/// returns the first Collider of type T found
		/// </summary>
		/// <returns>The collider.</returns>
		/// <param name="onlyReturnInitializedColliders">Only return initialized colliders.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
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


		/// <summary>
		/// returns all the Colliders whether they have been initialized or not without a list allocation
		/// </summary>
		/// <returns>The colliders.</returns>
		/// <param name="colliders">Colliders.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void getColliders( List<Collider> colliders )
		{
			for( var i = 0; i < _colliders.Count; i++ )
				colliders.Add( _colliders[i] );

			for( var i = 0; i < _collidersToAdd.Count; i++ )
				colliders.Add( _collidersToAdd[i] );
		}


		/// <summary>
		/// Gets all the Colliders. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The colliders.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public List<Collider> getColliders()
		{
			var list = ListPool<Collider>.obtain();
			getColliders( list );
			return list;
		}


		#region Collider lifecycle

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


		internal void onEntityTransformChanged( Transform.Component comp )
		{
			for( var i = 0; i < _colliders.Count; i++ )
				_colliders[i].onEntityTransformChanged( comp );
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

		#endregion


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


		#region IEnumerable

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

