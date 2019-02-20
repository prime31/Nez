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
			_rangeAttribute = _memberInfo.getCustomAttribute<RangeAttribute>();
		}

		public override void draw()
		{
			var value = getValue<int>();
			if( _rangeAttribute != null )
			{
				if( _rangeAttribute != null && _rangeAttribute.useDragVersion )
				{
					if( ImGui.DragInt( _name, ref value, 1, (int)_rangeAttribute.minValue, (int)_rangeAttribute.maxValue ) )
						setValue( value );
				}
				else
				{
					if( ImGui.SliderInt( _name, ref value, (int)_rangeAttribute.minValue, (int)_rangeAttribute.maxValue ) )
						setValue( value );
				}
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
