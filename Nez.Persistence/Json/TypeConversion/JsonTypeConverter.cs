using System;


namespace Nez.Persistence
{
	/// <summary>
	/// used to override the default JSON writing and augment converting JSON to an object
	/// </summary>
	public abstract class JsonTypeConverter
	{
		/// <summary>
		/// indicates this converter wants to exclusively write the JSON data. The props/fields will
		/// not be encoded.
		/// </summary>
		/// <value><c>true</c> if wants exclusive write; otherwise, <c>false</c>.</value>
		public virtual bool WantsExclusiveWrite => false;

		/// <summary>
		/// indicates this converter wants to write JSON. Both WantsExclusiveWrite and CanWrite need to be true
		/// if this convertoer wants to write the JSON exclusively.
		/// </summary>
		/// <value><c>true</c> if can write; otherwise, <c>false</c>.</value>
		public virtual bool CanWrite => true;

		/// <summary>
		/// indicates this converter wants to read JSON. <see cref="OnFoundCustomData"/> will be called for each
		/// key found that does not have a corresponding member field/property. This is the data that would have
		/// been writtn in the <see cref="WriteJson"/> method.
		/// </summary>
		/// <value><c>true</c> if can read; otherwise, <c>false</c>.</value>
		public virtual bool CanRead => true;

		/// <summary>
		/// indicates this converter can work with Type <paramref name="objectType"/>
		/// </summary>
		/// <returns><c>true</c>, if convert type was caned, <c>false</c> otherwise.</returns>
		/// <param name="objectType">Object type.</param>
		public abstract bool CanConvertType(Type objectType);

		public abstract void WriteJson(IJsonEncoder encoder, object value);

		public abstract void OnFoundCustomData(object instance, string key, object value);
	}


	/// <summary>
	/// These can be used to fully override the reading and writing of JSON for any object type.
	/// </summary>
	public abstract class JsonTypeConverter<T> : JsonTypeConverter
	{
		public override bool CanConvertType(Type objectType) => typeof(T).IsAssignableFrom(objectType);

		public override void WriteJson(IJsonEncoder encoder, object value)
		{
			WriteJson(encoder, (T) value);
		}

		/// <summary>
		/// if CanWrite returns true this will be called. Here is where you encode your object using the <paramref name="encoder"/>
		/// </summary>
		/// <param name="encoder">Encoder.</param>
		/// <param name="value">Value.</param>
		public abstract void WriteJson(IJsonEncoder encoder, T value);

		public override void OnFoundCustomData(object instance, string key, object value)
		{
			OnFoundCustomData((T) instance, key, value);
		}

		/// <summary>
		/// If CanConvert returns true this will be called anytime a key/value pair that isnt found via
		/// reflection is in the JSON
		/// </summary>
		/// <returns>The to object.</returns>
		/// <param name="instance">Instance.</param>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public abstract void OnFoundCustomData(T instance, string key, object value);
	}
}