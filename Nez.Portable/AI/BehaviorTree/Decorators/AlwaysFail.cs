namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// will always return failure except when the child task is running
	/// </summary>
	public class AlwaysFail<T> : Decorator<T>
	{
		public override TaskStatus Update(T context)
		{
			Insist.IsNotNull(Child, "child must not be null");

			var status = Child.Update(context);

			if (status == TaskStatus.Running)
				return TaskStatus.Running;

			return TaskStatus.Failure;
		}
	}
}