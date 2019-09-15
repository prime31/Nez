namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// Similar to the selector task, the ParallelSelector task will return success as soon as a child task returns success. The difference
	/// is that the parallel task will run all of its children tasks simultaneously versus running each task one at a time. If one tasks returns
	/// success the parallel selector task will end all of the child tasks and return success. If every child task returns failure then the
	/// ParallelSelector task will return failure.
	/// </summary>
	public class ParallelSelector<T> : Composite<T>
	{
		public override TaskStatus Update(T context)
		{
			var didAllFail = true;
			for (var i = 0; i < _children.Count; i++)
			{
				var child = _children[i];
				child.Tick(context);

				// if any child succeeds we return success
				if (child.Status == TaskStatus.Success)
					return TaskStatus.Success;

				// if all children didn't fail, we're not done yet
				if (child.Status != TaskStatus.Failure)
					didAllFail = false;
			}

			if (didAllFail)
				return TaskStatus.Failure;

			return TaskStatus.Running;
		}
	}
}