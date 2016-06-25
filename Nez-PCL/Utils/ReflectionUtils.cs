#define NETFX_CORE
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Nez
{
	/// <summary>
	/// helper class to fetch property delegates
	/// </summary>
	class ReflectionUtils
	{
		public static FieldInfo getFieldInfo( System.Object targetObject, string fieldName )
		{
			FieldInfo fieldInfo = null;
			var type = targetObject.GetType();

			#if NETFX_CORE
			foreach( var fi in type.GetRuntimeFields() )
			{
				if( fi.Name == fieldName )
				{
					fieldInfo = fi;
					break;
				}
			}
			#else
			do
			{
				fieldInfo = type.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
				type = type.BaseType;
			} while ( fieldInfo == null && type != null );
			#endif

			return fieldInfo;
		}


		public static IEnumerable<FieldInfo> getFields( Type type )
		{
			#if NETFX_CORE
			return type.GetRuntimeFields();
			#else
			return type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			#endif
		}


		public static PropertyInfo getPropertyInfo( System.Object targetObject, string propertyName )
		{
			#if NETFX_CORE
			return targetObject.GetType().GetRuntimeProperty( propertyName );
			#else
			return targetObject.GetType().GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			#endif
		}


		public static IEnumerable<PropertyInfo> getProperties( Type type )
		{
			#if NETFX_CORE
			return type.GetRuntimeProperties();
			#else
			return type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			#endif
		}


		public static MethodInfo getPropertyGetter( PropertyInfo prop )
		{
			#if NETFX_CORE
			return prop.GetMethod;
			#else
			return prop.GetGetMethod( true );
			#endif
		}


		public static MethodInfo getPropertySetter( PropertyInfo prop )
		{
			#if NETFX_CORE
			return prop.SetMethod;
			#else
			return prop.GetSetMethod( true );
			#endif
		}


		public static IEnumerable<MethodInfo> getMethods( Type type )
		{
			#if NETFX_CORE
			return type.GetRuntimeMethods();
			#else
			return type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			#endif
		}


		public static MethodInfo getMethodInfo( System.Object targetObject, string methodName )
		{
			#if NETFX_CORE
			foreach( var method in targetObject.GetType().GetRuntimeMethods() )
				if( method.Name == methodName )
					return method;
			return null;
			#else
			return targetObject.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			#endif
		}


		public static T createDelegate<T>( System.Object targetObject, MethodInfo methodInfo )
		{
			#if NETFX_CORE
			return (T)(object)methodInfo.CreateDelegate( typeof( T ), targetObject );
			#else
			return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, methodInfo );
			#endif
		}

		
		/// <summary>
		/// either returns a super fast Delegate to set the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T setterForProperty<T>( System.Object targetObject, string propertyName )
		{
			// first get the property
			var propInfo = getPropertyInfo( targetObject, propertyName );
			if( propInfo == null )
				return default(T);

			return createDelegate<T>( targetObject, propInfo.SetMethod );
		}


		/// <summary>
		/// either returns a super fast Delegate to get the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T getterForProperty<T>( System.Object targetObject, string propertyName )
		{
			// first get the property
			var propInfo = getPropertyInfo( targetObject, propertyName );
			if( propInfo == null )
				return default(T);

			return createDelegate<T>( targetObject, propInfo.SetMethod );
		}

	}
}

