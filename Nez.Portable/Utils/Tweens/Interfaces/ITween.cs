using System;


namespace Nez.Tweens
{
	/// <summary>
	/// a series of strongly typed, chainable methods to setup various tween properties
	/// </summary>
	public interface ITween<T> : ITweenControl where T : struct
	{
		/// <summary>
		/// sets the ease type used for this tween
		/// </summary>
		/// <returns>The ease type.</returns>
		/// <param name="easeType">Ease type.</param>
		ITween<T> SetEaseType(EaseType easeType);

		/// <summary>
		/// sets the delay before starting the tween
		/// </summary>
		/// <returns>The delay.</returns>
		/// <param name="delay">Delay.</param>
		ITween<T> SetDelay(float delay);

		/// <summary>
		/// sets the tween duration
		/// </summary>
		/// <returns>The duration.</returns>
		/// <param name="duration">Duration.</param>
		ITween<T> SetDuration(float duration);

		/// <summary>
		/// sets the timeScale used for this tween. The timeScale will be multiplied with Time.deltaTime/Time.unscaledDeltaTime
		/// to get the actual delta time used for the tween.
		/// </summary>
		/// <returns>The time scale.</returns>
		/// <param name="timeScale">Time scale.</param>
		ITween<T> SetTimeScale(float timeScale);

		/// <summary>
		/// sets the tween to use Time.unscaledDeltaTime instead of Time.deltaTime
		/// </summary>
		/// <returns>The is time scale independant.</returns>
		ITween<T> SetIsTimeScaleIndependent();

		/// <summary>
		/// chainable. sets the action that should be called when the tween is complete.
		/// </summary>
		ITween<T> SetCompletionHandler(Action<ITween<T>> completionHandler);

		/// <summary>
		/// chainable. set the loop type for the tween. a single pingpong loop means going from start-finish-start.
		/// </summary>
		ITween<T> SetLoops(LoopType loopType, int loops = 1, float delayBetweenLoops = 0f);

		/// <summary>
		/// chainable. sets the action that should be called when a loop is complete. A loop is either when the first part of
		/// a ping-pong animation completes or when starting over when using a restart-from-beginning loop type. Note that ping-pong
		/// loops (which are really two part tweens) will not fire the loop completion handler on the last iteration. The normal
		/// tween completion handler will fire though
		/// </summary>
		ITween<T> SetLoopCompletionHandler(Action<ITween<T>> loopCompleteHandler);

		/// <summary>
		/// sets the start position for the tween
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="from">From.</param>
		ITween<T> SetFrom(T from);

		/// <summary>
		/// prepares a tween for reuse by resetting its from/to values and duration
		/// </summary>
		/// <returns>The for reuse.</returns>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		ITween<T> PrepareForReuse(T from, T to, float duration);

		/// <summary>
		/// if true (the default) the tween will be recycled after use. All Tween<T> subclasses have their own associated automatic
		/// caching if configured in the TweenManager class.
		/// </summary>
		/// <returns>The recycle tween.</returns>
		/// <param name="shouldRecycleTween">If set to <c>true</c> should recycle tween.</param>
		ITween<T> SetRecycleTween(bool shouldRecycleTween);

		/// <summary>
		/// helper that just sets the to value of the tween to be to + from making the tween relative
		/// to its current value.
		/// </summary>
		/// <returns>The is relative tween.</returns>
		ITween<T> SetIsRelative();

		/// <summary>
		/// allows you to set any object reference retrievable via tween.context. This is handy for avoiding
		/// closure allocations for completion handler Actions. You can also search TweenManager for all tweens with a specific
		/// context.
		/// </summary>
		/// <returns>The context.</returns>
		/// <param name="context">Context.</param>
		ITween<T> SetContext(object context);

		/// <summary>
		/// allows you to add a tween that will get run after this tween completes. Note that nextTween must be an ITweenable!
		/// Also note that all ITweenTs are ITweenable.
		/// </summary>
		/// <returns>The next tween.</returns>
		/// <param name="nextTween">Next tween.</param>
		ITween<T> SetNextTween(ITweenable nextTween);
	}
}