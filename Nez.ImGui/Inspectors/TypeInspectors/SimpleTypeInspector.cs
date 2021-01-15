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
		public static Type[] KSupportedTypes =
		{
			typeof(bool), typeof(Color), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float),
			typeof(string), typeof(Vector2), typeof(Vector3)
		};

		RangeAttribute _rangeAttribute;
		Action _inspectMethodAction;
		bool _isUnsignedInt;

		public override void Initialize()
		{
			base.Initialize();
			_rangeAttribute = _memberInfo.GetAttribute<RangeAttribute>();

			// the inspect method name matters! We use reflection to feth it.
			var valueTypeName = _valueType.Name.ToString();
			var inspectorMethodName = "Inspect" + valueTypeName[0].ToString().ToUpper() + valueTypeName.Substring(1);
			var inspectMethodInfo = ReflectionUtils.GetMethodInfo(this, inspectorMethodName);
			_inspectMethodAction = ReflectionUtils.CreateDelegate<Action>(this, inspectMethodInfo);

			// fix up the Range.minValue if we have an unsigned value to avoid overflow when converting
			_isUnsignedInt = _valueType == typeof(uint) || _valueType == typeof(ulong);
			if (_isUnsignedInt && _rangeAttribute == null)
				_rangeAttribute = new RangeAttribute(0);
			else if (_isUnsignedInt && _rangeAttribute != null && _rangeAttribute.MinValue < 0)
				_rangeAttribute.MinValue = 0;
		}

		public override void DrawMutable()
		{
			_inspectMethodAction();
			HandleTooltip();
		}

		void InspectBoolean()
		{
			var value = GetValue<bool>();
			if (ImGui.Checkbox(_name, ref value))
				SetValue(value);
		}

		void InspectColor()
		{
			var value = GetValue<Color>().ToNumerics();
			if (ImGui.ColorEdit4(_name, ref value))
				SetValue(value.ToXNAColor());
		}

		/// <summary>
		/// simplifies int, uint, long and ulong handling. They all get converted to Int32 so there is some precision loss.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool InspectAnyInt(ref int value)
		{
			if (_rangeAttribute != null)
			{
				if (_rangeAttribute.UseDragVersion)
					return ImGui.DragInt(_name, ref value, 1, (int) _rangeAttribute.MinValue,
						(int) _rangeAttribute.MaxValue);
				else
					return ImGui.SliderInt(_name, ref value, (int) _rangeAttribute.MinValue,
						(int) _rangeAttribute.MaxValue);
			}
			else
			{
				return ImGui.InputInt(_name, ref value);
			}
		}

		void InspectInt32()
		{
			var value = GetValue<int>();

			if (InspectAnyInt(ref value))
				SetValue(value);
		}

		void InspectUInt32()
		{
			var value = Convert.ToInt32(GetValue());
			if (InspectAnyInt(ref value))
				SetValue(Convert.ToUInt32(value));
		}

		void InspectInt64()
		{
			var value = Convert.ToInt32(GetValue());
			if (InspectAnyInt(ref value))
				SetValue(Convert.ToInt64(value));
		}

		unsafe void InspectUInt64()
		{
			var value = Convert.ToInt32(GetValue());
			if (InspectAnyInt(ref value))
				SetValue(Convert.ToUInt64(value));
		}

		void InspectSingle()
		{
			var value = GetValue<float>();
			if (_rangeAttribute != null)
			{
				if (_rangeAttribute.UseDragVersion)
				{
					if (ImGui.DragFloat(_name, ref value, 1, _rangeAttribute.MinValue, _rangeAttribute.MaxValue))
						SetValue(value);
				}
				else
				{
					if (ImGui.SliderFloat(_name, ref value, _rangeAttribute.MinValue, _rangeAttribute.MaxValue))
						SetValue(value);
				}
			}
			else
			{
				if (ImGui.DragFloat(_name, ref value))
					SetValue(value);
			}
		}

		void InspectString()
		{
			var value = GetValue<string>() ?? string.Empty;
			if (ImGui.InputText(_name, ref value, 100))
				SetValue(value);
		}

		void InspectVector2()
		{
			var value = GetValue<Vector2>().ToNumerics();
			if (ImGui.DragFloat2(_name, ref value))
				SetValue(value.ToXNA());
		}

		void InspectVector3()
		{
			var value = GetValue<Vector3>().ToNumerics();
			if (ImGui.DragFloat3(_name, ref value))
				SetValue(value.ToXNA());
		}
	}
}