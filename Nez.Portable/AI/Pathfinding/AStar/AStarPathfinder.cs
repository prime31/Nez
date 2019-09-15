using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// calculates paths given an IAstarGraph and start/goal positions
	/// </summary>
	public static class AStarPathfinder
	{
		/// <summary>
		/// wraps up the raw data in a small class with the extra fields the PriorityQueue requires
		/// </summary>
		class AStarNode<T> : PriorityQueueNode
		{
			public T Data;

			public AStarNode(T data)
			{
				Data = data;
			}
		}


		public static bool Search<T>(IAstarGraph<T> graph, T start, T goal, out Dictionary<T, T> cameFrom)
		{
			var foundPath = false;
			cameFrom = new Dictionary<T, T>();
			cameFrom.Add(start, start);

			var costSoFar = new Dictionary<T, int>();
			var frontier = new PriorityQueue<AStarNode<T>>(1000);
			frontier.Enqueue(new AStarNode<T>(start), 0);

			costSoFar[start] = 0;

			while (frontier.Count > 0)
			{
				var current = frontier.Dequeue();

				if (current.Data.Equals(goal))
				{
					foundPath = true;
					break;
				}

				foreach (var next in graph.GetNeighbors(current.Data))
				{
					var newCost = costSoFar[current.Data] + graph.Cost(current.Data, next);
					if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
					{
						costSoFar[next] = newCost;
						var priority = newCost + graph.Heuristic(next, goal);
						frontier.Enqueue(new AStarNode<T>(next), priority);
						cameFrom[next] = current.Data;
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
		public static List<T> Search<T>(IAstarGraph<T> graph, T start, T goal)
		{
			Dictionary<T, T> cameFrom;
			var foundPath = Search(graph, start, goal, out cameFrom);

			return foundPath ? RecontructPath(cameFrom, start, goal) : null;
		}


		/// <summary>
		/// reconstructs a path from the cameFrom Dictionary
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="cameFrom">Came from.</param>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> RecontructPath<T>(Dictionary<T, T> cameFrom, T start, T goal)
		{
			var path = new List<T>();
			var current = goal;
			path.Add(goal);

			while (!current.Equals(start))
			{
				current = cameFrom[current];
				path.Add(current);
			}

			path.Reverse();

			return path;
		}
	}
}