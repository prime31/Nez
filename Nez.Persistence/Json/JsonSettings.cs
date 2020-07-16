using System;
using System.Collections.Generic;


namespace Nez.Persistence
{
	public enum TypeNameHandling
	{
		/// <summary>
		/// Do not include the .NET type name when serializing types
		/// </summary>
		None,

		/// <summary>
		/// Include the .NET type name when serializing into a JSON object structure
		/// </summary>
		Objects,

		/// <summary>
		/// Include the .NET type name when serializing into a JSON array structure
		/// </summary>
		Arrays,

		/// <summary>
		/// Always include the .NET type name when serializing
		/// </summary>
		All,

		/// <summary>
		/// Include the .NET type name when the type of the object being serialized is not the same as its declared type
		/// </summary>
		Auto
	}

	public class JsonSettings
	{
		/// <summary>
		/// stub to save the most useful configuration for object graphs
		/// </summary>
		public static JsonSettings HandlesReferences = new JsonSettings()
		{
			TypeNameHandling = TypeNameHandling.Auto,
			PreserveReferencesHandling = true
		};

		public TypeNameHandling TypeNameHandling = TypeNameHandling.None;
		public bool PrettyPrint;
		public bool PreserveReferencesHandling;
		public JsonTypeConverter[] TypeConverters;


		/// <summary>
		/// finds and returns the first <see cref="JsonTypeConverter"/> found that can convert <paramref name="objectType"/>
		/// </summary>
		/// <returns>The type converter for type.</returns>
		/// <param name="objectType">Object type.</param>
		internal JsonTypeConverter GetTypeConverterForType(Type objectType)
		{
			if (TypeConverters == null)
				return null;

			foreach (var converter in TypeConverters)
			{
				if (converter.CanConvertType(objectType))
				{
					return converter;
				}
			}

			return null;
		}

		/// <summary>
		/// finds and returns the first <see cref="JsonObjectFactory"/> found that can convert <paramref name="objectType"/>
		/// </summary>
		/// <returns>The object factory for type.</returns>
		/// <param name="objectType">Object type.</param>
		internal JsonObjectFactory GetObjectFactoryForType(Type objectType)
		{
			if (TypeConverters == null)
				return null;

			foreach (var converter in TypeConverters)
			{
				if (converter.CanConvertType(objectType) && converter is JsonObjectFactory)
				{
					return converter as JsonObjectFactory;
				}
			}

			return null;
		}
	}
}