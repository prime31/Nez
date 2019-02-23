using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nez.Persistence
{
	/// <summary>
	/// responsible for caching as much of the reflection calls as we can. Should be cleared after each encode/decode.
	/// </summary>
	class CacheResolver
	{
		Dictionary<string, object> _referenceTracker = new Dictionary<string, object>();
		Dictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();
		Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
		Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		static Dictionary<MemberInfo, bool> _memberInfoEncodeableCache = new Dictionary<MemberInfo, bool>();

		/// <summary>
		/// checks the <paramref name="memberInfo"/> custom attributes to see if it should be encoded/decoded
		/// </summary>
		/// <returns><c>true</c>, if member info encodeable or decodeable was ised, <c>false</c> otherwise.</returns>
		/// <param name="memberInfo">Member info.</param>
		/// <param name="isPublic">If set to <c>true</c> is public.</param>
		internal static bool IsMemberInfoEncodeableOrDecodeable( MemberInfo memberInfo, bool isPublic )
		{
			if( _memberInfoEncodeableCache.TryGetValue( memberInfo, out var isEncodeable ) )
				return isEncodeable;
				

			foreach( var attribute in memberInfo.GetCustomAttributes( true ) )
			{
				if( Json.excludeAttrType.IsInstanceOfType( attribute ) )
				{
					isPublic = false;
				}

				if( Json.includeAttrType.IsInstanceOfType( attribute ) )
				{
					isPublic = true;
				}
			}

			_memberInfoEncodeableCache[memberInfo] = isPublic;

			return isPublic;
		}

		public static void Flush() => _memberInfoEncodeableCache.Clear();

		internal void Clear()
		{
			_referenceTracker.Clear();
			_constructorCache.Clear();
			_fieldInfoCache.Clear();
			_propertyInfoCache.Clear();
			_memberInfoEncodeableCache.Clear();
		}

		internal void TrackReference<T>( string id, T instance ) => _referenceTracker[id] = instance;

		internal object GetReference( string refId ) => _referenceTracker[refId];

		/// <summary>
		/// Creates an instance of <paramref name="type"/> and caches the ConstructorInfo for future use
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="type">Type.</param>
		internal object CreateInstance( Type type )
		{
			// structs have no constructors present so just let Activator.CreateInstance make them
			if( type.IsValueType )
				return Activator.CreateInstance( type );

			if( _constructorCache.TryGetValue( type, out var constructor ) )
				return constructor.Invoke( null );

			constructor = type.GetConstructor( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null );
			_constructorCache[type] = constructor;
			return constructor.Invoke( null );
		}

		/// <summary>
		/// Gets the FieldInfo with <paramref name="name"/> or if that isnt found checks for any matching
		/// <seealso cref="DecodeAliasAttribute"/>
		/// </summary>
		/// <returns>The field.</returns>
		/// <param name="type">Type.</param>
		/// <param name="name">Name.</param>
		internal FieldInfo GetField( Type type, string name )
		{
			if( _fieldInfoCache.TryGetValue( type, out var map ) )
				if( map.TryGetValue( name, out var fieldInfo ) )
					return fieldInfo;

			if( map == null )
			{
				map = new Dictionary<string, FieldInfo>();
				_fieldInfoCache[type] = map;

				foreach( var field in type.GetFields( VariantConverter.instanceBindingFlags ) )
					map[field.Name] = field;

				if( map.TryGetValue( name, out var fieldInfo ) )
					return fieldInfo;
			}

			// last resort: check DecodeAliasAttributes
			return FindFieldFromDecodeAlias( type, name );
		}

		FieldInfo FindFieldFromDecodeAlias( Type type, string name )
		{
			foreach( var kvPair in _fieldInfoCache[type] )
			{
				foreach( var attribute in kvPair.Value.GetCustomAttributes( true ) )
				{
					if( VariantConverter.decodeAliasAttrType.IsInstanceOfType( attribute ) )
					{
						if( ( (DecodeAliasAttribute)attribute ).Contains( name ) )
						{
							return kvPair.Value;
						}
					}
				}
			}
			return null;
		}

		internal PropertyInfo GetProperty( Type type, string name )
		{
			if( _propertyInfoCache.TryGetValue( type, out var map ) )
				if( map.TryGetValue( name, out var propInfo ) )
					return propInfo;

			if( map == null )
			{
				map = new Dictionary<string, PropertyInfo>();
				_propertyInfoCache[type] = map;

				foreach( var prop in type.GetProperties( VariantConverter.instanceBindingFlags ) )
					map[prop.Name] = prop;

				if( map.TryGetValue( name, out var propInfo ) )
					return propInfo;
			}

			return FindPropertyFromDecodeAlias( type, name );
		}

		PropertyInfo FindPropertyFromDecodeAlias( Type type, string name )
		{
			foreach( var kvPair in _propertyInfoCache[type] )
			{
				foreach( var attribute in kvPair.Value.GetCustomAttributes( true ) )
				{
					if( VariantConverter.decodeAliasAttrType.IsInstanceOfType( attribute ) )
					{
						if( ( (DecodeAliasAttribute)attribute ).Contains( name ) )
						{
							return kvPair.Value;
						}
					}
				}
			}
			return null;
		}

	}
}
