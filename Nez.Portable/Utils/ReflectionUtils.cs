#define NETFX_CORE
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Nez
{
	/// <summary>
	/// helper class to fetch property delegates
	/// </summary>
	public class ReflectionUtils
	{
		public static Assembly GetAssembly(Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
#endif
		}

		#region Fields

		public static FieldInfo GetFieldInfo(object targetObject, string fieldName) =>
			GetFieldInfo(targetObject.GetType(), fieldName);

		public static FieldInfo GetFieldInfo(Type type, string fieldName)
		{
			FieldInfo fieldInfo = null;

#if NETFX_CORE
			foreach (var fi in type.GetRuntimeFields())
			{
				if (fi.Name == fieldName)
				{
					fieldInfo = fi;
					break;
				}
			}
#else
			do
			{
				fieldInfo =
 type.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
				type = type.BaseType;
			} while ( fieldInfo == null && type != null );
#endif

			return fieldInfo;
		}

		public static IEnumerable<FieldInfo> GetFields(Type type)
		{
#if NETFX_CORE
			return type.GetRuntimeFields();
#else
			return type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
#endif
		}

		public static object GetFieldValue(object targetObject, string fieldName)
		{
			var fieldInfo = GetFieldInfo(targetObject, fieldName);
			return fieldInfo.GetValue(targetObject);
		}

		#endregion

		#region Properties

		public static PropertyInfo GetPropertyInfo(object targetObject, string propertyName) =>
			GetPropertyInfo(targetObject.GetType(), propertyName);

		public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
#if NETFX_CORE
			return type.GetRuntimeProperty(propertyName);
#else
			return type.GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
#endif
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
#if NETFX_CORE
			return type.GetRuntimeProperties();
#else
			return type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
#endif
		}

		public static MethodInfo GetPropertyGetter(PropertyInfo prop)
		{
#if NETFX_CORE
			return prop.GetMethod;
#else
			return prop.GetGetMethod( true );
#endif
		}

		public static MethodInfo GetPropertySetter(PropertyInfo prop)
		{
#if NETFX_CORE
			return prop.SetMethod;
#else
			return prop.GetSetMethod( true );
#endif
		}

		public static object GetPropertyValue(object targetObject, string propertyName)
		{
			var propInfo = GetPropertyInfo(targetObject, propertyName);
			var methodInfo = GetPropertyGetter(propInfo);
			return methodInfo.Invoke(targetObject, new object[] { });
		}

		/// <summary>
		/// either returns a super fast Delegate to set the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T SetterForProperty<T>(object targetObject, string propertyName)
		{
			// first get the property
			var propInfo = GetPropertyInfo(targetObject, propertyName);
			if (propInfo == null)
				return default(T);

			return CreateDelegate<T>(targetObject, propInfo.SetMethod);
		}

		/// <summary>
		/// either returns a super fast Delegate to get the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T GetterForProperty<T>(object targetObject, string propertyName)
		{
			// first get the property
			var propInfo = GetPropertyInfo(targetObject, propertyName);
			if (propInfo == null)
				return default(T);

			return CreateDelegate<T>(targetObject, propInfo.GetMethod);
		}

		#endregion

		#region Methods

		public static IEnumerable<MethodInfo> GetMethods(Type type)
		{
#if NETFX_CORE
			return type.GetRuntimeMethods();
#else
			return type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
#endif
		}

		public static MethodInfo GetMethodInfo(object targetObject, string methodName) =>
			GetMethodInfo(targetObject.GetType(), methodName);

		public static MethodInfo GetMethodInfo(object targetObject, string methodName, Type[] parameters) =>
			GetMethodInfo(targetObject.GetType(), methodName, parameters);

		public static MethodInfo GetMethodInfo(Type type, string methodName, Type[] parameters = null)
		{
#if NETFX_CORE
			if (parameters != null)
				return type.GetRuntimeMethod(methodName, parameters);

			foreach (var method in type.GetRuntimeMethods())
				if (method.Name == methodName)
					return method;

			return null;
#else
			if( parameters == null )
				return type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			return type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, parameters, null );
#endif
		}

		#endregion

		public static T CreateDelegate<T>(object targetObject, MethodInfo methodInfo)
		{
#if NETFX_CORE
			return (T) (object) methodInfo.CreateDelegate(typeof(T), targetObject);
#else
			return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, methodInfo );
#endif
		}

		/// <summary>
		/// gets all subclasses of <paramref name="baseClassType"> optionally filtering only for those with
		/// a parameterless constructor. Abstract Types will not be returned.
		/// </summary>
		/// <param name="baseClassType"></param>
		/// <param name="onlyIncludeParameterlessConstructors"></param>
		/// <returns></returns>
		public static List<Type> GetAllSubclasses(Type baseClassType, bool onlyIncludeParameterlessConstructors = false)
		{
			var typeList = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(baseClassType) && !type.IsAbstract)
					{
						if (onlyIncludeParameterlessConstructors)
						{
							if (type.GetConstructor(Type.EmptyTypes) == null)
							{
								Debug.Log("no go: " + type.Name);
								continue;
							}
						}

						typeList.Add(type);
					}
				}
			}

			return typeList;
		}

		/// <summary>
		/// gets all Types assignable from <paramref name="baseClassType"> optionally filtering only for those with
		/// a parameterless constructor. Abstract Types will not be returned.
		/// </summary>
		/// <param name="baseClassType"></param>
		/// <param name="onlyIncludeParameterlessConstructors"></param>
		/// <returns></returns>
		public static List<Type> GetAllTypesAssignableFrom(Type baseClassType,
		                                                   bool onlyIncludeParameterlessConstructors = false)
		{
			var typeList = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (baseClassType.IsAssignableFrom(type) && !type.IsAbstract)
					{
						if (onlyIncludeParameterlessConstructors)
						{
							if (type.GetConstructor(Type.EmptyTypes) == null)
								continue;
						}

						typeList.Add(type);
					}
				}
			}

			return typeList;
		}

		/// <summary>
		/// checks <paramref name="type"/> to see if it or any base class in the chain IsGenericType
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsGenericTypeOrSubclassOfGenericType(Type type)
		{
			var currentType = type;
			while (currentType != null && currentType != typeof(object))
			{
				if (currentType.IsGenericType)
					return true;

				currentType = currentType.BaseType;
			}

			return false;
		}
	}
}