namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// scorer for use with a Consideration
	/// </summary>
	public interface IAppraisal<T>
	{
		float GetScore(T context);
	}
}