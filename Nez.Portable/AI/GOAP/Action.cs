using System;
using System.Collections.Generic;


namespace Nez.AI.GOAP
{
	public class Action
	{
		/// <summary>
		/// optional name for the Action. Used for debugging purposes
		/// </summary>
		public string Name;

		/// <summary>
		/// The cost of performing the action.  Figure out a weight that suits the action.  Changing it will affect what actions are
		/// chosen during planning
		/// </summary>
		public int Cost = 1;


		internal HashSet<Tuple<string, bool>> _preConditions = new HashSet<Tuple<string, bool>>();

		internal HashSet<Tuple<string, bool>> _postConditions = new HashSet<Tuple<string, bool>>();


		public Action()
		{
		}


		public Action(string name)
		{
			Name = name;
		}


		public Action(string name, int cost) : this(name)
		{
			Cost = cost;
		}


		public void SetPrecondition(string conditionName, bool value)
		{
			_preConditions.Add(new Tuple<string, bool>(conditionName, value));
		}


		public void SetPostcondition(string conditionName, bool value)
		{
			_postConditions.Add(new Tuple<string, bool>(conditionName, value));
		}


		/// <summary>
		/// called before the Planner does its planning. Gives the Action an opportunity to set its score or to opt out if it isnt of use.
		/// For example, if the Action is to pick up a gun but there are no guns in the world returning false would keep the Action from being
		/// considered by the ActionPlanner.
		/// </summary>
		public virtual bool Validate()
		{
			return true;
		}


		public override string ToString()
		{
			return string.Format("[Action] {0} - cost: {1}", Name, Cost);
		}
	}
}