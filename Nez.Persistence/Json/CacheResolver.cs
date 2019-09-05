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
		static Type decodeAliasAttrType = typeof(DecodeAliasAttribute);
		Dictionary<string, object> _referenceTracker = new Dictionary<string, object>();
		Dictionary<Type, ConstructorInfo> _constructorCache = new Dictionary<Type, ConstructorInfo>();

		Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfoCache =
			new Dictionary<Type, Dictionary<string, FieldInfo>>();

		Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyInfoCache =
			new Dictionary<Type, Dictionary<string, PropertyInfo>>();

		Dictionary<MemberInfo, bool> _memberInfoEncodeableCache = new Dictionary<MemberInfo, bool>();

		/// <summary>
		/// checks the <paramref name="memberInfo"/> custom attributes to see if it should be encoded/decoded
		/// </summary>
		/// <returns><c>true</c>, if member info encodeable or decodeable was ised, <c>false</c> otherwise.</returns>
		/// <param name="memberInfo">Member info.</param>
		/// <param name="isPublic">If set to <c>true</c> is public.</param>
		internal bool IsMemberInfoEncodeableOrDecodeable(MemberInfo memberInfo, bool isPublic)
		{
			if (_memberInfoEncodeableCache.TryGetValue(memberInfo, out var isEncodeable))
			{
				return isEncodeable;
			}

			foreach (var attribute in memberInfo.GetCustomAttributes(true))
			{
				if (JsonConstants.excludeAttrType.IsInstanceOfType(attribute))
				{
					isPublic = false;
				}

				if (JsonConstants.includeAttrType.IsInstanceOfType(attribute))
				{
					isPublic = true;
				}
			}

			_memberInfoEncodeableCache[memberInfo] = isPublic;

			return isPublic;
		}

		/// <summary>
		/// tracks a reference to an object for later fetching via <seealso cref="ResolveReference"/>
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="instance">Instance.</param>
		internal void TrackReference(string id, object instance) => _referenceTracker[id] = instance;

		/// <summary>
		/// fetches a cached reference to an object
		/// </summary>
		/// <returns>The reference.</returns>
		/// <param name="refId">Reference identifier.</param>
		internal object ResolveReference(string refId) => _referenceTracker[refId];

		/// <summary>
		/// Creates an instance of <paramref name="type"/> and caches the ConstructorInfo for future use
		/// </summary>
		/// <returns>The instance.</returns>
		/// <param name="type">Type.</param>
		internal object CreateInstance(Type type)
		{
			// structs have no constructors present so just let Activator.CreateInstance make them
			if (type.IsValueType)
				return Activator.CreateInstance(type);

			if (_constructorCache.TryGetValue(type, out var constructor))
				return constructor.Invoke(null);

			constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				null, Type.EmptyTypes, null);
			_constructorCache[type] = constructor;
			return constructor.Invoke(null);
		}


		#region FieldInfo methods

		/// <summary>
		/// returns all the encodeable fields based on the attributes set on each
		/// </summary>
		/// <returns>The encodable fields for type.</returns>
		/// <param name="type">Type.</param>
		internal IEnumerable<FieldInfo> GetEncodableFieldsForType(Type type)
		{
			// cleanse the fields based on our attributes
			foreach (var kvPair in GetFieldInfoCache(type))
			{
				if (IsMemberInfoEncodeableOrDecodeable(kvPair.Value, kvPair.Value.IsPublic))
					yield return kvPair.Value;
			}
		}

		/// <summary>
		/// Gets the FieldInfo with <paramref name="name"/> or if that isnt found checks for any matching
		/// <seealso cref="DecodeAliasAttribute"/>
		/// </summary>
		/// <returns>The field.</returns>
		/// <param name="type">Type.</param>
		/// <param name="name">Name.</param>
		internal FieldInfo GetField(Type type, string name)
		{
			var map = GetFieldInfoCache(type);
			if (map.TryGetValue(name, out var fieldInfo))
			{
				return fieldInfo;
			}

			// last resort: check DecodeAliasAttributes
			return FindFieldFromDecodeAlias(type, name);
		}

		Dictionary<string, FieldInfo> GetFieldInfoCache(Type type)
		{
			if (_fieldInfoCache.TryGetValue(type, out var map))
			{
				return map;
			}

			// no data cached. Fetch and populate it now
			map = new Dictionary<string, FieldInfo>();
			_fieldInfoCache[type] = map;

			var allFields = type.GetFields(JsonConstants.instanceBindingFlags);
			foreach (var field in allFields)
			{
				map[field.Name] = field;
			}

			return map;
		}

		FieldInfo FindFieldFromDecodeAlias(Type type, string name)
		{
			foreach (var kvPair in _fieldInfoCache[type])
			{
				foreach (var attribute in kvPair.Value.GetCustomAttributes(true))
				{
					if (decodeAliasAttrType.IsInstanceOfType(attribute))
					{
						if (((DecodeAliasAttribute) attribute).Contains(name))
						{
							return kvPair.Value;
						}
					}
				}
			}

			return null;
		}

		#endregion


		#region PropertyInfo methods

		/// <summary>
		/// returns all the encodeable properties based on the attributes set on each
		/// </summary>
		/// <returns>The encodable properties for type.</returns>
		/// <param name="type">Type.</param>
		internal IEnumerable<PropertyInfo> GetEncodablePropertiesForType(Type type)
		{
			// cleanse the properties based on our attributes
			foreach (var kvPair in GetPropertyInfoCache(type))
			{
				if (IsMemberInfoEncodeableOrDecodeable(kvPair.Value, false))
				{
					yield return kvPair.Value;
				}
			}
		}

		/// <summary>
		/// first checks the property cache. If nothing is found it will check for DecoedeAliases
		/// </summary>
		/// <returns>The encodeable property.</returns>
		/// <param name="type">Type.</param>
		/// <param name="name">Name.</param>
		internal PropertyInfo GetProperty(Type type, string name)
		{
			var map = GetPropertyInfoCache(type);
			if (map.TryGetValue(name, out var propInfo))
			{
				return propInfo;
			}

			// last resort: check DecodeAliasAttributes
			return FindPropertyFromDecodeAlias(type, name);
		}

		/// <summary>
		/// fetches the PropertyInfo cache, creating it if it doesnt exist already
		/// </summary>
		/// <returns>The property info cache.</returns>
		/// <param name="type">Type.</param>
		Dictionary<string, PropertyInfo> GetPropertyInfoCache(Type type)
		{
			if (_propertyInfoCache.TryGetValue(type, out var map))
			{
				return map;
			}

			// no data cached. Fetch and populate it now
			map = new Dictionary<string, PropertyInfo>();
			_propertyInfoCache[type] = map;

			var allProps = type.GetProperties(JsonConstants.instanceBindingFlags);
			foreach (var prop in allProps)
			{
				// skip properties that have index parameters (i.e. array accessor)
				if (prop.GetIndexParameters().Length > 0)
				{
					continue;
				}

				// skip properties that didnt opt-in

				map[prop.Name] = prop;
			}

			return map;
		}

		/// <summary>
		/// looks for a property matching the name based on the DecodeAlias attribute
		/// </summary>
		/// <returns>The property from decode alias.</returns>
		/// <param name="type">Type.</param>
		/// <param name="name">Name.</param>
		PropertyInfo FindPropertyFromDecodeAlias(Type type, string name)
		{
			foreach (var kvPair in _propertyInfoCache[type])
			{
				foreach (var attribute in kvPair.Value.GetCustomAttributes(true))
				{
					if (decodeAliasAttrType.IsInstanceOfType(attribute))
					{
						if (((DecodeAliasAttribute) attribute).Contains(name))
						{
							return kvPair.Value;
						}
					}
				}
			}

			return null;
		}

		#endregion
	}
}