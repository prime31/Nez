using System;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// always returns a fixed score. Serves double duty as a default Consideration.
	/// </summary>
	public class FixedScoreConsideration<T> : IConsideration<T>
	{
		public float score;

		public IAction<T> action { get; set; }


		public FixedScoreConsideration( float score = 1 )
		{
			this.score = score;
		}


		float IConsideration<T>.getScore( T context )
		{
			return score;
		}

	}
}

