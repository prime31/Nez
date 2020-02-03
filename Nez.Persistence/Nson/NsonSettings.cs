using System;
using System.Collections.Generic;

namespace Nez.Persistence
{
	public class NsonSettings
	{
		/// <summary>
		/// stub to save the most useful configuration for object graphs
		/// </summary>
		public static NsonSettings HandlesReferences = new NsonSettings()
		{
			PreserveReferencesHandling = true
		};


		public bool PrettyPrint;
		public bool PreserveReferencesHandling;
		public NsonTypeConverter[] TypeConverters;


		/// <summary>
		/// finds and returns the first <see cref="NsonTypeConverter"/> found that can convert <paramref name="objectType"/>
		/// </summary>
		/// <returns>The type converter for type.</returns>
		/// <param name="objectType">Object type.</param>
		public NsonTypeConverter GetTypeConverterForType(Type objectType)
		{
			if (TypeConverters == null)
				return null;

			foreach (var converter in TypeConverters)
			{
				if (converter.CanConvertType(objectType))
					return converter;
			}
			return null;
		}

		/// <summary>
		/// finds and returns the first <see cref="NsonObjectFactory"/> found that can convert <paramref name="objectType"/>
		/// </summary>
		/// <returns>The object factory for type.</returns>
		/// <param name="objectType">Object type.</param>
		internal NsonObjectFactory GetObjectFactoryForType(Type objectType)
		{
			if (TypeConverters == null)
				return null;

			foreach (var converter in TypeConverters)
			{
				if (converter.CanConvertType(objectType) && converter is NsonObjectFactory)
				{
					return converter as NsonObjectFactory;
				}
			}
			return null;
		}
	}

}
