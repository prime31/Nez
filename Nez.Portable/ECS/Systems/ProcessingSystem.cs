using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// A basic processing system that doesn't rely on entities.
	/// It's got no entities associated but it's still being called each frame.
	/// Use this as a base class for generic systems that need to coordinate other systems
	/// </summary>
	public abstract class ProcessingSystem : EntitySystem
	{
		public override void OnChange(Entity entity)
		{
			// We do not manage any notification of entities changing state  and avoid polluting our list of entities as we want to keep it empty
		}

		protected override void Process(List<Entity> entities)
		{
			// We replace the basic entity system with our own that doesn't take into account entities
			Begin();
			Process();
			End();
		}

		/// <summary>
		/// Process our system. This is being called each and every frame.
		/// </summary>
		public abstract void Process();
	}
}