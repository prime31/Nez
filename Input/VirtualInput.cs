using System;


namespace Nez
{
	/// <summary>
	/// Represents a virtual button, axis or joystick whose state is determined by the state of its VirtualInputNodes
	/// </summary>
	public abstract class VirtualInput
	{
		public enum OverlapBehavior { CancelOut, TakeOlder, TakeNewer };


		public VirtualInput()
		{
			Input._virtualInputs.Add( this );
		}


		public void deregister()
		{
			Input._virtualInputs.Remove( this );
		}

		public abstract void update();
	}


	/// <summary>
	/// Add these to your VirtualInput to define how it determines its current input state. 
	/// For example, if you want to check whether a keyboard key is pressed, create a VirtualButton and add to it a VirtualButton.KeyboardKey
	/// </summary>
	public abstract class VirtualInputNode
	{
		public virtual void update()
		{}
	}
}

