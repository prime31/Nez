using ImGuiNET;
using Microsoft.Xna.Framework;
using System.Globalization;


namespace Nez.ImGuiTools.TypeInspectors
{
	public class Vector2Inspector : AbstractTypeInspector
	{
		public override void draw()
		{
			var value = getValue<Vector2>().toNumerics();
			if( ImGui.DragFloat2( _name, ref value ) )
				setValue( value.toXNA() );
			handleTooltip();
		}
	}
}
