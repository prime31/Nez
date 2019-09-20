using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Scores by summing child Appraisals until a child scores below the threshold
	/// </summary>
	public class ThresholdConsideration<T> : IConsideration<T>
	{
		public float Threshold;

		public IAction<T> Action { get; set; }

		List<IAppraisal<T>> _appraisals = new List<IAppraisal<T>>();


		public ThresholdConsideration(float threshold)
		{
			Threshold = threshold;
		}


		float IConsideration<T>.GetScore(T context)
		{
			var sum = 0f;
			for (var i = 0; i < _appraisals.Count; i++)
			{
				var score = _appraisals[i].GetScore(context);
				if (score < Threshold)
					return sum;

				sum += score;
			}

			return sum;
		}
	}
}