using System;
using System.Collections;


namespace Nez.Persistence
{
	public abstract class JsonObjectFactory : JsonTypeConverter
	{
		public override bool CanWrite => false;
		public override bool CanRead => false;

		public abstract object CreateObject(Type objectType, IDictionary objectData);

		public override void WriteJson(IJsonEncoder encoder, object value)
		{
			throw new NotSupportedException(
				"JsonObjectFactory should only be used while decoding unless you overwrite WriteJson");
		}

		public override void OnFoundCustomData(object instance, string key, object value)
		{
			throw new NotSupportedException("This should never happen");
		}
	}

	/// <summary>
	/// used to override the default JSON object creation for a Type.
	/// </summary>
	public abstract class JsonObjectFactory<T> : JsonObjectFactory
	{
		public override bool CanConvertType(Type objectType) => typeof(T).IsAssignableFrom(objectType);

		public override object CreateObject(Type objectType, IDictionary objectData)
		{
			return Create(objectType, objectData);
		}

		public abstract T Create(Type objectType, IDictionary objectData);
	}
}