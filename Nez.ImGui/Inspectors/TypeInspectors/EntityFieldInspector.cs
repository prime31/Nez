using ImGuiNET;
using System.Text;


namespace Nez.ImGuiTools.TypeInspectors
{
	/// <summary>
	/// special Inspector that handles Entity references displaying a button that opens the inspector for the Entity
	/// </summary>
	public class EntityFieldInspector : AbstractTypeInspector
	{
		public override void DrawMutable()
		{
			var entity = GetValue<Entity>();

			if (NezImGui.LabelButton(_name, entity.Name))
				Core.GetGlobalManager<ImGuiManager>().StartInspectingEntity(entity);
			HandleTooltip();
		}
	}
}