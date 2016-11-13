using System;


namespace Nez
{
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

