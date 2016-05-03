using System;
using System.Collections.Generic;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// the root of UtilityAI.
	/// </summary>
	public abstract class Reasoner<T>
	{
		public IConsideration<T> defaultConsideration = new FixedScoreConsideration<T>();

		protected List<IConsideration<T>> _considerations = new List<IConsideration<T>>();


		public IAction<T> select( T context )
		{
			var consideration = selectBestConsideration( context );
			if( consideration != null )
				return consideration.action;
			
			return null;
		}


		protected abstract IConsideration<T> selectBestConsideration( T context );


		public Reasoner<T> addConsideration( IConsideration<T> consideration )
		{
			_considerations.Add( consideration );
			return this;
		}


		public Reasoner<T> setDefaultConsideration( IConsideration<T> defaultConsideration )
		{
			this.defaultConsideration = defaultConsideration;
			return this;
		}

	}
}

