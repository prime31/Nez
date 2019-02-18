using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class ColorInspector : AbstractTypeInspector
	{
		public override void draw()
		{
			var value = getValue<Color>().toNumerics();
			if( ImGui.ColorEdit4( _name, ref value ) )
				setValue( value.toXNAColor() );
		}
	}
}
