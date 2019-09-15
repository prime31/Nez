namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// always returns a fixed score. Serves double duty as a default Consideration.
	/// </summary>
	public class FixedScoreConsideration<T> : IConsideration<T>
	{
		public float Score;

		public IAction<T> Action { get; set; }


		public FixedScoreConsideration(float score = 1)
		{
			Score = score;
		}


		float IConsideration<T>.GetScore(T context)
		{
			return Score;
		}
	}
}