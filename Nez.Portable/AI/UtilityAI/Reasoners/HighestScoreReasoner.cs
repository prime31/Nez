using System;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// The Consideration with the highest score is selected
	/// </summary>
	public class HighestScoreReasoner<T> : Reasoner<T>
	{
		protected override IConsideration<T> selectBestConsideration( T context )
		{
			var highestScore = defaultConsideration.getScore( context );
			IConsideration<T> consideration = null;
			for( var i = 0; i < _considerations.Count; i++ )
			{
				var score = _considerations[i].getScore( context );
				if( score > highestScore )
				{
					highestScore = score;
					consideration = _considerations[i];
				}
			}

			if( consideration == null )
				return defaultConsideration;

			return consideration;
		}
	}
}

