using ImGuiNET;
using System.Text;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class StringInspector : AbstractTypeInspector
	{
		public override void draw()
		{
			var value = getValue<string>() ?? string.Empty;
			if( ImGui.InputText( _name, ref value, 100 ) )
				setValue( value );
			handleTooltip();
		}
	}
}
