using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Nez.Persistence
{
	public class VariantConverter : IObjectConverter
	{
		internal const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		static readonly Type afterDecodeAttrType = typeof( AfterDecodeAttribute );

		static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		CacheResolver _cacheResolver = new CacheResolver();
		JsonSettings _settings;


		public static T Decode<T>( Variant data, JsonSettings settings = null )
		{
			var converter = new VariantConverter( settings );
			return (T)converter.DecodeType( data, typeof( T ) );
		}

		public static void DecodeInto<T>( Variant data, out T item )
		{
			var converter = new VariantConverter( null );
			item = (T)converter.DecodeType( data, typeof( T ) );
		}

		static Type FindType( string fullName )
		{
			if( fullName == null )
			{
				return null;
			}

			if( typeCache.TryGetValue( fullName, out var type ) )
			{
				return type;
			}

			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
			{
				type = assembly.GetType( fullName );
				if( type != null )
				{
					typeCache.Add( fullName, type );
					return type;
				}
			}

			return null;
		}

		public VariantConverter( JsonSettings settings = null )
		{
			_settings = settings;
		}

		public object DecodeType( Variant data, Type type )
		{
			if( data == null )
			{
				if( type.IsValueType )
					return Activator.CreateInstance( type );
				return null;
			}

			// handle Nullables. If the type is Nullable use the underlying type
			type = Nullable.GetUnderlyingType( type ) ?? type;
			if( type.IsEnum )
			{
				return Enum.Parse( type, data.ToString( CultureInfo.InvariantCulture ) );
			}

			if( type.IsPrimitive || type == typeof( string ) || type == typeof( decimal ) )
			{
				return Convert.ChangeType( data, type );
			}

			if( type.IsArray )
			{
				if( type.GetArrayRank() == 1 )
				{
					return DecodeArray( type.GetElementType(), data );
				}

				var arrayData = data as ProxyArray;
				if( arrayData == null )
				{
					throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
				}

				var arrayRank = type.GetArrayRank();
				var rankLengths = new int[arrayRank];
				if( arrayData.CanBeMultiRankArray( rankLengths ) )
				{
					var elementType = type.GetElementType();
					if( elementType == null )
					{
						throw new DecodeException( "Array element type is expected to not be null, but it is." );
					}

					var array = Array.CreateInstance( elementType, rankLengths );
					try
					{
						DecodeMultiRankArray( elementType, arrayData, array, 1, rankLengths );
					}
					catch( Exception e )
					{
						throw new DecodeException( "Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e );
					}

					return Convert.ChangeType( array, type );
				}

				throw new DecodeException( "Error decoding multidimensional array; JSON data doesn't seem fit this structure." );
			}

			if( typeof( IList ).IsAssignableFrom( type ) )
			{
				return DecodeList( type, data );
			}

			if( typeof( IDictionary ).IsAssignableFrom( type ) )
			{
				return DecodeDictionary( type, data );
			}


			// At this point we should be dealing with a class or struct			
			var proxyObject = data as ProxyObject;
			if( proxyObject == null )
			{
				throw new InvalidCastException( "ProxyObject expected when decoding into '" + type.FullName + "'." );
			}
			var instance = DecodeObject( type, proxyObject );

			// Invoke methods tagged with [AfterDecode] attribute.
			foreach( var method in type.GetMethods( instanceBindingFlags ) )
			{
				if( method.IsDefined( afterDecodeAttrType ) )
				{
					method.Invoke( instance, method.GetParameters().Length == 0 ? null : new object[] { data } );
				}
			}

			return instance;
		}

		public object DecodeObject( Type type, ProxyObject data )
		{
			var refId = data.ReferenceId;
			if( refId != null )
			{
				return _cacheResolver.GetReference( refId );
			}


			bool didCreateInstanceViaConverter = false;
			object instance;

			// check for a JsonConverter and use it if we have one
			var converter = _settings?.GetTypeConverterForType( type );
			if( converter != null )
			{
				instance = converter.ConvertToObject( this, type, null, data );
				didCreateInstanceViaConverter = true;
			}
			else
			{
				// If there's a type hint, use it to create the instance.
				var typeHint = data.TypeHint;
				if( typeHint != null && typeHint != type.FullName )
				{
					var makeType = FindType( typeHint );
					if( makeType == null )
					{
						throw new TypeLoadException( "Could not load type '" + typeHint + "'." );
					}

					if( type.IsAssignableFrom( makeType ) )
					{
						instance = _cacheResolver.CreateInstance( makeType );
						type = makeType;
					}
					else
					{
						throw new InvalidCastException( "Cannot assign type '" + typeHint + "' to type '" + type.FullName + "'." );
					}
				}
				else
				{
					// We don't have a type hint, so just instantiate the type we have.
					instance = _cacheResolver.CreateInstance( type );
				}
			}

			// if there is an instanceId, cache the object in case any other objects are referencing it
			var id = data.InstanceId;
			if( id != null )
			{
				_cacheResolver.TrackReference( id, instance );
			}

			// if we were created by a JsonTypeConverter we are done. No need to fill in the object data.
			if( didCreateInstanceViaConverter )
			{
				return instance;
			}

			// Now decode fields and properties.
			foreach( var pair in data )
			{
				var field = _cacheResolver.GetField( type, pair.Key );
				if( field != null )
				{
					if( _cacheResolver.IsMemberInfoEncodeableOrDecodeable( field, field.IsPublic ) )
					{
						if( type.IsValueType )
						{
							// Type is a struct.
							var instanceRef = instance;
							field.SetValue( instanceRef, DecodeType( pair.Value, field.FieldType ) );
							instance = instanceRef;
						}
						else
						{
							// Type is a class.
							field.SetValue( instance, DecodeType( pair.Value, field.FieldType ) );
						}
					}
					continue;
				}

				var property = _cacheResolver.GetEncodeableProperty( type, pair.Key );
				if( property != null && property.CanWrite && property.IsDefined( Json.includeAttrType ) )
				{
					if( type.IsValueType )
					{
						// Type is a struct
						var instanceRef = instance;
						property.SetValue( instanceRef, DecodeType( pair.Value, property.PropertyType ), null );
						instance = instanceRef;
					}
					else
					{
						// Type is a class
						property.SetValue( instance, DecodeType( pair.Value, property.PropertyType ), null );
					}
				}
			}

			return instance;
		}

		public object DecodeList( Type type, Variant data )
		{
			var proxyArray = data as ProxyArray;
			if( proxyArray == null )
			{
				throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
			}

			var innerType = type.GetGenericArguments()[0];
			var genericType = typeof( List<> ).MakeGenericType( innerType );
			var list = (IList)_cacheResolver.CreateInstance( genericType );

			foreach( var item in proxyArray )
			{
				list.Add( DecodeType( item, innerType ) );
			}

			return list;
		}

		public object DecodeDictionary( Type type, Variant data )
		{
			var proxyObject = data as ProxyObject;
			if( proxyObject == null )
			{
				throw new DecodeException( "Variant is expected to be a ProxyObject here, but it is not." );
			}

			var keyType = type.GetGenericArguments()[0];
			var valueType = type.GetGenericArguments()[1];

			var genericType = typeof( Dictionary<,> ).MakeGenericType( keyType, valueType );
			var dict = (IDictionary)_cacheResolver.CreateInstance( genericType );

			foreach( var pair in proxyObject )
			{
				var k = keyType.IsEnum ? Enum.Parse( keyType, pair.Key ) : Convert.ChangeType( pair.Key, keyType );
				var v = DecodeType( pair.Value, valueType );
				dict.Add( k, v );
			}

			return dict;
		}

		public object DecodeArray( Type elementType, Variant data )
		{
			var arrayData = data as ProxyArray;
			if( arrayData == null )
			{
				throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
			}

			var arrayLength = arrayData.Count;
			var array = Array.CreateInstance( elementType, arrayLength );

			for( var i = 0; i < arrayLength;  i++ )
			{
				array.SetValue( DecodeType( data[i], elementType ), i );
			}

			return array;
		}

		public void DecodeMultiRankArray( Type elementType, ProxyArray arrayData, Array array, int arrayRank, int[] indices )
		{
			var count = arrayData.Count;
			for( var i = 0; i < count; i++ )
			{
				indices[arrayRank - 1] = i;
				if( arrayRank < array.Rank )
				{
					DecodeMultiRankArray( elementType, arrayData[i] as ProxyArray, array, arrayRank + 1, indices );
				}
				else
				{
					array.SetValue( DecodeType( arrayData[i], elementType ), indices );
				}
			}
		}

	}
}