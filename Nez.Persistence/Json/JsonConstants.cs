using System;
using System.Reflection;


namespace Nez.Persistence
{
	/// <summary>
	/// caches some commonly used data by the Json classes
	/// </summary>
	internal static class JsonConstants
	{
		internal const string TypeHintPropertyName = "@type";
		internal const string IdPropertyName = "@id";
		internal const string RefPropertyName = "@ref";

		internal static readonly Type includeAttrType = typeof(JsonIncludeAttribute);
		internal static readonly Type excludeAttrType = typeof(JsonExcludeAttribute);
		internal static readonly Type beforeEncodeAttrType = typeof(BeforeEncodeAttribute);

		internal const BindingFlags instanceBindingFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		internal static readonly string[] iso8601Format =
		{
			@"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z",
			@"yyyy-MM-dd\THH:mm:ss\Z",
			@"yyyy-MM-dd\THH:mm:ssK"
		};
	}
}