namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// will repeat execution of its child task until the child task has been run a specified number of times. It has the option of
	/// continuing to execute the child task even if the child task returns a failure.
	/// </summary>
	public class Repeater<T> : Decorator<T>
	{
		/// <summary>
		/// The number of times to repeat the execution of its child task
		/// </summary>
		public int Count;

		/// <summary>
		/// Allows the repeater to repeat forever
		/// </summary>
		public bool RepeatForever;

		/// <summary>
		/// Should the task return if the child task returns a failure
		/// </summary>
		public bool EndOnFailure;

		int _iterationCount;


		public Repeater(int count, bool endOnFailure = false)
		{
			Count = count;
			EndOnFailure = endOnFailure;
		}


		public Repeater(bool repeatForever, bool endOnFailure = false)
		{
			RepeatForever = repeatForever;
			EndOnFailure = endOnFailure;
		}


		public override void OnStart()
		{
			_iterationCount = 0;
		}


		public override TaskStatus Update(T context)
		{
			Insist.IsNotNull(Child, "child must not be null");

			// early out if we are done. we check here and after running just in case the count is 0
			if (!RepeatForever && _iterationCount == Count)
				return TaskStatus.Success;

			var status = Child.Tick(context);
			_iterationCount++;

			if (EndOnFailure && status == TaskStatus.Failure)
				return TaskStatus.Success;

			if (!RepeatForever && _iterationCount == Count)
				return TaskStatus.Success;

			return TaskStatus.Running;
		}
	}
}