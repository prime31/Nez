using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Nez.Persistance
{
	public class VariantConverter
	{
		const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		const BindingFlags staticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		static readonly MethodInfo decodeTypeMethod = typeof( VariantConverter ).GetMethod( "DecodeType", staticBindingFlags );
		static readonly MethodInfo decodeListMethod = typeof( VariantConverter ).GetMethod( "DecodeList", staticBindingFlags );
		static readonly MethodInfo decodeDictionaryMethod = typeof( VariantConverter ).GetMethod( "DecodeDictionary", staticBindingFlags );
		static readonly MethodInfo decodeArrayMethod = typeof( VariantConverter ).GetMethod( "DecodeArray", staticBindingFlags );
		static readonly MethodInfo decodeMultiRankArrayMethod = typeof( VariantConverter ).GetMethod( "DecodeMultiRankArray", staticBindingFlags );

		static readonly Type decodeAliasAttrType = typeof( DecodeAliasAttribute );
		static readonly Type afterDecodeAttrType = typeof( AfterDecodeAttribute );

		static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		static Dictionary<string, object> _referenceTracker = new Dictionary<string, object>();


		public static T Make<T>( Variant data )
		{
			var item = DecodeType<T>( data );
			_referenceTracker.Clear();
			return item;
		}

		public static void MakeInto<T>( Variant data, out T item )
		{
			item = DecodeType<T>( data );
			_referenceTracker.Clear();
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
				return (T)_referenceTracker[refId];
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
					instance = (T)Activator.CreateInstance( makeType );
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
				instance = Activator.CreateInstance<T>();
			}

			// if there is an instanceId, cache the object in case any other objects are referencing it
			var id = proxyObject.InstanceId;
			if( id != null )
			{
				_referenceTracker[id] = instance;
			}

			// Now decode fields and properties.
			foreach( var pair in (ProxyObject)data )
			{
				var field = type.GetField( pair.Key, instanceBindingFlags );

				// If the field doesn't exist, search through any [DecodeAlias] attributes.
				if( field == null )
				{
					var fields = type.GetFields( instanceBindingFlags );
					foreach( var fieldInfo in fields )
					{
						foreach( var attribute in fieldInfo.GetCustomAttributes( true ) )
						{
							if( decodeAliasAttrType.IsInstanceOfType( attribute ) )
							{
								if( ( (DecodeAliasAttribute)attribute ).Contains( pair.Key ) )
								{
									field = fieldInfo;
									break;
								}
							}
						}
					}
				}

				if( field != null )
				{
					var shouldDecode = field.IsPublic;
					foreach( var attribute in field.GetCustomAttributes( true ) )
					{
						if( Json.excludeAttrType.IsInstanceOfType( attribute ) )
						{
							shouldDecode = false;
						}

						if( Json.includeAttrType.IsInstanceOfType( attribute ) )
						{
							shouldDecode = true;
						}
					}

					if( shouldDecode )
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( field.FieldType );
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
				}

				var property = type.GetProperty( pair.Key, instanceBindingFlags );

				// If the property doesn't exist, search through any [DecodeAlias] attributes.
				if( property == null )
				{
					var properties = type.GetProperties( instanceBindingFlags );
					foreach( var propertyInfo in properties )
					{
						foreach( var attribute in propertyInfo.GetCustomAttributes( false ) )
						{
							if( decodeAliasAttrType.IsInstanceOfType( attribute ) )
							{
								if( ( (DecodeAliasAttribute)attribute ).Contains( pair.Key ) )
								{
									property = propertyInfo;
									break;
								}
							}
						}
					}
				}

				if( property != null )
				{
					if( property.CanWrite && property.IsDefined( Json.includeAttrType ) )
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( new Type[] { property.PropertyType } );
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