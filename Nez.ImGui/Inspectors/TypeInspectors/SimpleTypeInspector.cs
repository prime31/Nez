using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Num = System.Numerics;

namespace Nez.ImGuiTools.TypeInspectors
{
	/// <summary>
	/// handles inspecting a slew of different basic types
	/// </summary>
    public class SimpleTypeInspector : AbstractTypeInspector
    {
        public static Type[] kSupportedTypes = { typeof( bool ), typeof( Color ), typeof( int ), typeof( float ), typeof( string ), typeof( Vector2 ), typeof( Vector3 ) };
        RangeAttribute _rangeAttribute;
        Action _inspectMethodAction;

        public override void initialize()
        {
            base.initialize();
            _rangeAttribute = _memberInfo.getCustomAttribute<RangeAttribute>();

            // the inspect method name matters! We use reflection to feth it.
            var valueTypeName = _valueType.Name.ToString();
            var inspectorMethodName = "inspect" + valueTypeName[0].ToString().ToUpper() + valueTypeName.Substring( 1 );
            var inspectMethodInfo = ReflectionUtils.getMethodInfo( this, inspectorMethodName );
            _inspectMethodAction = ReflectionUtils.createDelegate<Action>( this, inspectMethodInfo );
        }

		public override void drawMutable()
		{
            _inspectMethodAction();
			handleTooltip();
		}

		void inspectBoolean()
		{
			var value = getValue<bool>();
			if( ImGui.Checkbox( _name, ref value ) )
				setValue( value );
		}

		void inspectColor()
		{
			var value = getValue<Color>().toNumerics();
			if( ImGui.ColorEdit4( _name, ref value ) )
				setValue( value.toXNAColor() );
		}

		void inspectInt32()
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
		}

		void inspectSingle()
		{
			var value = getValue<float>();
			if( _rangeAttribute != null )
			{
				if( _rangeAttribute.useDragVersion )
				{
					if( ImGui.DragFloat( _name, ref value, 1, _rangeAttribute.minValue, _rangeAttribute.maxValue ) )
						setValue( value );
				}
				else
				{
					if( ImGui.SliderFloat( _name, ref value, _rangeAttribute.minValue, _rangeAttribute.maxValue ) )
						setValue( value );
				}
			}
			else
			{
				if( ImGui.DragFloat( _name, ref value ) )
					setValue( value );
			}
		}

		void inspectString()
		{
			var value = getValue<string>() ?? string.Empty;
			if( ImGui.InputText( _name, ref value, 100 ) )
				setValue( value );
		}

		void inspectVector2()
		{
			var value = getValue<Vector2>().toNumerics();
			if( ImGui.DragFloat2( _name, ref value ) )
				setValue( value.toXNA() );
		}

		void inspectVector3()
		{
			var value = getValue<Vector3>().toNumerics();
			if( ImGui.DragFloat3( _name, ref value ) )
				setValue( value.toXNA() );
		}
    
    }
}