using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class EntityList : IEnumerable<Entity>, IEnumerable
	{
		// global updateOrder sort for Entity lists
		internal static Comparison<Entity> compareEntityOrder = ( a, b ) => { return Math.Sign( b._actualUpdateOrder - a._actualUpdateOrder ); };

		public Scene scene;

		/// <summary>
		/// list of entities added to the scene
		/// </summary>
		List<Entity> _entities = new List<Entity>();
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
		Dictionary<int,List<Entity>> _entityDict;
		List<int> _unsortedTags;



		public EntityList( Scene scene )
		{
			this.scene = scene;
			_entityDict = new Dictionary<int,List<Entity>>();
			_unsortedTags = new List<int>();
		}


		public void markTagUnsorted()
		{
			_isEntityListUnsorted = true;
		}


		internal void markTagUnsorted( int tag )
		{
			_unsortedTags.Add( tag );
		}


		public void add( Entity entity )
		{
			_entitiesToAdd.Add( entity );
		}


		public void remove( Entity entity )
		{
			Debug.warnIf( _entitiesToRemove.Contains( entity ), "You are trying to remove an entity ({0}) that you already removed", entity.name );

			if( !_entitiesToRemove.Contains( entity ) )
				_entitiesToRemove.Add( entity );
		}


		/// <summary>
		/// removes all entities from the entities list and passes them back to the entity cache
		/// </summary>
		public void removeAllEntities()
		{
			for( var i = 0; i < _entities.Count; i++ )
			{
				_entities[i].onRemovedFromScene();
				_entities[i].scene = null;
			}

			_entities.Clear();
		}


		public bool contains( Entity entity )
		{
			return _entities.Contains( entity ) || _entitiesToAdd.Contains( entity );
		}


		internal void addToTagList( Entity entity )
		{
			var list = entitiesWithTag( entity.tag );
			Assert.isFalse( list.Contains( entity ), "Entity tag list already contains this entity: {0}", entity );

			list.Add( entity );
			_unsortedTags.Add( entity.tag );
		}


		internal void removeFromTagList( Entity entity )
		{
			_entityDict[entity.tag].Remove( entity );
		}


		public void updateLists()
		{
			// handle removals
			if( _entitiesToRemove.Count > 0 )
			{
				for( var i = 0; i < _entitiesToRemove.Count; i++ )
				{
					var entity = _entitiesToRemove[i];

					// handle the tagList
					removeFromTagList( entity );

					// handle the regular entity list
					_entities.Remove( entity );
					entity.onRemovedFromScene();
					entity.scene = null;

					if( Core.entitySystemsEnabled )
						scene.entityProcessors.onEntityRemoved( entity );
				}
				_entitiesToRemove.Clear();
			}

			// handle additions
			if( _entitiesToAdd.Count > 0 )
			{
				for( var i = 0; i < _entitiesToAdd.Count; i++ )
				{
					var entity = _entitiesToAdd[i];

					_entities.Add( entity );
					entity.scene = scene;
					entity.onAddedToScene();
					scene.setActualOrder( entity );

					// handle the tagList
					addToTagList( entity );

					if( Core.entitySystemsEnabled )
						scene.entityProcessors.onEntityAdded( entity );
				}

				// now that all entities are added to the scene, we loop through again and call onAwake
				for( var i = 0; i < _entitiesToAdd.Count; i++ )
					_entitiesToAdd[i].onAwake();

				_entitiesToAdd.Clear();
				_isEntityListUnsorted = true;
			}

			if( _isEntityListUnsorted )
			{
				_entities.Sort( compareEntityOrder );
				_isEntityListUnsorted = false;
			}

			// sort our tagList if needed
			if( _unsortedTags.Count > 0 )
			{
				for( var i = 0; i < _unsortedTags.Count; i++ )
				{
					var tag = _unsortedTags[i];
					_entityDict[tag].Sort( EntityList.compareEntityOrder );
				}

				_unsortedTags.Clear();
			}
		}


		public Entity findEntity( string name )
		{
			for( var i = 0; i < _entities.Count; i++ )
			{
				if( _entities[i].name == name )
					return _entities[i];
			}

			// in case an entity is added and searched for in the same frame we check the toAdd list
			for( var i = 0; i < _entitiesToAdd.Count; i++ )
			{
				if( _entitiesToAdd[i].name == name )
					return _entitiesToAdd[i];
			}

			return null;
		}


		public List<Entity> entitiesWithTag( int tag )
		{
			List<Entity> list = null;
			if( !_entityDict.TryGetValue( tag, out list ) )
			{
				list = new List<Entity>();
				_entityDict[tag] = list;
			}

			return _entityDict[tag];
		}


		public int Count
		{
			get
			{
				return _entities.Count;
			}
		}


		#region IEnumerable and array access

		public Entity this[int index]
		{
			get
			{
				return _entities[index];
			}
		}


		public IEnumerator<Entity> GetEnumerator()
		{
			return _entities.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

		#endregion
	
	}
}

