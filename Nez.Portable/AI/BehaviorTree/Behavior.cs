namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// root class for all nodes
	/// </summary>
	public abstract class Behavior<T>
	{
		public TaskStatus Status = TaskStatus.Invalid;


		public abstract TaskStatus Update(T context);


		/// <summary>
		/// invalidate the status of the node. Composites can override this and invalidate all of their children.
		/// </summary>
		public virtual void Invalidate()
		{
			Status = TaskStatus.Invalid;
		}


		/// <summary>
		/// called immediately before execution. It is used to setup any variables that need to be reset from the previous run
		/// </summary>
		public virtual void OnStart()
		{
		}


		/// <summary>
		/// called when a task changes state to something other than running
		/// </summary>
		public virtual void OnEnd()
		{
		}


		/// <summary>
		/// tick handles calling through to update where the actual work is done. It exists so that it can call onStart/onEnd when necessary.
		/// </summary>
		/// <param name="context">Context.</param>
		internal TaskStatus Tick(T context)
		{
			if (Status == TaskStatus.Invalid)
				OnStart();

			Status = Update(context);

			if (Status != TaskStatus.Running)
				OnEnd();

			return Status;
		}
	}
}