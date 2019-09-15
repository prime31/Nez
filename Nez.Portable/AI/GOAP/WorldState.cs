using System;
using System.Text;


namespace Nez.AI.GOAP
{
	public struct WorldState : IEquatable<WorldState>
	{
		/// <summary>
		/// we use a bitmask shifting on the condition index to flip bits
		/// </summary>
		public long Values;

		/// <summary>
		/// bitmask used to explicitly state false. We need a separate store for negatives because the absense of a value doesnt necessarily mean
		/// it is false.
		/// </summary>
		public long DontCare;

		/// <summary>
		/// required so that we can get the condition index from the string name
		/// </summary>
		internal ActionPlanner planner;


		public static WorldState Create(ActionPlanner planner)
		{
			return new WorldState(planner, 0, -1);
		}


		public WorldState(ActionPlanner planner, long values, long dontcare)
		{
			this.planner = planner;
			Values = values;
			DontCare = dontcare;
		}


		public bool Set(string conditionName, bool value)
		{
			return Set(planner.FindConditionNameIndex(conditionName), value);
		}


		internal bool Set(int conditionId, bool value)
		{
			Values = value ? (Values | (1L << conditionId)) : (Values & ~(1L << conditionId));
			DontCare ^= (1 << conditionId);
			return true;
		}


		public bool Equals(WorldState other)
		{
			var care = DontCare ^ -1L;
			return (Values & care) == (other.Values & care);
		}


		/// <summary>
		/// for debugging purposes. Provides a human readable string of all the preconditions.
		/// </summary>
		/// <param name="planner">Planner.</param>
		public string Describe(ActionPlanner planner)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < ActionPlanner.MAX_CONDITIONS; i++)
			{
				if ((DontCare & (1L << i)) == 0)
				{
					var val = planner.ConditionNames[i];
					if (val == null)
						continue;

					bool set = ((Values & (1L << i)) != 0L);

					if (sb.Length > 0)
						sb.Append(", ");
					sb.Append(set ? val.ToUpper() : val);
				}
			}

			return sb.ToString();
		}
	}
}