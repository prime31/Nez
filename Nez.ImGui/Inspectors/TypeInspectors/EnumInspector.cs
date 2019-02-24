using System;
using System.Collections.Generic;
using ImGuiNET;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class EnumInspector : AbstractTypeInspector
	{
		string[] _enumNames;

		public override void initialize()
		{
			base.initialize();
			_enumNames = Enum.GetNames( _valueType );
		}

		public override void drawMutable()
		{
            var index = Array.IndexOf( _enumNames, getValue<object>().ToString() );
            if( ImGui.Combo( _name, ref index, _enumNames, _enumNames.Length ) )
                setValue( Enum.Parse( _valueType, _enumNames[index] ) );
			handleTooltip();
		}

	}
}
