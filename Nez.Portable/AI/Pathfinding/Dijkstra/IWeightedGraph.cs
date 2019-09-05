using System.Collections.Generic;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// interface for a graph that can be fed to the DijkstraPathfinder.search method
	/// </summary>
	public interface IWeightedGraph<T>
	{
		/// <summary>
		/// The getNeighbors method should return any neighbor nodes that can be reached from the passed in node
		/// </summary>
		/// <returns>The neighbors.</returns>
		/// <param name="node">Node.</param>
		IEnumerable<T> GetNeighbors(T node);

		/// <summary>
		/// calculates the cost to get from 'from' to 'to'
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		int Cost(T from, T to);
	}
}