using System;
using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Action that encompasses a List of options. The options are passed to Appraisals which score and locate the best option.
	/// </summary>
	public abstract class ActionWithOptions<T,U> : IAction<T>
	{
		protected List<IActionOptionAppraisal<T,U>> _appraisals = new List<IActionOptionAppraisal<T,U>>();


		public U getBestOption( T context, List<U> options )
		{
			var result = default(U);
			var bestScore = float.MinValue;

			for( var i = 0; i < options.Count; i++ )
			{
				var option = options[i];
				var current = 0f;
				for( var j = 0; j < _appraisals.Count; j++ )
					current += _appraisals[j].getScore( context, option );

				if( current > bestScore )
				{
					bestScore = current;
					result = option;
				}
			}

			return result;
		}

		public abstract void execute( T context );


		public ActionWithOptions<T,U> addScorer( IActionOptionAppraisal<T,U> scorer )
		{
			_appraisals.Add( scorer );
			return this;
		}

	}
}

