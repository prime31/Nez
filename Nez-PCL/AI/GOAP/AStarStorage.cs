using System;
using System.Collections.Generic;


namespace Nez.AI.GOAP
{
	public class AStarStorage
	{
		// The maximum number of nodes we can store
		const int MAX_NODES = 128;

		AStarNode[] _opened = new AStarNode[MAX_NODES];
		AStarNode[] _closed = new AStarNode[MAX_NODES];
		int _numOpened;
		int _numClosed;

		int _lastFoundOpened;
		int _lastFoundClosed;


		internal AStarStorage()
		{}


		public void clear()
		{
			for( var i = 0; i < _numOpened; i++ )
			{
				Pool<AStarNode>.free( _opened[i] );
				_opened[i] = null;
			}

			for( var i = 0; i < _numClosed; i++ )
			{
				Pool<AStarNode>.free( _closed[i] );
				_closed[i] = null;
			}

			_numOpened = _numClosed = 0;
			_lastFoundClosed = _lastFoundOpened = 0;
		}


		public AStarNode findOpened( AStarNode node )
		{
			for( var i = 0; i < _numOpened; i++ )
			{
				long care = node.worldState.dontCare ^ -1L;
				if( ( node.worldState.values & care ) == ( _opened[i].worldState.values & care ) )
				{
					_lastFoundClosed = i;
					return _closed[i];
				}
			}
			return null;
		}


		public AStarNode findClosed( AStarNode node )
		{
			for( var i = 0; i < _numClosed; i++ )
			{
				long care = node.worldState.dontCare ^ -1L;
				if( ( node.worldState.values & care ) == ( _closed[i].worldState.values & care ) )
				{
					_lastFoundClosed = i;
					return _closed[i];
				}
			}
			return null;
		}


		public bool hasOpened()
		{
			return _numOpened > 0;
		}


		public void removeOpened( AStarNode node )
		{
			if( _numOpened > 0 )
				_opened[_lastFoundOpened] = _opened[_numOpened - 1];
			_numOpened--;
		}


		public void removeClosed( AStarNode node )
		{
			if( _numClosed > 0 )
				_closed[_lastFoundClosed] = _closed[_numClosed - 1];
			_numClosed--;
		}


		public bool isOpen( AStarNode node )
		{
			return Array.IndexOf( _opened, node ) > -1;
		}


		public bool isClosed( AStarNode node )
		{
			return Array.IndexOf( _closed, node ) > -1;
		}


		public void addToOpenList( AStarNode node )
		{
			_opened[_numOpened++] = node;
		}


		public void addToClosedList( AStarNode node )
		{
			_closed[_numClosed++] = node;
		}


		public AStarNode removeCheapestOpenNode()
		{
			var lowestVal = int.MaxValue;
			_lastFoundOpened = -1;
			for( var i = 0; i < _numOpened; i++ )
			{
				if( _opened[i].costSoFarAndHeuristicCost < lowestVal )
				{
					lowestVal = _opened[i].costSoFarAndHeuristicCost;
					_lastFoundOpened = i;
				}
			}
			var val = _opened[_lastFoundOpened];
			removeOpened( val );

			return val;
		}
	
	}
}

