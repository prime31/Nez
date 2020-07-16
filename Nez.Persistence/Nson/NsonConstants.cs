using System;
using System.Reflection;

namespace Nez.Persistence
{
	/// <summary>
	/// caches some commonly used data by the Nson classes
	/// </summary>
	internal static class NsonConstants
	{
		internal const string TypeHintPropertyName = "@type";
		internal const string IdPropertyName = "@id";
		internal const string RefPropertyName = "@ref";

		internal static readonly Type includeAttrType = typeof(NsonIncludeAttribute);
		internal static readonly Type beforeEncodeAttrType = typeof(BeforeEncodeAttribute);

		internal const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		internal static readonly string[] iso8601Format =
		{
			@"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z",
			@"yyyy-MM-dd\THH:mm:ss\Z",
			@"yyyy-MM-dd\THH:mm:ssK"
		};
	}
}
