using ImGuiNET;
using Microsoft.Xna.Framework;
using System.Globalization;


namespace Nez.ImGuiTools.TypeInspectors
{
	public class Vector3Inspector : AbstractTypeInspector
	{
		public override void draw()
		{
			var value = getValue<Vector3>().toNumerics();
			if( ImGui.DragFloat3( _name, ref value ) )
				setValue( value.toXNA() );
		}
	}
}
