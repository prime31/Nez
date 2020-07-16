using System;
using System.Reflection;


namespace Nez.Persistence
{
	public sealed class DecodeException : Exception
	{
		public DecodeException(string message) : base(message)
		{
		}

		public DecodeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}


	public static class Json
	{
		/// <summary>
		/// encodes <paramref name="obj"/> to a json string, pretty printed
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="obj">Object.</param>
		/// <param name="prettyPrint">If set to <c>true</c> pretty print.</param>
		public static string ToJson(object obj, bool prettyPrint)
		{
			var settings = new JsonSettings {PrettyPrint = prettyPrint};
			return ToJson(obj, settings);
		}

		/// <summary>
		/// encodes <paramref name="obj"/> to a json string with an optional <see cref="JsonTypeConverter"/>
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="obj">Object.</param>
		/// <param name="converters">Converters.</param>
		public static string ToJson(object obj, params JsonTypeConverter[] converters)
		{
			var settings = new JsonSettings {TypeConverters = converters};
			return ToJson(obj, settings);
		}

		/// <summary>
		/// encodes <paramref name="obj"/> to a json string
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string ToJson(object obj, JsonSettings options = null)
		{
			// Invoke methods tagged with [BeforeEncode] attribute.
			if (obj != null)
			{
				var type = obj.GetType();
				if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
				{
					foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
					                                       BindingFlags.Instance))
					{
						if (method.IsDefined(JsonConstants.beforeEncodeAttrType) && method.GetParameters().Length == 0)
						{
							method.Invoke(obj, null);
						}
					}
				}
			}

			return JsonEncoder.ToJson(obj, options ?? new JsonSettings());
		}

		/// <summary>
		/// decodes <paramref name="json"/> into a strongly typed object of type T
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="json">Json.</param>
		/// <param name="settings">Settings.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static object FromJson(string json, JsonSettings settings = null)
		{
			System.Diagnostics.Debug.Assert(json != null);
			return JsonDecoder.FromJson(json, settings);
		}

		/// <summary>
		/// decods <paramref name="json"/> into a strongly typed object
		/// </summary>
		/// <param name="json"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T FromJson<T>(string json, JsonSettings settings = null)
		{
			System.Diagnostics.Debug.Assert(json != null);
			return JsonDecoder.FromJson<T>(json, settings);
		}

		/// <summary>
		/// overwrites any data on <paramref name="item"/> with the data found in the json string
		/// </summary>
		/// <param name="json">Json.</param>
		/// <param name="item">Item.</param>
		public static void FromJsonOverwrite(string json, object item)
		{
			System.Diagnostics.Debug.Assert(json != null);
			JsonDecoder.FromJsonOverwrite(json, item);
		}
	}
}