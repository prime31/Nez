using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// root class for all nodes
	/// </summary>
	public abstract class Behavior<T>
	{
		public TaskStatus status = TaskStatus.Invalid;


		public abstract TaskStatus update( T context );


		/// <summary>
		/// invalidate the status of the node. Composites can override this and invalidate all of their children.
		/// </summary>
		public virtual void invalidate()
		{
			status = TaskStatus.Invalid;
		}


		/// <summary>
		/// called immediately before execution. It is used to setup any variables that need to be reset from the previous run
		/// </summary>
		public virtual void onStart()
		{}


		/// <summary>
		/// called when a task changes state to something other than running
		/// </summary>
		public virtual void onEnd()
		{}


		/// <summary>
		/// tick handles calling through to update where the actual work is done. It exists so that it can call onStart/onEnd when necessary.
		/// </summary>
		/// <param name="context">Context.</param>
		internal TaskStatus tick( T context )
		{
			if( status == TaskStatus.Invalid )
				onStart();

			status = update( context );

			if( status != TaskStatus.Running )
				onEnd();

			return status;
		}

	}
}

