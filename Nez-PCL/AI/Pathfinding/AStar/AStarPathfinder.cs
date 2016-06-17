using System;
using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// calculates paths given an IWeightedGraph and start/goal positions
	/// </summary>
	public static class AStarPathfinder
	{
		class AStarNode<T> : PriorityQueueNode
		{
			public T data;

			public AStarNode( T data )
			{
				this.data = data;
			}
		}


		public static bool search<T>( IWeightedGraph<T> graph, T start, T goal, out Dictionary<T,T> cameFrom )
		{
			var foundPath = false;
			cameFrom = new Dictionary<T,T>();
			cameFrom.Add( start, start );

			var costSoFar = new Dictionary<T, int>();
			var frontier = new PriorityQueue<AStarNode<T>>( 1000 );
			frontier.Enqueue( new AStarNode<T>( start ), 0 );

			costSoFar[start] = 0;

			while( frontier.Count > 0 )
			{
				var current = frontier.Dequeue();

				if( current.data.Equals( goal ) )
				{
					foundPath = true;
					break;
				}

				foreach( var next in graph.getNeighbors( current.data ) )
				{
					var newCost = costSoFar[current.data] + graph.cost( current.data, next );
					if( !costSoFar.ContainsKey( next ) || newCost < costSoFar[next] )
					{
						costSoFar[next] = newCost;
						var priority = newCost + graph.heuristic( next, goal );
						frontier.Enqueue( new AStarNode<T>( next ), priority );
						cameFrom[next] = current.data;
					}
				}
			}

			return foundPath;
		}


		/// <summary>
		/// gets a path from start to goal if possible. If no path is found null is returned.
		/// </summary>
		/// <param name="graph">Graph.</param>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> search<T>( IWeightedGraph<T> graph, T start, T goal )
		{
			Dictionary<T,T> cameFrom;
			var foundPath = search( graph, start, goal, out cameFrom );

			return foundPath ? recontructPath( cameFrom, start, goal ) : null;
		}


		/// <summary>
		/// reconstructs a path from the cameFrom Dictionary
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="cameFrom">Came from.</param>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> recontructPath<T>( Dictionary<T,T> cameFrom, T start, T goal )
		{
			var path = new List<T>();
			var current = goal;
			path.Add( goal );

			while( !current.Equals( start ) )
			{
				current = cameFrom[current];
				path.Add( current );
			}
			path.Reverse();

			return path;
		}

	}
}

