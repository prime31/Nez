namespace Nez.Tweens
{
	/// <summary>
	/// any object that wants to be tweened needs to implement this. TweenManager internally likes to make a simple object
	/// that implements this interface and stores a reference to the object being tweened. That makes for tiny, simple,
	/// lightweight implementations that can be handed off to any TweenT
	/// </summary>
	public interface ITweenTarget<T> where T : struct
	{
		/// <summary>
		/// sets the final, tweened value on the object of your choosing.
		/// </summary>
		/// <param name="value">Value.</param>
		void SetTweenedValue(T value);


		T GetTweenedValue();

		/// <summary>
		/// gets the target of the tween or null for TweenTargets that arent necessarily all about a single object.
		/// its only real use is for TweenManager to find a list of tweens by target.
		/// </summary>
		/// <returns>The target object.</returns>
		object GetTargetObject();
	}
}