using System;
using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	public interface IUnweightedGraph<T>
	{
		IEnumerable<T> getNeighbors( T node );
	}
}

