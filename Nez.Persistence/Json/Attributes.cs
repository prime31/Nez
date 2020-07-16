using System;


namespace Nez.Persistence
{
	/// <summary>
	/// Mark members that should be included. Public fields and properties are included by default.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class JsonIncludeAttribute : Attribute
	{
	}

	/// <summary>
	/// Mark members that should not be included.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class JsonExcludeAttribute : Attribute
	{
	}

	/// <summary>
	/// Mark methods to be called before an object is encoded.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class BeforeEncodeAttribute : Attribute
	{
	}

	/// <summary>
	/// Mark methods to be called after an object is decoded.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class AfterDecodeAttribute : Attribute
	{
	}

	/// <summary>
	/// Provide field and property aliases when an object is decoded.
	/// If a field or property is not found while decoding, this list will be searched for a matching alias.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public class DecodeAliasAttribute : Attribute
	{
		public string[] Names { get; private set; }

		public DecodeAliasAttribute(params string[] names) => Names = names;

		public bool Contains(string name) => Array.IndexOf(Names, name) > -1;
	}
}