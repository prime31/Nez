using System;
namespace Nez.Persistence
{
	public abstract class JsonTypeConverter
	{
		public virtual bool CanConvert { get; } = true;

		public virtual bool CanWrite { get; } = true;

		public abstract bool CanConvertType( Type objectType );

		public abstract void WriteJson( IJsonEncoder encoder, object value );

		public abstract object ConvertToObject( IObjectConverter converter, Type objectType, object existingValue, ProxyObject data );
	}

	/// <summary>
	/// These can be used to fully override the reading and writing of JSON for any object type.
	/// </summary>
	public abstract class JsonTypeConverter<T> : JsonTypeConverter
	{
		public override bool CanConvertType( Type objectType )
		{
			return typeof( T ).IsAssignableFrom( objectType );
		}

		public override void WriteJson( IJsonEncoder encoder, object value )
		{
			WriteJson( encoder, (T)value );
		}

		/// <summary>
		/// if CanWrite returns true this will be called. Here is where you encode your object using the <paramref name="encoder"/>
		/// </summary>
		/// <param name="encoder">Encoder.</param>
		/// <param name="value">Value.</param>
		public abstract void WriteJson( IJsonEncoder encoder, T value );

		public override object ConvertToObject( IObjectConverter converter, Type objectType, object existingValue, ProxyObject data )
		{
			return ConvertToObject( converter, objectType, (T)existingValue, data );
		}

		/// <summary>
		/// If CanConvert returns true this will be called so you can repopulate your object. 
		/// </summary>
		/// <returns>The to object.</returns>
		/// <param name="converter">Converter.</param>
		/// <param name="objectType">Object type.</param>
		/// <param name="existingValue">Existing value.</param>
		/// <param name="data">Data.</param>
		public abstract T ConvertToObject( IObjectConverter converter, Type objectType, T existingValue, ProxyObject data );
	}
}
