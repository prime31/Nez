using System.Collections.Generic;


namespace Nez
{
	public enum CoreEvents
	{
		/// <summary>
		/// fired when the graphics device resets. When this happens, any RenderTargets or other contents of VRAM will be wiped and need
		/// to be regenerated
		/// </summary>
		GraphicsDeviceReset,

		/// <summary>
		/// fired when the scene changes
		/// </summary>
		SceneChanged,

		/// <summary>
		/// fired when the device orientation changes
		/// </summary>
		OrientationChanged,

		/// <summary>
		/// fired when the game is exiting
		/// </summary>
		Exiting
	}


	/// <summary>
	/// comparer that should be passed to a dictionary constructor to avoid boxing/unboxing when using an enum as a key
	/// on Mono
	/// </summary>
	public struct CoreEventsComparer : IEqualityComparer<CoreEvents>
	{
		public bool Equals(CoreEvents x, CoreEvents y)
		{
			return x == y;
		}


		public int GetHashCode(CoreEvents obj)
		{
			return (int) obj;
		}
	}
}