using System;
using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Scores by summing child Appraisals until a child scores below the threshold
	/// </summary>
	public class ThresholdConsideration<T> : IConsideration<T>
	{
		public float threshold;

		public IAction<T> action { get; set; }

		List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();


		public ThresholdConsideration( float threshold )
		{
			this.threshold = threshold;
		}


		float IConsideration<T>.getScore( T context )
		{
			var sum = 0f;
			for( var i = 0; i < _appraisals.Count; i++ )
			{
				var score = _appraisals[i].getScore( context );
				if( score < threshold )
					return sum;
				sum += score;
			}

			return sum;
		}
	}
}

