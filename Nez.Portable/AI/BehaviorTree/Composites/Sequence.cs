namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// The sequence task is similar to an "and" operation. It will return failure as soon as one of its child tasks return failure. If a
	/// child task returns success then it will sequentially run the next task. If all child tasks return success then it will return success.
	/// </summary>
	public class Sequence<T> : Composite<T>
	{
		public Sequence(AbortTypes abortType = AbortTypes.None)
		{
			AbortType = abortType;
		}


		public override TaskStatus Update(T context)
		{
			// first, we handle conditional aborts if we are not already on the first child
			if (_currentChildIndex != 0)
				HandleConditionalAborts(context);

			var current = _children[_currentChildIndex];
			var status = current.Tick(context);

			// if the child failed or is still running, early return
			if (status != TaskStatus.Success)
				return status;

			_currentChildIndex++;

			// if the end of the children is hit the whole sequence suceeded
			if (_currentChildIndex == _children.Count)
			{
				// reset index for next run
				_currentChildIndex = 0;
				return TaskStatus.Success;
			}

			return TaskStatus.Running;
		}


		void HandleConditionalAborts(T context)
		{
			if (_hasLowerPriorityConditionalAbort)
				UpdateLowerPriorityAbortConditional(context, TaskStatus.Success);

			if (AbortType.Has(AbortTypes.Self))
				UpdateSelfAbortConditional(context, TaskStatus.Success);
		}
	}
}