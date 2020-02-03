using System;
using System.Collections;
using System.Collections.Generic;

namespace Nez.Persistence
{
	public abstract class NsonObjectFactory : NsonTypeConverter
	{
		public override bool CanWrite => false;
		public override bool CanRead => false;

		public abstract object CreateObject(Type objectType, Dictionary<string, object> objectData);

		public override void WriteNson(INsonEncoder encoder, object value)
		{
			throw new NotSupportedException("NsonObjectFactory should only be used while decoding unless you overwrite WriteNson");
		}

		public override void OnFoundCustomData(ref object instance, string key, object value)
		{
			throw new NotSupportedException("This should never happen");
		}
	}

	/// <summary>
	/// used to override the default NSON object creation for a Type.
	/// </summary>
	public abstract class NsonObjectFactory<T> : NsonObjectFactory
	{
		public override bool CanConvertType(Type objectType) => typeof(T).IsAssignableFrom(objectType);

		public override object CreateObject(Type objectType, Dictionary<string, object> objectData)
		{
			return Create(objectType, objectData);
		}

		public abstract T Create(Type objectType, Dictionary<string, object> objectData);

	}
}
