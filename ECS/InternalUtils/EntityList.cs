using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class EntityList : IEnumerable<Entity>, IEnumerable
	{
		// global depth sort for Entity lists
		static public Comparison<Entity> compareEntityDepth = ( a, b ) => { return Math.Sign( b._actualDepth - a._actualDepth ); };

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
			_entitiesToRemove.Add( entity );
		}


		public bool contains( Entity entity )
		{
			return _entities.Contains( entity ) || _entitiesToAdd.Contains( entity );
		}


		internal void addToTagList( Entity entity )
		{
			var list = entitiesWithTag( entity.tag );
			Debug.assertIsFalse( list.Contains( entity ), "Entity tag list already contains this entity: {0}", entity );

			list.Add( entity );
			_unsortedTags.Add( entity.tag );
		}


		internal void removeFromTagList( Entity entity )
		{
			_entityDict[entity.tag].Remove( entity );
		}


		public void updateLists()
		{
			if( _entitiesToRemove.Count > 0 )
			{
				foreach( var entity in _entitiesToRemove )
				{
					// handle the tagList
					removeFromTagList( entity );

					// handle the regular entity list
					_entities.Remove( entity );
					entity.onRemovedFromScene();
					entity.scene = null;
					Scene._entityCache.push( entity );
					// Tracker.EntityRemoved(entity);
				}
			}

			if( _entitiesToAdd.Count > 0 )
			{
				foreach( var entity in _entitiesToAdd )
				{
					_entities.Add( entity );
					entity.scene = scene;
					entity.onAddedToScene();
					scene.setActualDepth( entity );

					// handle the tagList
					addToTagList( entity );
					// Tracker.EntityAdded(entity);
				}

				// now that all entities are added to the scene, we loop through again and call onAwake
				foreach( var entity in _entitiesToAdd )
					entity.onAwake();

				_entitiesToAdd.Clear();
				_isEntityListUnsorted = true;
			}

			if( _isEntityListUnsorted )
			{
				_entities.Sort( compareEntityDepth );
				_isEntityListUnsorted = false;
			}

			// sort our tagList if needed
			if( _unsortedTags.Count > 0 )
			{
				foreach( var tag in _unsortedTags )
					_entityDict[tag].Sort( EntityList.compareEntityDepth );

				_unsortedTags.Clear();
			}
		}


		public Entity findEntity( string name )
		{
			foreach( var entity in _entities )
			{
				if( entity.name == name )
					return entity;
			}

			// in case an entity is added and searched for in the same frame we check the toAdd list
			foreach( var entity in _entitiesToAdd )
			{
				if( entity.name == name )
					return entity;
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

