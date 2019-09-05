using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// wraps a Func so that you can avoid having to subclass to create new actions
	/// </summary>
	public class ExecuteAction<T> : Behavior<T>
	{
		Func<T, TaskStatus> _action;


		public ExecuteAction(Func<T, TaskStatus> action)
		{
			_action = action;
		}


		public override TaskStatus Update(T context)
		{
			Insist.IsNotNull(_action, "action must not be null");

			return _action(context);
		}
	}
}