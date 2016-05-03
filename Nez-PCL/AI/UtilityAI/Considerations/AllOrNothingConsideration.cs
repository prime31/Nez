using System;
using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Only scores if all child Appraisals score above the threshold
	/// </summary>
	public class AllOrNothingConsideration<T> : IConsideration<T>
	{
		public float threshold;

		public IAction<T> action { get; set; }

		List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();


		public AllOrNothingConsideration( float threshold = 0 )
		{
			this.threshold = threshold;
		}


		public AllOrNothingConsideration<T> addAppraisal( IAppraisal<T> appraisal )
		{
			_appraisals.Add( appraisal );
			return this;
		}


		float IConsideration<T>.getScore( T context )
		{
			var sum = 0f;
			for( var i = 0; i < _appraisals.Count; i++ )
			{
				var score = _appraisals[i].getScore( context );
				if( score < threshold )
					return 0;
				sum += score;
			}

			return sum;
		}
	}
}

