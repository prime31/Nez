using System.Collections.Generic;


namespace Nez
{
	public abstract class PassiveSystem : EntitySystem
	{
		public override void OnChange(Entity entity)
		{
			// We do not manage any notification of entities changing state  and avoid polluting our list of entities as we want to keep it empty
		}


		protected override void Process(List<Entity> entities)
		{
			// We replace the basic entity system with our own that doesn't take into account entities
			Begin();
			End();
		}
	}
}