using System;
using System.Reflection;


namespace Nez.Persistence
{
	public static class Nson
	{
		/// <summary>
		/// encodes <paramref name="obj"/> to a nson string, pretty printed
		/// </summary>
		/// <returns>The nson.</returns>
		/// <param name="obj">Object.</param>
		/// <param name="prettyPrint">If set to <c>true</c> pretty print.</param>
		public static string ToNson(object obj, bool prettyPrint)
		{
			var settings = new NsonSettings { PrettyPrint = prettyPrint };
			return ToNson(obj, settings);
		}

		/// <summary>
		/// encodes <paramref name="obj"/> to a nson string with an optional <see cref="NsonTypeConverter"/>
		/// </summary>
		/// <returns>The nson.</returns>
		/// <param name="obj">Object.</param>
		/// <param name="converters">Converters.</param>
		public static string ToNson(object obj, params NsonTypeConverter[] converters)
		{
			var settings = new NsonSettings { TypeConverters = converters };
			return ToNson(obj, settings);
		}

		/// <summary>
		/// encodes <paramref name="obj"/> to a nson string
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string ToNson(object obj, NsonSettings options = null)
		{
			// Invoke methods tagged with [BeforeEncode] attribute.
			if (obj != null)
			{
				var type = obj.GetType();
				if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
				{
					foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						if (method.IsDefined(NsonConstants.beforeEncodeAttrType) && method.GetParameters().Length == 0)
						{
							method.Invoke(obj, null);
						}
					}
				}
			}

			return NsonEncoder.ToNson(obj, options ?? new NsonSettings());
		}

		/// <summary>
		/// decodes <paramref name="nson"/> into a strongly typed object of type T
		/// </summary>
		/// <returns>The nson.</returns>
		/// <param name="nson">Nson.</param>
		/// <param name="settings">Settings.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static object FromNson(string nson, NsonSettings settings = null)
		{
			System.Diagnostics.Debug.Assert(nson != null);
			return NsonDecoder.FromNson(nson, settings);
		}

		/// <summary>
		/// decods <paramref name="nson"/> into a strongly typed object
		/// </summary>
		/// <param name="nson"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T FromNson<T>(string nson, NsonSettings settings = null)
		{
			System.Diagnostics.Debug.Assert(nson != null);
			return NsonDecoder.FromNson<T>(nson, settings);
		}

		/// <summary>
		/// overwrites any data on <paramref name="item"/> with the data found in the nson string
		/// </summary>
		/// <param name="nson">Nson.</param>
		/// <param name="item">Item.</param>
		public static void FromNsonOverwrite(string nson, object item)
		{
			System.Diagnostics.Debug.Assert(nson != null);
			NsonDecoder.FromNsonOverwrite(nson, item);
		}

	}
}
