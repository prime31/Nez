using System;
using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	public interface IWeightedGraph<T>
	{
		IEnumerable<T> getNeighbors( T node );

		int cost( T from, T to );

		int heuristic( T node, T goal );
	}
}

