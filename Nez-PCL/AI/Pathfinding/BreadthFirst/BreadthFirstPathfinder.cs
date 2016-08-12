﻿using System;
using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// calculates paths given an IUnweightedGraph and start/goal positions
	/// </summary>
	public static class BreadthFirstPathfinder
	{
		public static bool search<T>( IUnweightedGraph<T> graph, T start, T goal, out Dictionary<T,T> cameFrom )
		{
			var foundPath = false;
			var frontier = new Queue<T>();
			frontier.Enqueue( start );

			cameFrom = new Dictionary<T,T>();
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

			return foundPath;
		}


		public static List<T> search<T>( IUnweightedGraph<T> graph, T start, T goal )
		{
			Dictionary<T, T> cameFrom;
			var foundPath = search( graph, start, goal, out cameFrom );
			return foundPath ? AStarPathfinder.recontructPath( cameFrom, start, goal ) : null;
		}
	}
}

