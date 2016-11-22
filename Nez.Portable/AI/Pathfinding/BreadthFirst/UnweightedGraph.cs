using System;
using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// basic implementation of an UnweightedGraph. All edges are cached. This type of graph is best suited for non-grid based graphs.
	/// Any nodes added as edges must also have an entry as the key in the edges Dictionary.
	/// </summary>
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

