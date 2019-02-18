using ImGuiNET;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class BoolInspector : AbstractTypeInspector
	{
		public override void draw()
		{
			var value = getValue<bool>();
			if( ImGui.Checkbox( _name, ref value ) )
				setValue( value );
		}
	}
}
