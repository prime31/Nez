using System;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// wraps a Func for use as an Appraisal without having to create a subclass
	/// </summary>
	public class ExecuteActionAppraisal<T> : IAppraisal<T>
	{
		Func<T,float> _appraisalAction;


		public ExecuteActionAppraisal( Func<T,float> appraisalAction )
		{
			_appraisalAction = appraisalAction;
		}


		float IAppraisal<T>.getScore( T context )
		{
			return _appraisalAction( context );
		}
	}
}

