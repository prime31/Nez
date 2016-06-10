using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Nez.AI.Pathfinding
{
	public class UnweightedGraph<T> : IUnweightedGraph<T>
	{
		public Dictionary<T,T[]> edges = new Dictionary<T,T[]>();


		public UnweightedGraph<T> addEdgesForNode( T node, T[] edges )
		{
			this.edges[node] = edges;
			return this;
		}


		public IEnumerable<T> getNeighbors( T node )
		{
			return edges[node];
		}

	}
}

