using System;
namespace Nez.Persistence
{
	public abstract class JsonTypeConverter
	{
		public abstract bool CanConvertType( Type objectType );

		public abstract void WriteJson( IJsonEncoder encoder, object value );

		public abstract void OnFoundCustomData( object instance, string key, object value );
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

		public override void OnFoundCustomData( object instance, string key, object value )
		{
			OnFoundCustomData( (T)instance, key, value );
		}

		/// <summary>
		/// If CanConvert returns true this will be called anytime a key/value pair that isnt found via
		/// reflection is in the JSON
		/// </summary>
		/// <returns>The to object.</returns>
		/// <param name="instance">Instance.</param>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public abstract void OnFoundCustomData( T instance, string key, object value );
	
	}
}
