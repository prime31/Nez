using System.Reflection;
using ImGuiNET;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class IntInspector : AbstractTypeInspector
	{
		RangeAttribute _rangeAttribute;

		public override void initialize()
		{
			base.initialize();
			_rangeAttribute = _memberInfo.GetCustomAttribute<RangeAttribute>();
		}

		public override void draw()
		{
			var value = getValue<int>();
			if( _rangeAttribute != null )
			{
				if( ImGui.SliderInt( _name, ref value, (int)_rangeAttribute.minValue, (int)_rangeAttribute.maxValue ) )
					setValue( value );
			}
			else
			{
				if( ImGui.DragInt( _name, ref value ) )
					setValue( value );
			}
			handleTooltip();
		}
	}
}
