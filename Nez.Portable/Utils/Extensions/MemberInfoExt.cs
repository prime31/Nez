using System;
using System.Reflection;


namespace Nez
{
	public static class MemberInfoExt
	{
		/// <summary>
		/// for some reason, GetCustomAttributes doesnt actually filter properly and throws a "Multiple custom attributes
		/// of the same type found" Exception if there are multiple (but differnt) attriburtes. This method fixes that issue.
		/// </summary>
		/// <param name="self"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetAttribute<T>(this MemberInfo self) where T : Attribute
		{
			var attributes = self.GetCustomAttributes(typeof(T));
			foreach (var attribute in attributes)
			{
				if (attribute.GetType() == typeof(T))
					return (T) attribute;
			}

			return null;
		}
	}
}