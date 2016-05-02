using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// wraps an ExecuteAction so that it can be used as a Conditional
	/// </summary>
	public class ExecuteActionConditional<T> : ExecuteAction<T>, IConditional<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="action">Action.</param>
		public ExecuteActionConditional( Func<T,TaskStatus> action ) : base( action )
		{}
	}
}

