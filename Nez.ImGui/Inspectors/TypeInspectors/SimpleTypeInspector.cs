using System;
using System.Runtime.InteropServices;
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
        public static Type[] kSupportedTypes = { typeof( bool ), typeof( Color ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( string ), typeof( Vector2 ), typeof( Vector3 ) };
        RangeAttribute _rangeAttribute;
        Action _inspectMethodAction;
		bool _isUnsignedInt;

        public override void initialize()
        {
            base.initialize();
            _rangeAttribute = _memberInfo.getCustomAttribute<RangeAttribute>();

            // the inspect method name matters! We use reflection to feth it.
            var valueTypeName = _valueType.Name.ToString();
            var inspectorMethodName = "inspect" + valueTypeName[0].ToString().ToUpper() + valueTypeName.Substring( 1 );
            var inspectMethodInfo = ReflectionUtils.getMethodInfo( this, inspectorMethodName );
            _inspectMethodAction = ReflectionUtils.createDelegate<Action>( this, inspectMethodInfo );

			// fix up the Range.minValue if we have an unsigned value to avoid overflow when converting
			_isUnsignedInt = _valueType == typeof( uint ) || _valueType == typeof( ulong );
			if( _isUnsignedInt && _rangeAttribute == null )
				_rangeAttribute = new RangeAttribute( 0 );
			else if( _isUnsignedInt && _rangeAttribute != null && _rangeAttribute.minValue < 0 )
				_rangeAttribute.minValue = 0;
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

		/// <summary>
		/// simplifies int, uint, long and ulong handling. They all get converted to Int32 so there is some precision loss.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool inspectAnyInt( ref int value )
		{
			if( _rangeAttribute != null )
			{
				if( _rangeAttribute.useDragVersion )
					return ImGui.DragInt( _name, ref value, 1, (int)_rangeAttribute.minValue, (int)_rangeAttribute.maxValue );
				else
					return ImGui.SliderInt( _name, ref value, (int)_rangeAttribute.minValue, (int)_rangeAttribute.maxValue );
			}
			else
			{
				return ImGui.InputInt( _name, ref value );
			}
		}

		void inspectInt32()
		{
			var value = getValue<int>();

			if( inspectAnyInt( ref value ) )
				setValue( value );
		}

		void inspectUInt32()
		{
			var value = Convert.ToInt32( getValue() );
			if( inspectAnyInt( ref value ) )
				setValue( Convert.ToUInt32( value ) );
		}

		void inspectInt64()
		{
			var value = Convert.ToInt32( getValue() );
			if( inspectAnyInt( ref value ) )
				setValue( Convert.ToInt64( value ) );
		}

		unsafe void inspectUInt64()
		{
			var value = Convert.ToInt32( getValue() );
			if( inspectAnyInt( ref value ) )
				setValue( Convert.ToUInt64( value ) );
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