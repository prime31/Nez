using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nez
{
	public class EntityList
	{
		public Scene scene;

		/// <summary>
		/// list of entities added to the scene
		/// </summary>
		FastList<Entity> _entities = new FastList<Entity>();

		/// <summary>
		/// The list of entities that were added this frame. Used to group the entities so we can process them simultaneously
		/// </summary>
		List<Entity> _entitiesToAdd = new List<Entity>();

		/// <summary>
		/// The list of entities that were marked for removal this frame. Used to group the entities so we can process them simultaneously
		/// </summary>
		List<Entity> _entitiesToRemove = new List<Entity>();

		/// <summary>
		/// flag used to determine if we need to sort our entities this frame
		/// </summary>
		bool _isEntityListUnsorted;

		/// <summary>
		/// tracks entities by tag for easy retrieval
		/// </summary>
		Dictionary<int,List<Entity>> _entityDict = new Dictionary<int, List<Entity>>();
		List<int> _unsortedTags = new List<int>();

		// used in updateLists to double buffer so that the original lists can be modified elsewhere
		List<Entity> _tempEntityList = new List<Entity>();


		public EntityList( Scene scene )
		{
			this.scene = scene;
		}


		#region array access

		public int count { get { return _entities.length; } }

		public Entity this[int index] { get { return _entities.buffer[index]; } }

		#endregion


		public void markEntityListUnsorted()
		{
			_isEntityListUnsorted = true;
		}


		internal void markTagUnsorted( int tag )
		{
			_unsortedTags.Add( tag );
		}


		/// <summary>
		/// adds an Entity to the list. All lifecycle methods will be called in the next frame.
		/// </summary>
		/// <param name="entity">Entity.</param>
		public void add( Entity entity )
		{
			_entitiesToAdd.Add( entity );
		}


		/// <summary>
		/// removes an Entity from the list. All lifecycle methods will be called in the next frame.
		/// </summary>
		/// <param name="entity">Entity.</param>
		public void remove( Entity entity )
		{
			Debug.warnIf( _entitiesToRemove.Contains( entity ), "You are trying to remove an entity ({0}) that you already removed", entity.name );

			// guard against adding and then removing an Entity in the same frame
			if( _entitiesToAdd.Contains( entity ) )
			{
				_entitiesToAdd.Remove( entity );
				return;
			}

			if( !_entitiesToRemove.Contains( entity ) )
				_entitiesToRemove.Add( entity );
		}


		/// <summary>
		/// removes all entities from the entities list and passes them back to the entity cache
		/// </summary>
		public void removeAllEntities()
		{
			// clear lists we dont need anymore
			_unsortedTags.Clear();
			_entitiesToAdd.Clear();
			_isEntityListUnsorted = false;

			// why do we update lists here? Mainly to deal with Entities that were detached before a Scene switch. They will still
			// be in the _entitiesToRemove list which will get handled by updateLists.
			updateLists();

			for( var i = 0; i < _entities.length; i++ )
			{
				_entities.buffer[i]._isDestroyed = true;
				_entities.buffer[i].onRemovedFromScene();
				_entities.buffer[i].scene = null;
			}

			_entities.clear();
			_entityDict.Clear();
		}


		/// <summary>
		/// checks to see if the Entity is presently managed by this EntityList
		/// </summary>
		/// <param name="entity">Entity.</param>
		public bool contains( Entity entity )
		{
			return _entities.contains( entity ) || _entitiesToAdd.Contains( entity );
		}


		List<Entity> getTagList( int tag )
		{
			List<Entity> list = null;
			if( !_entityDict.TryGetValue( tag, out list ) )
			{
				list = new List<Entity>();
				_entityDict[tag] = list;
			}

			return _entityDict[tag];
		}


		internal void addToTagList( Entity entity )
		{
			var list = getTagList( entity.tag );
			Assert.isFalse( list.Contains( entity ), "Entity tag list already contains this entity: {0}", entity );

			list.Add( entity );
			_unsortedTags.Add( entity.tag );
		}


		internal void removeFromTagList( Entity entity )
		{
			_entityDict[entity.tag].Remove( entity );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal void update()
		{
			for( var i = 0; i < _entities.length; i++ )
			{
				var entity = _entities.buffer[i];
				if( entity.enabled && ( entity.updateInterval == 1 || Time.frameCount % entity.updateInterval == 0 ) )
					entity.update();
			}
		}


		public void updateLists()
		{
			// handle removals
			if( _entitiesToRemove.Count > 0 )
			{
				Utils.swap( ref _entitiesToRemove, ref _tempEntityList );
				for( var i = 0; i < _tempEntityList.Count; i++ )
				{
					var entity = _tempEntityList[i];

					// handle the tagList
					removeFromTagList( entity );

					// handle the regular entity list
					_entities.remove( entity );
					entity.onRemovedFromScene();
					entity.scene = null;

					if( Core.entitySystemsEnabled )
						scene.entityProcessors.onEntityRemoved( entity );
				}

				_tempEntityList.Clear();
			}

			// handle additions
			if( _entitiesToAdd.Count > 0 )
			{
				Utils.swap( ref _entitiesToAdd, ref _tempEntityList );
				for( var i = 0; i < _tempEntityList.Count; i++ )
				{
					var entity = _tempEntityList[i];

					_entities.add( entity );
					entity.scene = scene;

					// handle the tagList
					addToTagList( entity );

					if( Core.entitySystemsEnabled )
						scene.entityProcessors.onEntityAdded( entity );
				}

				// now that all entities are added to the scene, we loop through again and call onAddedToScene
				for( var i = 0; i < _tempEntityList.Count; i++ )
					_tempEntityList[i].onAddedToScene();

				_tempEntityList.Clear();
				_isEntityListUnsorted = true;
			}

			if( _isEntityListUnsorted )
			{
				_entities.sort();
				_isEntityListUnsorted = false;
			}

			// sort our tagList if needed
			if( _unsortedTags.Count > 0 )
			{
				for( int i = 0, count = _unsortedTags.Count; i < count; i++ )
				{
					var tag = _unsortedTags[i];
					_entityDict[tag].Sort();
				}
				_unsortedTags.Clear();
			}
		}


		#region Entity search

		/// <summary>
		/// returns the first Entity found with a name of name. If none are found returns null.
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="name">Name.</param>
		public Entity findEntity( string name )
		{
			for( var i = 0; i < _entities.length; i++ )
			{
				if( _entities.buffer[i].name == name )
					return _entities.buffer[i];
			}

			// in case an entity is added and searched for in the same frame we check the toAdd list
			for( var i = 0; i < _entitiesToAdd.Count; i++ )
			{
				if( _entitiesToAdd[i].name == name )
					return _entitiesToAdd[i];
			}

			return null;
		}


		/// <summary>
		/// returns a list of all entities with tag. If no entities have the tag an empty list is returned. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The with tag.</returns>
		/// <param name="tag">Tag.</param>
		public List<Entity> entitiesWithTag( int tag )
		{
			var list = getTagList( tag );

			var returnList = ListPool<Entity>.obtain();
			returnList.AddRange( list );

			return returnList;
		}


		/// <summary>
		/// returns a List of all Entities of type T. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<Entity> entitiesOfType<T>() where T : Entity
		{
			var list = ListPool<Entity>.obtain();
			for( var i = 0; i < _entities.length; i++ )
			{
				if( _entities.buffer[i] is T )
					list.Add( _entities.buffer[i] );
			}

			// in case an entity is added and searched for in the same frame we check the toAdd list
			for( var i = 0; i < _entitiesToAdd.Count; i++ )
			{
				if( _entitiesToAdd[i] is T )
					list.Add( _entitiesToAdd[i] );
			}

			return list;
		}


		/// <summary>
		/// returns the first Component found in the Scene of type T
		/// </summary>
		/// <returns>The component of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T findComponentOfType<T>() where T : Component
		{
			for( var i = 0; i < _entities.length; i++ )
			{
				if( _entities.buffer[i].enabled )
				{
					var comp = _entities.buffer[i].getComponent<T>();
					if( comp != null )
						return comp;
				}
			}

			// in case an entity is added and searched for in the same frame we check the toAdd list
			for( var i = 0; i < _entitiesToAdd.Count; i++ )
			{
				if( _entitiesToAdd[i].enabled )
				{
					var comp = _entitiesToAdd[i].getComponent<T>();
					if( comp != null )
						return comp;
				}
			}

			return null;
		}


		/// <summary>
		/// returns all Components found in the Scene of type T. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The components of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> findComponentsOfType<T>() where T : Component
		{
			var comps = ListPool<T>.obtain();
			for( var i = 0; i < _entities.length; i++ )
			{
				if( _entities.buffer[i].enabled )
					_entities.buffer[i].getComponents<T>( comps );
			}

			// in case an entity is added and searched for in the same frame we check the toAdd list
			for( var i = 0; i < _entitiesToAdd.Count; i++ )
			{
				if( _entitiesToAdd[i].enabled )
					_entitiesToAdd[i].getComponents<T>( comps );
			}

			return comps;
		}

		#endregion

	}
}

