namespace Nez
{
	/// <summary>
	/// global manager that can be added to Core
	/// </summary>
	public interface IUpdatableManager
	{
		/// <summary>
		/// update is called just before Scene.update each frame
		/// </summary>
		void update();
	}
}

