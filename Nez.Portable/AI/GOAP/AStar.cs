using System;
using System.Collections.Generic;


namespace Nez.AI.GOAP
{
	public class AStarNode : IComparable<AStarNode>, IEquatable<AStarNode>, IPoolable
	{
		/// <summary>
		/// The state of the world at this node.
		/// </summary>
		public WorldState worldState;

		/// <summary>
		/// The cost so far.
		/// </summary>
		public int costSoFar;

		/// <summary>
		/// The heuristic for remaining cost (don't overestimate!)
		/// </summary>
		public int heuristicCost;

		/// <summary>
		/// costSoFar + heuristicCost (g+h) combined.
		/// </summary>
		public int costSoFarAndHeuristicCost;

		/// <summary>
		/// the Action associated with this node
		/// </summary>
		public Action action;

		// Where did we come from?
		public AStarNode parent;
		public WorldState parentWorldState;
		public int depth;


		#region IEquatable and IComparable

		public bool Equals( AStarNode other )
		{
			long care = worldState.dontCare ^ -1L;
			return ( worldState.values & care ) == ( other.worldState.values & care );
		}


		public int CompareTo( AStarNode other )
		{
			return this.costSoFarAndHeuristicCost.CompareTo( other.costSoFarAndHeuristicCost );
		}

		#endregion


		public void reset()
		{
			action = null;
			parent = null;
		}


		public AStarNode clone()
		{
			return (AStarNode)MemberwiseClone();
		}


		public override string ToString()
		{
			return string.Format( "[cost: {0} | heuristic: {1}]: {2}", costSoFar, heuristicCost, action );
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
		public static Stack<Action> plan( ActionPlanner ap, WorldState start, WorldState goal, List<AStarNode> selectedNodes = null )
		{
			storage.clear();

			var currentNode = Pool<AStarNode>.obtain();
			currentNode.worldState = start;
			currentNode.parentWorldState = start;
			currentNode.costSoFar = 0; // g
			currentNode.heuristicCost = calculateHeuristic( start, goal ); // h
			currentNode.costSoFarAndHeuristicCost = currentNode.costSoFar + currentNode.heuristicCost; // f
			currentNode.depth = 1;

			storage.addToOpenList( currentNode );

			while( true )
			{
				// nothing left open so we failed to find a path
				if( !storage.hasOpened() )
				{
					storage.clear();
					return null;
				}

				currentNode = storage.removeCheapestOpenNode();

				storage.addToClosedList( currentNode );

				// all done. we reached our goal
				if( goal.Equals( currentNode.worldState ) )
				{
					var plan = reconstructPlan( currentNode, selectedNodes );
					storage.clear();
					return plan;
				}

				var neighbors = ap.getPossibleTransitions( currentNode.worldState );
				for( var i = 0; i < neighbors.Count; i++ )
				{
					var cur = neighbors[i];
					var opened = storage.findOpened( cur );
					var closed = storage.findClosed( cur );
					var cost = currentNode.costSoFar + cur.costSoFar;

					// if neighbor in OPEN and cost less than g(neighbor):
					if( opened != null && cost < opened.costSoFar )
					{
						// remove neighbor from OPEN, because new path is better
						storage.removeOpened( opened );
						opened = null;
					}

					// if neighbor in CLOSED and cost less than g(neighbor):
					if( closed != null && cost < closed.costSoFar )
					{
						// remove neighbor from CLOSED
						storage.removeClosed( closed );
					}

					// if neighbor not in OPEN and neighbor not in CLOSED:
					if( opened == null && closed == null )
					{
						var nb = Pool<AStarNode>.obtain();
						nb.worldState = cur.worldState;
						nb.costSoFar = cost;
						nb.heuristicCost = calculateHeuristic( cur.worldState, goal );
						nb.costSoFarAndHeuristicCost = nb.costSoFar + nb.heuristicCost;
						nb.action = cur.action;
						nb.parentWorldState = currentNode.worldState;
						nb.parent = currentNode;
						nb.depth = currentNode.depth + 1;
						storage.addToOpenList( nb );
					}
				}

				// done with neighbors so release it back to the pool
				ListPool<AStarNode>.free( neighbors );
			}
		}


		/// <summary>
		/// internal function to reconstruct the plan by tracing from last node to initial node
		/// </summary>
		/// <returns>The plan.</returns>
		/// <param name="goalnode">Goalnode.</param>
		static Stack<Action> reconstructPlan( AStarNode goalNode, List<AStarNode> selectedNodes )
		{
			var totalActionsInPlan = goalNode.depth - 1;
			var plan = new Stack<Action>( totalActionsInPlan );

			var curnode = goalNode;
			for( var i = 0; i <= totalActionsInPlan - 1; i++ )
			{
				// optionally add the node to the List if we have been passed one
				if( selectedNodes != null )
					selectedNodes.Add( curnode.clone() );
				plan.Push( curnode.action );
				curnode = curnode.parent;
			}

			// our nodes went from the goal back to the start so reverse them
			if( selectedNodes != null )
				selectedNodes.Reverse();

			return plan;
		}


		/// <summary>
		/// This is our heuristic: estimate for remaining distance is the nr of mismatched atoms that matter.
		/// </summary>
		/// <returns>The heuristic.</returns>
		/// <param name="fr">Fr.</param>
		/// <param name="to">To.</param>
		static int calculateHeuristic( WorldState fr, WorldState to )
		{
			long care = ( to.dontCare ^ -1L );
			long diff = ( fr.values & care ) ^ ( to.values & care );
			int dist = 0;

			for( var i = 0; i < ActionPlanner.MAX_CONDITIONS; ++i )
				if( ( diff & ( 1L << i ) ) != 0 )
					dist++;
			return dist;
		}

	}
}

