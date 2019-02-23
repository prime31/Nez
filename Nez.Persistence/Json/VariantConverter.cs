using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Nez.Persistence
{
	public class VariantConverter
	{
		internal const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		const BindingFlags staticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		internal static readonly MethodInfo decodeTypeMethod = typeof( VariantConverter ).GetMethod( "DecodeType", staticBindingFlags );
		static readonly MethodInfo decodeListMethod = typeof( VariantConverter ).GetMethod( "DecodeList", staticBindingFlags );
		static readonly MethodInfo decodeDictionaryMethod = typeof( VariantConverter ).GetMethod( "DecodeDictionary", staticBindingFlags );
		static readonly MethodInfo decodeArrayMethod = typeof( VariantConverter ).GetMethod( "DecodeArray", staticBindingFlags );
		static readonly MethodInfo decodeMultiRankArrayMethod = typeof( VariantConverter ).GetMethod( "DecodeMultiRankArray", staticBindingFlags );

		internal static readonly Type decodeAliasAttrType = typeof( DecodeAliasAttribute );
		static readonly Type afterDecodeAttrType = typeof( AfterDecodeAttribute );

		static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		static CacheResolver _cacheResolver = new CacheResolver();


		public static T Make<T>( Variant data )
		{
			var item = DecodeType<T>( data );
			_cacheResolver.Clear();
			return item;
		}

		public static void MakeInto<T>( Variant data, out T item )
		{
			item = DecodeType<T>( data );
			_cacheResolver.Clear();
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

		static T DecodeType<T>( Variant data )
		{
			if( data == null )
			{
				return default( T );
			}

			var type = typeof( T );

			// handle Nullables. If the type is Nullable use the underlying type
			type = Nullable.GetUnderlyingType( type ) ?? type;
			if( type.IsEnum )
			{
				return (T)Enum.Parse( type, data.ToString( CultureInfo.InvariantCulture ) );
			}

			if( type.IsPrimitive || type == typeof( string ) || type == typeof( decimal ) )
			{
				return (T)Convert.ChangeType( data, type );
			}

			if( type.IsArray )
			{
				if( type.GetArrayRank() == 1 )
				{
					var makeFunc = decodeArrayMethod.MakeGenericMethod( type.GetElementType() );
					return (T)makeFunc.Invoke( null, new object[] { data } );
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
						throw new DecodeException( "Array element type is expected to be not null, but it is." );
					}

					var array = Array.CreateInstance( elementType, rankLengths );
					var makeFunc = decodeMultiRankArrayMethod.MakeGenericMethod( elementType );
					try
					{
						makeFunc.Invoke( null, new object[] { arrayData, array, 1, rankLengths } );
					}
					catch( Exception e )
					{
						throw new DecodeException( "Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e );
					}

					return (T)Convert.ChangeType( array, typeof( T ) );
				}

				throw new DecodeException( "Error decoding multidimensional array; JSON data doesn't seem fit this structure." );
			}

			if( typeof( IList ).IsAssignableFrom( type ) )
			{
				var makeFunc = decodeListMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T)makeFunc.Invoke( null, new object[] { data } );
			}

			if( typeof( IDictionary ).IsAssignableFrom( type ) )
			{
				var makeFunc = decodeDictionaryMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T)makeFunc.Invoke( null, new object[] { data } );
			}


			// At this point we should be dealing with a class or struct			
			var proxyObject = data as ProxyObject;
			if( proxyObject == null )
			{
				throw new InvalidCastException( "ProxyObject expected when decoding into '" + type.FullName + "'." );
			}

			var refId = proxyObject.ReferenceId;
			if( refId != null )
			{
				return _cacheResolver.GetReference<T>( refId );
			}

			// If there's a type hint, use it to create the instance.
			T instance;
			var typeHint = proxyObject.TypeHint;
			if( typeHint != null && typeHint != type.FullName )
			{
				var makeType = FindType( typeHint );
				if( makeType == null )
				{
					throw new TypeLoadException( "Could not load type '" + typeHint + "'." );
				}

				if( type.IsAssignableFrom( makeType ) )
				{
					instance = _cacheResolver.CreateInstance<T>( makeType );
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
				instance = _cacheResolver.CreateInstance<T>( typeof( T ) );
			}

			// if there is an instanceId, cache the object in case any other objects are referencing it
			var id = proxyObject.InstanceId;
			if( id != null )
			{
				_cacheResolver.TrackReference( id, instance );
			}

			// Now decode fields and properties.
			foreach( var pair in (ProxyObject)data )
			{
				var field = _cacheResolver.GetField( type, pair.Key );
				if( field != null )
				{
					if( CacheResolver.IsMemberInfoEncodeableOrDecodeable( field, field.IsPublic ) )
					{
						var makeFunc = _cacheResolver.GetDecodeTypeMethodForField( field.FieldType );
						if( type.IsValueType )
						{
							// Type is a struct.
							var instanceRef = (object)instance;
							field.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ) );
							instance = (T)instanceRef;
						}
						else
						{
							// Type is a class.
							field.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ) );
						}
					}
					continue;
				}

				var property = _cacheResolver.GetProperty( type, pair.Key );
				if( property != null )
				{
					if( property.CanWrite && property.IsDefined( Json.includeAttrType ) )
					{
						var makeFunc = _cacheResolver.GetDecodeTypeMethodForField( property.PropertyType );
						if( type.IsValueType )
						{
							// Type is a struct.
							var instanceRef = (object)instance;
							property.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ), null );
							instance = (T)instanceRef;
						}
						else
						{
							// Type is a class.
							property.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ), null );
						}
					}
				}
			}

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

		static List<T> DecodeList<T>( Variant data )
		{
			var list = new List<T>();

			var proxyArray = data as ProxyArray;
			if( proxyArray == null )
			{
				throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
			}

			foreach( var item in proxyArray )
			{
				list.Add( DecodeType<T>( item ) );
			}

			return list;
		}

		static Dictionary<TKey, TValue> DecodeDictionary<TKey, TValue>( Variant data )
		{
			var dict = new Dictionary<TKey, TValue>();
			var type = typeof( TKey );

			var proxyObject = data as ProxyObject;
			if( proxyObject == null )
			{
				throw new DecodeException( "Variant is expected to be a ProxyObject here, but it is not." );
			}

			foreach( var pair in proxyObject )
			{
				var k = (TKey)( type.IsEnum ? Enum.Parse( type, pair.Key ) : Convert.ChangeType( pair.Key, type ) );
				var v = DecodeType<TValue>( pair.Value );
				dict.Add( k, v );
			}

			return dict;
		}

		static T[] DecodeArray<T>( Variant data )
		{
			var arrayData = data as ProxyArray;
			if( arrayData == null )
			{
				throw new DecodeException( "Variant is expected to be a ProxyArray here, but it is not." );
			}

			var arraySize = arrayData.Count;
			var array = new T[arraySize];

			var i = 0;
			foreach( var item in arrayData )
			{
				array[i++] = DecodeType<T>( item );
			}

			return array;
		}

		static void DecodeMultiRankArray<T>( ProxyArray arrayData, Array array, int arrayRank, int[] indices )
		{
			var count = arrayData.Count;
			for( var i = 0; i < count; i++ )
			{
				indices[arrayRank - 1] = i;
				if( arrayRank < array.Rank )
				{
					DecodeMultiRankArray<T>( arrayData[i] as ProxyArray, array, arrayRank + 1, indices );
				}
				else
				{
					array.SetValue( DecodeType<T>( arrayData[i] ), indices );
				}
			}
		}

	}
}