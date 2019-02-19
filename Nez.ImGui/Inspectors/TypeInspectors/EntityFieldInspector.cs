using ImGuiNET;
using System.Text;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class EntityFieldInspector : AbstractTypeInspector
	{
		public override void draw()
		{
			var entity = getValue<Entity>();
			ImGui.AlignTextToFramePadding();
			ImGui.Text( _name );
			ImGui.SameLine();
			if( ImGui.Button( entity.name ) )
				Core.getGlobalManager<ImGuiManager>().startInspectingEntity( entity );
			handleTooltip();
		}
	}
}
