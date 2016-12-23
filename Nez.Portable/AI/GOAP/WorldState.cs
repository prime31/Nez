using System;
using System.Text;


namespace Nez.AI.GOAP
{
	public struct WorldState : IEquatable<WorldState>
	{
		/// <summary>
		/// we use a bitmask shifting on the condition index to flip bits
		/// </summary>
		public long values;

		/// <summary>
		/// bitmask used to explicitly state false. We need a separate store for negatives because the absense of a value doesnt necessarily mean
		/// it is false.
		/// </summary>
		public long dontCare;

		/// <summary>
		/// required so that we can get the condition index from the string name
		/// </summary>
		internal ActionPlanner planner;


		public static WorldState create( ActionPlanner planner )
		{
			return new WorldState( planner, 0, -1 );
		}


		public WorldState( ActionPlanner planner, long values, long dontcare )
		{
			this.planner = planner;
			this.values = values;
			this.dontCare = dontcare;
		}


		public bool set( string conditionName, bool value )
		{
			return set( planner.findConditionNameIndex( conditionName ), value );
		}


		internal bool set( int conditionId, bool value )
		{
			values = value ? ( values | ( 1L << conditionId ) ) : ( values & ~( 1L << conditionId ) );
			dontCare ^= ( 1 << conditionId );
			return true;
		}


		public bool Equals( WorldState other )
		{
			var care = dontCare ^ -1L;
			return ( values & care ) == ( other.values & care );
		}


		/// <summary>
		/// for debugging purposes. Provides a human readable string of all the preconditions.
		/// </summary>
		/// <param name="planner">Planner.</param>
		public string describe( ActionPlanner planner )
		{
			var sb = new StringBuilder();
			for( var i = 0; i < ActionPlanner.MAX_CONDITIONS; i++ )
			{
				if( ( dontCare & ( 1L << i ) ) == 0 )
				{
					var val = planner.conditionNames[i];
					if( val == null )
						continue;

					bool set = ( ( values & ( 1L << i ) ) != 0L );

					if( sb.Length > 0 )
						sb.Append( ", " );
					sb.Append( set ? val.ToUpper() : val );
				}
			}
			return sb.ToString();
		}

	}
}

