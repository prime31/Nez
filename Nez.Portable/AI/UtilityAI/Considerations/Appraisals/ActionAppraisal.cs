using System;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// wraps a Func for use as an Appraisal without having to create a subclass
	/// </summary>
	public class ActionAppraisal<T> : IAppraisal<T>
	{
		Func<T, float> _appraisalAction;


		public ActionAppraisal(Func<T, float> appraisalAction)
		{
			_appraisalAction = appraisalAction;
		}


		float IAppraisal<T>.GetScore(T context)
		{
			return _appraisalAction(context);
		}
	}
}