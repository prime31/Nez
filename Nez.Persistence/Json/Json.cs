using System;
using System.Reflection;


namespace Nez.Persistence
{
	public sealed class DecodeException : Exception
	{
		public DecodeException( string message ) : base( message ) { }

		public DecodeException( string message, Exception innerException ) : base( message, innerException ) { }
	}


	public static class Json
	{
		internal const string TypeHintPropertyName = "@type";
		internal const string IdPropertyName = "@id";
		internal const string RefPropertyName = "@ref";

		internal static readonly Type includeAttrType = typeof( SerializedAttribute );
		internal static readonly Type excludeAttrType = typeof( NonSerializedAttribute );
		static readonly Type beforeEncodeAttrType = typeof( BeforeEncodeAttribute );


		public static string ToJson( object obj, bool prettyPrint )
		{
			var settings = new JsonSettings { PrettyPrint = prettyPrint };
			return ToJson( obj, settings );
		}

		public static string ToJson( object obj, params JsonTypeConverter[] converters )
		{
			var settings = new JsonSettings { TypeConverters = converters };
			return ToJson( obj, settings );
		}

		/// <summary>
		/// encodes <paramref name="obj"/> to a json string
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string ToJson( object obj, JsonSettings options = null )
		{
			// Invoke methods tagged with [BeforeEncode] attribute.
			if( obj != null )
			{
				var type = obj.GetType();
				if( !( type.IsEnum || type.IsPrimitive || type.IsArray ) )
				{
					foreach( var method in type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
					{
						if( method.IsDefined( beforeEncodeAttrType ) && method.GetParameters().Length == 0 )
						{
							method.Invoke( obj, null );
						}
					}
				}
			}

			return JsonEncoder.ToJson( obj, options ?? new JsonSettings() );
		}

		/// <summary>
		/// decodes <paramref name="json"/> into a Variant object which can be used directly or converted to
		/// a strongly typed object via <cref="VariantConverter"/>
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Variant FromJson( string json )
		{
			System.Diagnostics.Debug.Assert( json != null );
			return JsonDecoder.FromJson( json );
		}

		/// <summary>
		/// decodes <paramref name="json"/> into a strongly typed object of type T
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="json">Json.</param>
		/// <param name="settings">Settings.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T FromJson<T>( string json, JsonSettings settings = null )
		{
			return VariantConverter.Decode<T>( JsonDecoder.FromJson( json ), settings );
		}

		public static T FromJson<T>( string json, params JsonTypeConverter[] converters )
		{
			var settings = new JsonSettings { TypeConverters = converters };
			return VariantConverter.Decode<T>( JsonDecoder.FromJson( json ), settings );
		}

		/// <summary>
		/// decods <paramref name="json"/> into a strongly typed object
		/// </summary>
		/// <param name="json"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T FromJson<T>( string json )
		{
			System.Diagnostics.Debug.Assert( json != null );
			return VariantConverter.Decode<T>( JsonDecoder.FromJson( json ) );
		}

		public static void FromJsonOverwrite( string json, object item )
		{
			System.Diagnostics.Debug.Assert( json != null );
			VariantConverter.DecodeInto( JsonDecoder.FromJson( json ), out item );
		}

	}
}
