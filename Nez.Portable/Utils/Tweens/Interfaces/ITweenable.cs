using System;


namespace Nez.Tweens
{

	public interface ITweenable
	{
		/// <summary>
		/// called by TweenManager each frame like an internal Update
		/// </summary>
		bool tick();

		/// <summary>
		/// called by TweenManager when a tween is removed. Subclasses can optionally recycle themself. Subclasses
		/// should first check the _shouldRecycleTween bool in their implementation!
		/// </summary>
		void recycleSelf();

		/// <summary>
		/// checks to see if a tween is running
		/// </summary>
		/// <returns><c>true</c>, if running was ised, <c>false</c> otherwise.</returns>
		bool isRunning();

		/// <summary>
		/// starts the tween
		/// </summary>
		void start();

		/// <summary>
		/// pauses the tween
		/// </summary>
		void pause();

		/// <summary>
		/// resumes the tween after a pause
		/// </summary>
		void resume();

		/// <summary>
		/// stops the tween optionally bringing it to completion
		/// </summary>
		/// <param name="bringToCompletion">If set to <c>true</c> bring to completion.</param>
		void stop( bool bringToCompletion = false );
	}

}
