using System;
using System.Collections.Generic;


namespace Nez.AI.GOAP
{
	/// <summary>
	/// Agent provides a simple and concise way to use the planner. It is not necessary to use at all since it is just a convenince wrapper
	/// around the ActionPlanner making it easier to get plans and store the results.
	/// </summary>
	public abstract class Agent
	{
		public Stack<Action> actions;
		protected ActionPlanner _planner;


		public Agent()
		{
			_planner = new ActionPlanner();
		}


		public bool plan( bool debugPlan = false )
		{
			List<AStarNode> nodes = null;
			if( debugPlan )
				nodes = new List<AStarNode>();
			
			actions = _planner.plan( getWorldState(), getGoalState(), nodes );

			if( nodes != null && nodes.Count > 0 )
			{
				Debug.log( "---- ActionPlanner plan ----" );
				Debug.log( "plan cost = {0}\n", nodes[nodes.Count - 1].costSoFar );
				Debug.log( "{0}\t{1}", "start".PadRight( 15 ), getWorldState().describe( _planner ) );
				for( var i = 0; i < nodes.Count; i++ )
				{
					Debug.log( "{0}: {1}\t{2}", i, nodes[i].action.GetType().Name.PadRight( 15 ), nodes[i].worldState.describe( _planner ) );
					Pool<AStarNode>.free( nodes[i] );
				}
			}

			return hasActionPlan();
		}


		public bool hasActionPlan()
		{
			return actions != null && actions.Count > 0;
		}


		/// <summary>
		/// current WorldState
		/// </summary>
		/// <returns>The world state.</returns>
		abstract public WorldState getWorldState();


		/// <summary>
		/// the goal state that the agent wants to achieve
		/// </summary>
		/// <returns>The goal state.</returns>
		abstract public WorldState getGoalState();

	}
}

