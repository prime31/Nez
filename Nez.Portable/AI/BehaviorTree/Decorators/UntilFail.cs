namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// will keep executing its child task until the child task returns failure
	/// </summary>
	public class UntilFail<T> : Decorator<T>
	{
		public override TaskStatus Update(T context)
		{
			Insist.IsNotNull(Child, "child must not be null");

			var status = Child.Update(context);

			if (status != TaskStatus.Failure)
				return TaskStatus.Running;

			return TaskStatus.Success;
		}
	}
}