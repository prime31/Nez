using System;
using System.Collections.Generic;


namespace Nez.AI.GOAP
{
	public class AStarNode : IComparable<AStarNode>, IEquatable<AStarNode>, IPoolable
	{
		/// <summary>
		/// The state of the world at this node.
		/// </summary>
		public WorldState WorldState;

		/// <summary>
		/// The cost so far.
		/// </summary>
		public int CostSoFar;

		/// <summary>
		/// The heuristic for remaining cost (don't overestimate!)
		/// </summary>
		public int HeuristicCost;

		/// <summary>
		/// costSoFar + heuristicCost (g+h) combined.
		/// </summary>
		public int CostSoFarAndHeuristicCost;

		/// <summary>
		/// the Action associated with this node
		/// </summary>
		public Action Action;

		// Where did we come from?
		public AStarNode Parent;
		public WorldState ParentWorldState;
		public int Depth;


		#region IEquatable and IComparable

		public bool Equals(AStarNode other)
		{
			long care = WorldState.DontCare ^ -1L;
			return (WorldState.Values & care) == (other.WorldState.Values & care);
		}


		public int CompareTo(AStarNode other)
		{
			return CostSoFarAndHeuristicCost.CompareTo(other.CostSoFarAndHeuristicCost);
		}

		#endregion


		public void Reset()
		{
			Action = null;
			Parent = null;
		}


		public AStarNode Clone()
		{
			return (AStarNode) MemberwiseClone();
		}


		public override string ToString()
		{
			return string.Format("[cost: {0} | heuristic: {1}]: {2}", CostSoFar, HeuristicCost, Action);
		}
	}


	public class AStar
	{
		static AStarStorage storage = new AStarStorage();

		/* from: http://theory.stanford.edu/~amitp/GameProgramming/ImplementationNotes.html
		OPEN = priority queue containing START
		CLOSED = empty set
		while lowest rank in OPEN is not the GOAL:
		  current = remove lowest rank item from OPEN
		  add current to CLOSED
		  for neighbors of current:
		    cost = g(current) + movementcost(current, neighbor)
		    if neighbor in OPEN and cost less than g(neighbor):
		      remove neighbor from OPEN, because new path is better
		    if neighbor in CLOSED and cost less than g(neighbor): **
		      remove neighbor from CLOSED
		    if neighbor not in OPEN and neighbor not in CLOSED:
		      set g(neighbor) to cost
		      add neighbor to OPEN
		      set priority queue rank to g(neighbor) + h(neighbor)
		      set neighbor's parent to current
        */

		/// <summary>
		/// Make a plan of actions that will reach desired world state
		/// </summary>
		/// <param name="ap">Ap.</param>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		/// <param name="storage">Storage.</param>
		public static Stack<Action> Plan(ActionPlanner ap, WorldState start, WorldState goal,
		                                 List<AStarNode> selectedNodes = null)
		{
			storage.Clear();

			var currentNode = Pool<AStarNode>.Obtain();
			currentNode.WorldState = start;
			currentNode.ParentWorldState = start;
			currentNode.CostSoFar = 0; // g
			currentNode.HeuristicCost = CalculateHeuristic(start, goal); // h
			currentNode.CostSoFarAndHeuristicCost = currentNode.CostSoFar + currentNode.HeuristicCost; // f
			currentNode.Depth = 1;

			storage.AddToOpenList(currentNode);

			while (true)
			{
				// nothing left open so we failed to find a path
				if (!storage.HasOpened())
				{
					storage.Clear();
					return null;
				}

				currentNode = storage.RemoveCheapestOpenNode();

				storage.AddToClosedList(currentNode);

				// all done. we reached our goal
				if (goal.Equals(currentNode.WorldState))
				{
					var plan = ReconstructPlan(currentNode, selectedNodes);
					storage.Clear();
					return plan;
				}

				var neighbors = ap.GetPossibleTransitions(currentNode.WorldState);
				for (var i = 0; i < neighbors.Count; i++)
				{
					var cur = neighbors[i];
					var opened = storage.FindOpened(cur);
					var closed = storage.FindClosed(cur);
					var cost = currentNode.CostSoFar + cur.CostSoFar;

					// if neighbor in OPEN and cost less than g(neighbor):
					if (opened != null && cost < opened.CostSoFar)
					{
						// remove neighbor from OPEN, because new path is better
						storage.RemoveOpened(opened);
						opened = null;
					}

					// if neighbor in CLOSED and cost less than g(neighbor):
					if (closed != null && cost < closed.CostSoFar)
					{
						// remove neighbor from CLOSED
						storage.RemoveClosed(closed);
					}

					// if neighbor not in OPEN and neighbor not in CLOSED:
					if (opened == null && closed == null)
					{
						var nb = Pool<AStarNode>.Obtain();
						nb.WorldState = cur.WorldState;
						nb.CostSoFar = cost;
						nb.HeuristicCost = CalculateHeuristic(cur.WorldState, goal);
						nb.CostSoFarAndHeuristicCost = nb.CostSoFar + nb.HeuristicCost;
						nb.Action = cur.Action;
						nb.ParentWorldState = currentNode.WorldState;
						nb.Parent = currentNode;
						nb.Depth = currentNode.Depth + 1;
						storage.AddToOpenList(nb);
					}
				}

				// done with neighbors so release it back to the pool
				ListPool<AStarNode>.Free(neighbors);
			}
		}


		/// <summary>
		/// internal function to reconstruct the plan by tracing from last node to initial node
		/// </summary>
		/// <returns>The plan.</returns>
		/// <param name="goalnode">Goalnode.</param>
		static Stack<Action> ReconstructPlan(AStarNode goalNode, List<AStarNode> selectedNodes)
		{
			var totalActionsInPlan = goalNode.Depth - 1;
			var plan = new Stack<Action>(totalActionsInPlan);

			var curnode = goalNode;
			for (var i = 0; i <= totalActionsInPlan - 1; i++)
			{
				// optionally add the node to the List if we have been passed one
				if (selectedNodes != null)
					selectedNodes.Add(curnode.Clone());
				plan.Push(curnode.Action);
				curnode = curnode.Parent;
			}

			// our nodes went from the goal back to the start so reverse them
			if (selectedNodes != null)
				selectedNodes.Reverse();

			return plan;
		}


		/// <summary>
		/// This is our heuristic: estimate for remaining distance is the nr of mismatched atoms that matter.
		/// </summary>
		/// <returns>The heuristic.</returns>
		/// <param name="fr">Fr.</param>
		/// <param name="to">To.</param>
		static int CalculateHeuristic(WorldState fr, WorldState to)
		{
			long care = (to.DontCare ^ -1L);
			long diff = (fr.Values & care) ^ (to.Values & care);
			int dist = 0;

			for (var i = 0; i < ActionPlanner.MAX_CONDITIONS; ++i)
				if ((diff & (1L << i)) != 0)
					dist++;
			return dist;
		}
	}
}