using System;
using System.Collections.Generic;


namespace Nez
{
	public class EntityTagLists
	{
		Dictionary<int,List<Entity>> _entityDict;
		List<int> _unsortedTags;
		bool _areAnyUnsorted;


		internal EntityTagLists()
		{
			_entityDict = new Dictionary<int,List<Entity>>();
			_unsortedTags = new List<int>();
		}


		public List<Entity> getEntitiesWithTag( int tag )
		{
			List<Entity> list = null;
			if( !_entityDict.TryGetValue( tag, out list ) )
			{
				list = new List<Entity>();
				_entityDict[tag] = list;
			}

			return _entityDict[tag];
		}


		internal void markUnsorted( int tag )
		{
			_areAnyUnsorted = true;
			_unsortedTags.Add( tag );
		}


		internal void updateLists()
		{
			if( _areAnyUnsorted )
			{
				for( var i = 0; i < _unsortedTags.Count; i++ )
				{
					var tag = _unsortedTags[i];
					_entityDict[tag].Sort();
				}

				_unsortedTags.Clear();
				_areAnyUnsorted = false;
			}
		}


		internal void addEntity( Entity entity )
		{
			var list = getEntitiesWithTag( entity.tag );
			Assert.isFalse( list.Contains( entity ), "Entity tag list already contains this entity" );

			list.Add( entity );
			_areAnyUnsorted = true;
			_unsortedTags.Add( entity.tag );
		}


		internal void removeEntity( Entity entity )
		{
			if( entity.scene != null )
				_entityDict[entity.tag].Remove( entity );
		}

	}
}

