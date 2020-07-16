namespace Nez
{
	/// <summary>
	/// Represents a virtual button, axis or joystick whose state is determined by the state of its VirtualInputNodes
	/// </summary>
	public abstract class VirtualInput
	{
		public enum OverlapBehavior
		{
			/// <summary>
			/// duplicate input will result in canceling each other out and no input will be recorded. Example: press left arrow key and while
			/// holding it down press right arrow. This will result in canceling each other out.
			/// </summary>
			CancelOut,

			/// <summary>
			/// the first input found will be used
			/// </summary>
			TakeOlder,

			/// <summary>
			/// the last input found will be used
			/// </summary>
			TakeNewer
		};


		protected VirtualInput()
		{
			Input._virtualInputs.Add(this);
		}


		/// <summary>
		/// deregisters the VirtualInput from the Input system. Call this when you are done polling the VirtualInput
		/// </summary>
		public void Deregister()
		{
			Input._virtualInputs.Remove(this);
		}

		public abstract void Update();
	}


	/// <summary>
	/// Add these to your VirtualInput to define how it determines its current input state. 
	/// For example, if you want to check whether a keyboard key is pressed, create a VirtualButton and add to it a VirtualButton.KeyboardKey
	/// </summary>
	public abstract class VirtualInputNode
	{
		public virtual void Update()
		{
		}
	}
}