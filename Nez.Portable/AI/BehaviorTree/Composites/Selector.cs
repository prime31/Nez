namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// The selector task is similar to an "or" operation. It will return success as soon as one of its child tasks return success. If a
	/// child task returns failure then it will sequentially run the next task. If no child task returns success then it will return failure.
	/// </summary>
	public class Selector<T> : Composite<T>
	{
		public Selector(AbortTypes abortType = AbortTypes.None)
		{
			AbortType = abortType;
		}


		public override TaskStatus Update(T context)
		{
			// first, we handle conditinoal aborts if we are not already on the first child
			if (_currentChildIndex != 0)
				HandleConditionalAborts(context);

			var current = _children[_currentChildIndex];
			var status = current.Tick(context);

			// if the child succeeds or is still running, early return.
			if (status != TaskStatus.Failure)
				return status;

			_currentChildIndex++;

			// if the end of the children is hit, that means the whole thing fails.
			if (_currentChildIndex == _children.Count)
			{
				// reset index otherwise it will crash on next run through
				_currentChildIndex = 0;
				return TaskStatus.Failure;
			}

			return TaskStatus.Running;
		}


		void HandleConditionalAborts(T context)
		{
			// check any lower priority tasks to see if they changed to a success
			if (_hasLowerPriorityConditionalAbort)
				UpdateLowerPriorityAbortConditional(context, TaskStatus.Failure);

			if (AbortType.Has(AbortTypes.Self))
				UpdateSelfAbortConditional(context, TaskStatus.Failure);
		}
	}
}