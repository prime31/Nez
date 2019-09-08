using System;

namespace Nez.Persistence
{
	/// <summary>
	/// Mark members that should be included. Public fields and properties are included by default.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class NsonIncludeAttribute : Attribute { }

}