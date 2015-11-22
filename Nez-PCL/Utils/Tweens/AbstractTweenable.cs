using System.Collections;
using System;


namespace Nez.Tweens
{
	/// <summary>
	/// AbstractTweenable serves as a base for any custom classes you might want to make that can be ticked. These differ from
	/// ITweens in that they dont implement the ITweenT interface. What does that mean? It just says that an AbstractTweenable
	/// is not just moving a value from start to finish. It can do anything at all that requires a tick each frame.
	/// 
	/// The TweenChain is one example of AbstractTweenable for reference.
	/// </summary>
	public abstract class AbstractTweenable : ITweenable
	{
		protected bool _isPaused;

		/// <summary>
		/// AbstractTweenable are often kept around after they complete. This flag lets them know internally if they are currently
		/// being tweened by TweenManager so that they can re-add themselves if necessary.
		/// </summary>
		protected bool _isCurrentlyManagedByTweenManager;


		#region ITweenable

		public abstract bool tick();


		public virtual void recycleSelf()
		{}


		public bool isRunning()
		{
			return _isCurrentlyManagedByTweenManager && !_isPaused;
		}


		public virtual void start()
		{
			// dont add ourself twice!
			if( _isCurrentlyManagedByTweenManager )
			{
				_isPaused = false;
				return;
			}
			
			TweenManager.addTween( this );
			_isCurrentlyManagedByTweenManager = true;
			_isPaused = false;
		}


		public void pause()
		{
			_isPaused = true;
		}


		public void resume()
		{
			_isPaused = false;
		}


		public virtual void stop( bool bringToCompletion = false )
		{
			TweenManager.removeTween( this );
			_isCurrentlyManagedByTweenManager = false;
			_isPaused = true;
		}

		#endregion

	}
}