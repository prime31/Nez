using System;
using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// calculates paths given an IUnweightedGraph and start/goal positions
	/// </summary>
	public static class BreadthFirstPathfinder
	{
		public static List<T> search<T>( IUnweightedGraph<T> graph, T start, T goal )
		{
			var foundPath = false;
			var frontier = new Queue<T>();
			frontier.Enqueue( start );

			var cameFrom = new Dictionary<T,T>();
			cameFrom.Add( start, start );

			while( frontier.Count > 0 )
			{
				var current = frontier.Dequeue();
				if( current.Equals( goal ) )
				{
					foundPath = true;
					break;
				}

				foreach( var next in graph.getNeighbors( current ) )
				{
					if( !cameFrom.ContainsKey( next ) )
					{
						frontier.Enqueue( next );
						cameFrom.Add( next, current );
					}
				}
			}

			return foundPath ? AStarPathfinder.recontructPath( cameFrom, start, goal ) : null;
		}
	}
}

