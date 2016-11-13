using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// Wait a specified amount of time. The task will return running until the task is done waiting. It will return success after the wait
	/// time has elapsed.
	/// </summary>
	public class WaitAction<T> : Behavior<T>
	{
		/// <summary>
		/// the amount of time to wait
		/// </summary>
		public float waitTime;

		float _startTime;


		public WaitAction( float waitTime )
		{
			this.waitTime = waitTime;
		}


		public override void onStart()
		{
			_startTime = 0;
		}


		public override TaskStatus update( T context )
		{
			// we cant use Time.deltaTime due to the tree ticking at its own rate so we store the start time instead
			if( _startTime == 0 )
				_startTime = Time.time;

			if( Time.time - _startTime >= waitTime )
				return TaskStatus.Success;

			return TaskStatus.Running;
		}
	}
}

