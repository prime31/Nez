using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// The sequence task is similar to an "and" operation. It will return failure as soon as one of its child tasks return failure. If a
	/// child task returns success then it will sequentially run the next task. If all child tasks return success then it will return success.
	/// </summary>
	public class Sequence<T> : Composite<T>
	{
		public Sequence( AbortTypes abortType = AbortTypes.None )
		{
			this.abortType = abortType;
		}


		public override TaskStatus update( T context )
		{
			// first, we handle conditional aborts if we are not already on the first child
			if( _currentChildIndex != 0 )
				handleConditionalAborts( context );
			
			var current = _children[_currentChildIndex];
			var status = current.tick( context );

			// if the child failed or is still running, early return
			if( status != TaskStatus.Success )
				return status;

			_currentChildIndex++;

			// if the end of the children is hit the whole sequence suceeded
			if( _currentChildIndex == _children.Count )
			{
				// reset index for next run
				_currentChildIndex = 0;
				return TaskStatus.Success;
			}

			return TaskStatus.Running;
		}


		void handleConditionalAborts( T context )
		{
			if( _hasLowerPriorityConditionalAbort )
				updateLowerPriorityAbortConditional( context, TaskStatus.Success );

			if( abortType.has( AbortTypes.Self ) )
				updateSelfAbortConditional( context, TaskStatus.Success );
		}

	}
}

