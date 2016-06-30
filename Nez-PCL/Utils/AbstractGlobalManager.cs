namespace Nez
{
	/// <summary>
	/// global manager that can be added to Core
	/// </summary>
	public abstract class AbstractGlobalManager
	{
		/// <summary>
		/// update is called just before Scene.update each frame
		/// </summary>
		public abstract void update();
	}
}

