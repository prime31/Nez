using System;


namespace Nez
{
	/// <summary>
	/// Attribute that is used to indicate that the field/property should be present in the inspector
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InspectableAttribute : Attribute
	{
	}

	/// <summary>
	/// Attribute that is used to indicate that the field/property should not be present in the inspector
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class NotInspectableAttribute : Attribute
	{
	}

	/// <summary>
	/// adding this to a method will expose it to the inspector if it has 0 params or 1 param of a supported type: int, float, string
	/// and bool are currently supported.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class InspectorCallableAttribute : InspectableAttribute
	{
	}

	/// <summary>
	/// displays a tooltip when hovering over the label of any inspectable elements
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
	public class TooltipAttribute : InspectableAttribute
	{
		public string Tooltip;

		public TooltipAttribute(string tooltip)
		{
			Tooltip = tooltip;
		}
	}

	/// <summary>
	/// Range attribute. Tells the inspector you want a slider to be displayed for a float/int
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class RangeAttribute : InspectableAttribute
	{
		public float MinValue;
		public float MaxValue;
		public float StepSize = 1;
		public bool UseDragVersion;


		public RangeAttribute(float minValue)
		{
			MinValue = minValue;

			// magic number! This is the highest number ImGui functions properly with for some reason.
			MaxValue = int.MaxValue - 100;
			UseDragVersion = true;
		}

		public RangeAttribute(float minValue, float maxValue, float stepSize)
		{
			MinValue = minValue;
			MaxValue = maxValue;
			StepSize = stepSize;
		}

		public RangeAttribute(float minValue, float maxValue, bool useDragFloat)
		{
			MinValue = minValue;
			MaxValue = maxValue;
			UseDragVersion = useDragFloat;
		}


		public RangeAttribute(float minValue, float maxValue) : this(minValue, maxValue, 0.1f)
		{
		}
	}

	/// <summary>
	/// putting this attribute on a class and specifying a subclass of Inspector lets you create custom inspectors for any type. When
	/// the Inspector finds a field/property of the type with the attribute on it the inspectorType will be instantiated and used.
	/// Inspectors are only active in DEBUG builds so make sure to wrap your custom inspector subclass in #if DEBUG/#endif.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class CustomInspectorAttribute : Attribute
	{
		public Type InspectorType;


		public CustomInspectorAttribute(Type inspectorType)
		{
			InspectorType = inspectorType;
		}
	}
}