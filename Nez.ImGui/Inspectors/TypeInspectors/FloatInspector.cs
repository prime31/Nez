using ImGuiNET;
using System.Reflection;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class FloatInspector : AbstractTypeInspector
	{
		RangeAttribute _rangeAttribute;

		public override void initialize()
		{
			base.initialize();
			_rangeAttribute = _memberInfo.GetCustomAttribute<RangeAttribute>();
		}

		public override void draw()
		{
			var value = getValue<float>();
			if( _rangeAttribute != null )
			{
				if( ImGui.SliderFloat( _name, ref value, _rangeAttribute.minValue, _rangeAttribute.maxValue ) )
					setValue( value );
			}
			else
			{
				if( ImGui.DragFloat( _name, ref value ) )
					setValue( value );
			}
			handleTooltip();
		}
	}
}
