using System;


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
		public int count;

		/// <summary>
		/// Allows the repeater to repeat forever
		/// </summary>
		public bool repeatForever;

		/// <summary>
		/// Should the task return if the child task returns a failure
		/// </summary>
		public bool endOnFailure;

		int _iterationCount;


		public Repeater( int count, bool endOnFailure = false )
		{
			this.count = count;
			this.endOnFailure = endOnFailure;
		}


		public Repeater( bool repeatForever, bool endOnFailure = false )
		{
			this.repeatForever = repeatForever;
			this.endOnFailure = endOnFailure;
		}


		public override void onStart()
		{
			_iterationCount = 0;
		}
	

		public override TaskStatus update( T context )
		{
			Assert.isNotNull( child, "child must not be null" );

			// early out if we are done. we check here and after running just in case the count is 0
			if( !repeatForever && _iterationCount == count )
				return TaskStatus.Success;
			
			var status = child.tick( context );
			_iterationCount++;

			if( endOnFailure && status == TaskStatus.Failure )
				return TaskStatus.Success;

			if( !repeatForever && _iterationCount == count )
				return TaskStatus.Success;

			return TaskStatus.Running;
		}
	}
}

