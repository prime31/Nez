using System.Text;
using System.Collections.Generic;


namespace Nez.AI.GOAP
{
	/// <summary>
	/// GOAP based on https://github.com/stolk/GPGOAP
	/// </summary>
	public class ActionPlanner
	{
		public const int MAX_CONDITIONS = 64;

		/// <summary>
		/// Names associated with all world state atoms
		/// </summary>
		public string[] conditionNames = new string[MAX_CONDITIONS];


		List<Action> _actions = new List<Action>();

		List<Action> _viableActions = new List<Action>();

		/// <summary>
		/// Preconditions for all actions
		/// </summary>
		WorldState[] _preConditions = new WorldState[MAX_CONDITIONS];

		/// <summary>
		/// Postconditions for all actions (action effects).
		/// </summary>
		WorldState[] _postConditions = new WorldState[MAX_CONDITIONS];

		/// <summary>
		/// Number of world state atoms.
		/// </summary>
		int _numConditionNames;


		public ActionPlanner()
		{
			_numConditionNames = 0;
			for( var i = 0; i < MAX_CONDITIONS; ++i )
			{
				conditionNames[i] = null;
				_preConditions[i] = WorldState.create( this );
				_postConditions[i] = WorldState.create( this );
			}
		}


		/// <summary>
		/// convenince method for fetching a WorldState object
		/// </summary>
		/// <returns>The world state.</returns>
		public WorldState createWorldState()
		{
			return WorldState.create( this );
		}


		public void addAction( Action action )
		{
			var actionId = findActionIndex( action );
			if( actionId == -1 )
				throw new KeyNotFoundException( "could not find or create Action" );

			foreach( var preCondition in action._preConditions )
			{
				var conditionId = findConditionNameIndex( preCondition.Item1 );
				if( conditionId == -1 )
					throw new KeyNotFoundException( "could not find or create conditionName" );

				_preConditions[actionId].set( conditionId, preCondition.Item2 );
			}

			foreach( var postCondition in action._postConditions )
			{
				var conditionId = findConditionNameIndex( postCondition.Item1 );
				if( conditionId == -1 )
					throw new KeyNotFoundException( "could not find conditionName" );

				_postConditions[actionId].set( conditionId, postCondition.Item2 );
			}
		}


		public Stack<Action> plan( WorldState startState, WorldState goalState, List<AStarNode> selectedNodes = null )
		{
			_viableActions.Clear();
			for( var i = 0; i < _actions.Count; i++ )
			{
				if( _actions[i].validate() )
					_viableActions.Add( _actions[i] );
			}

			return AStar.plan( this, startState, goalState, selectedNodes );
		}


		/// <summary>
		/// Describe the action planner by listing all actions with pre and post conditions. For debugging purpose.
		/// </summary>
		public string describe()
		{
			var sb = new StringBuilder();
			for( var a = 0; a < _actions.Count; ++a )
			{
				sb.AppendLine( _actions[a].GetType().Name );

				var pre = _preConditions[a];
				var pst = _postConditions[a];
				for( var i = 0; i < MAX_CONDITIONS; ++i )
				{
					if( ( pre.dontCare & ( 1L << i ) ) == 0 )
					{
						bool v = ( pre.values & ( 1L << i ) ) != 0;
						sb.AppendFormat( "  {0}=={1}\n", conditionNames[i], v ? 1 : 0 );
					}
				}

				for( var i = 0; i < MAX_CONDITIONS; ++i )
				{
					if( ( pst.dontCare & ( 1L << i ) ) == 0 )
					{
						bool v = ( pst.values & ( 1L << i ) ) != 0;
						sb.AppendFormat( "  {0}:={1}\n", conditionNames[i], v ? 1 : 0 );
					}
				}
			}

			return sb.ToString();
		}


		internal int findConditionNameIndex( string conditionName )
		{
			int idx;
			for( idx = 0; idx < _numConditionNames; ++idx )
			{
				if( string.Equals( conditionNames[idx], conditionName ) )
					return idx;
			}

			if( idx < MAX_CONDITIONS - 1 )
			{
				conditionNames[idx] = conditionName;
				_numConditionNames++;
				return idx;
			}

			return -1;
		}


		internal int findActionIndex( Action action )
		{
			var idx = _actions.IndexOf( action );
			if( idx > -1 )
				return idx;

			_actions.Add( action );

			return _actions.Count - 1;
		}


		internal List<AStarNode> getPossibleTransitions( WorldState fr )
		{
			var result = ListPool<AStarNode>.obtain();
			for( var i = 0; i < _viableActions.Count; ++i )
			{
				// see if precondition is met
				var pre = _preConditions[i];
				var care = ( pre.dontCare ^ -1L );
				bool met = ( ( pre.values & care ) == ( fr.values & care ) );
				if( met )
				{
					var node = Pool<AStarNode>.obtain();
					node.action = _viableActions[i];
					node.costSoFar = _viableActions[i].cost;
					node.worldState = applyPostConditions( this, i, fr );
					result.Add( node );
				}
			}
			return result;
		}


		internal WorldState applyPostConditions( ActionPlanner ap, int actionnr, WorldState fr )
		{
			var pst = ap._postConditions[actionnr];
			long unaffected = pst.dontCare;
			long affected = ( unaffected ^ -1L );

			fr.values = ( fr.values & unaffected ) | ( pst.values & affected );
			fr.dontCare &= pst.dontCare;
			return fr;
		}

	}
}

