using System;
namespace Nez.Persistence
{
	public abstract class JsonTypeConverter
	{
		public virtual bool CanRead { get; } = true;

		public virtual bool CanWrite { get; } = true;

		public abstract bool CanConvert( Type objectType );

		public abstract void WriteJson( IJsonEncoder encoder, object value );

		public abstract object ConvertToObject( IObjectConverter converter, Type objectType, object existingValue, ProxyObject data );
	}

	/// <summary>
	/// These can be used to fully override the reading and writing of JSON for any object type.
	/// </summary>
	public abstract class JsonTypeConverter<T> : JsonTypeConverter
	{
		public override bool CanConvert( Type objectType )
		{
			return typeof( T ).IsAssignableFrom( objectType );
		}

		public override void WriteJson( IJsonEncoder encoder, object value )
		{
			WriteJson( encoder, (T)value );
		}

		public abstract void WriteJson( IJsonEncoder encoder, T value );

		public override object ConvertToObject( IObjectConverter converter, Type objectType, object existingValue, ProxyObject data )
		{
			return ConvertToObject( converter, objectType, (T)existingValue, data );
		}

		public abstract T ConvertToObject( IObjectConverter converter, Type objectType, T existingValue, ProxyObject data );
	}
}
