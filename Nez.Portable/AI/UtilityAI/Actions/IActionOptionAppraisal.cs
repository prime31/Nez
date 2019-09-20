namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Appraisal for use with an ActionWithOptions
	/// </summary>
	public interface IActionOptionAppraisal<T, U>
	{
		float GetScore(T context, U option);
	}
}