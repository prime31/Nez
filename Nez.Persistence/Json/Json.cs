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


		/// <summary>
		/// encodes <paramref name="obj"/> to a json string
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string Encode( object obj, JsonSettings options = null )
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

			return JsonEncoder.Encode( obj, options ?? new JsonSettings() );
		}

		/// <summary>
		/// decodes <paramref name="json"/> into a Variant object which can be used directly or converted to
		/// a strongly typed object via <cref="VaraintConverter"/>
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Variant Decode( string json )
		{
			System.Diagnostics.Debug.Assert( json != null );
			return JsonDecoder.Decode( json );
		}

		/// <summary>
		/// decods <paramref name="json"/> into a strongly typed object
		/// </summary>
		/// <param name="json"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T Decode<T>( string json )
		{
			System.Diagnostics.Debug.Assert( json != null );
			return VariantConverter.Decode<T>( JsonDecoder.Decode( json ) );
		}

		/// <summary>
		/// note: this does not do what the name implies! <paramref name="item"/> can be null when passed in
		/// and it will be overwritten entirely.
		/// </summary>
		/// <param name="json"></param>
		/// <param name="item"></param>
		/// <typeparam name="T"></typeparam>
		public static void DecodeInto<T>( string json, out T item )
		{
			System.Diagnostics.Debug.Assert( json != null );
			VariantConverter.DecodeInto( JsonDecoder.Decode( json ), out item );
		}

		public static void PopulateObject( string json, object item )
		{
			throw new NotImplementedException();
		}

	}
}
