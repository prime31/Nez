using System;

namespace Nez
{
	/// <summary>
	/// Attribute that is used to indicate that the field/property should be present in the inspector
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class InspectableAttribute : Attribute
	{}

	/// <summary>
	/// Attribute that is used to indicate that the field/property should not be present in the inspector
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class NotInspectableAttribute : Attribute
	{}

	/// <summary>
	/// adding this to a method will expose it to the inspector if it has 0 params or 1 param of a supported type: int, float, string
	/// and bool are currently supported.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class InspectorCallableAttribute : InspectableAttribute
	{}

	/// <summary>
	/// displays a tooltip when hovering over the label of any inspectable elements
	/// </summary>
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method )]
	public class TooltipAttribute : InspectableAttribute
	{
		public string tooltip;

		public TooltipAttribute( string tooltip )
		{
			this.tooltip = tooltip;
		}
	}

	/// <summary>
	/// Range attribute. Tells the inspector you want a slider to be displayed for a float/int
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class RangeAttribute : InspectableAttribute
	{
		public float minValue;
		public float maxValue;
		public float stepSize = 1;
		public bool useDragVersion;


		public RangeAttribute( float minValue )
		{
			this.minValue = minValue;

			// magic number! This is the highest number ImGui functions properly with for some reason.
			maxValue = int.MaxValue - 100;
			useDragVersion = true;
		}

		public RangeAttribute( float minValue, float maxValue, float stepSize )
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.stepSize = stepSize;
		}

		public RangeAttribute( float minValue, float maxValue, bool useDragFloat )
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.useDragVersion = useDragFloat;
		}


		public RangeAttribute( float minValue, float maxValue ) : this( minValue, maxValue, 0.1f )
		{ }

	}

	/// <summary>
	/// putting this attribute on a class and specifying a subclass of Inspector lets you create custom inspectors for any type. When
	/// the Inspector finds a field/property of the type with the attribute on it the inspectorType will be instantiated and used.
	/// Inspectors are only active in DEBUG builds so make sure to wrap your custom inspector subclass in #if DEBUG/#endif.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct )]
	public class CustomInspectorAttribute : Attribute
	{
		public Type inspectorType;


		public CustomInspectorAttribute( Type inspectorType )
		{
			this.inspectorType = inspectorType;
		}
	}
}