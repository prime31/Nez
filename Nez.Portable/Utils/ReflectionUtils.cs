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
		public static Assembly getAssembly( Type type )
		{
#if NETFX_CORE
			return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
#endif
		}

		#region Fields

		public static FieldInfo getFieldInfo( object targetObject, string fieldName ) => getFieldInfo( targetObject.GetType(), fieldName );

		public static FieldInfo getFieldInfo( Type type, string fieldName )
		{
			FieldInfo fieldInfo = null;

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

		public static object getFieldValue( object targetObject, string fieldName )
		{
			var fieldInfo = getFieldInfo( targetObject, fieldName );
			return fieldInfo.GetValue( targetObject );
		}

		#endregion

		#region Properties

		public static PropertyInfo getPropertyInfo( object targetObject, string propertyName ) => getPropertyInfo( targetObject.GetType(), propertyName );

		public static PropertyInfo getPropertyInfo( Type type, string propertyName )
		{
#if NETFX_CORE
			return type.GetRuntimeProperty( propertyName );
#else
			return type.GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
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

		public static object getPropertyValue( object targetObject, string propertyName )
		{
			var propInfo = getPropertyInfo( targetObject, propertyName );
			var methodInfo = getPropertyGetter( propInfo );
			return methodInfo.Invoke( targetObject, new object[] { } );
		}

		/// <summary>
		/// either returns a super fast Delegate to set the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T setterForProperty<T>( object targetObject, string propertyName )
		{
			// first get the property
			var propInfo = getPropertyInfo( targetObject, propertyName );
			if( propInfo == null )
				return default( T );

			return createDelegate<T>( targetObject, propInfo.SetMethod );
		}

		/// <summary>
		/// either returns a super fast Delegate to get the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T getterForProperty<T>( object targetObject, string propertyName )
		{
			// first get the property
			var propInfo = getPropertyInfo( targetObject, propertyName );
			if( propInfo == null )
				return default( T );

			return createDelegate<T>( targetObject, propInfo.GetMethod );
		}

		#endregion

		#region Methods

		public static IEnumerable<MethodInfo> getMethods( Type type )
		{
#if NETFX_CORE
			return type.GetRuntimeMethods();
#else
			return type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
#endif
		}

		public static MethodInfo getMethodInfo( object targetObject, string methodName ) => getMethodInfo( targetObject.GetType(), methodName );

		public static MethodInfo getMethodInfo( object targetObject, string methodName, Type[] parameters ) => getMethodInfo( targetObject.GetType(), methodName, parameters );

		public static MethodInfo getMethodInfo( Type type, string methodName, Type[] parameters = null )
		{
#if NETFX_CORE
			if( parameters != null )
				return type.GetRuntimeMethod( methodName, parameters );

			foreach( var method in type.GetRuntimeMethods() )
				if( method.Name == methodName )
					return method;
			return null;
#else
			if( parameters == null )
				return type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			return type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, parameters, null );
#endif
		}

		#endregion

		public static T createDelegate<T>( object targetObject, MethodInfo methodInfo )
		{
#if NETFX_CORE
			return (T)(object)methodInfo.CreateDelegate( typeof( T ), targetObject );
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
		public static List<Type> getAllSubclasses( Type baseClassType, bool onlyIncludeParameterlessConstructors = false )
		{
			var typeList = new List<Type>();
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( type.IsSubclassOf( baseClassType ) && !type.IsAbstract )
					{
						if( onlyIncludeParameterlessConstructors )
						{
							if( type.GetConstructor( Type.EmptyTypes ) == null )
							{
								Debug.log( "no go: " + type.Name );
								continue;
							}
						}
						typeList.Add( type );
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
		public static List<Type> getAllTypesAssignableFrom( Type baseClassType, bool onlyIncludeParameterlessConstructors = false )
		{
			var typeList = new List<Type>();
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( baseClassType.IsAssignableFrom( type ) && !type.IsAbstract )
					{
						if( onlyIncludeParameterlessConstructors )
						{
							if( type.GetConstructor( Type.EmptyTypes ) == null )
								continue;
						}
						typeList.Add( type );
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
		public static bool isGenericTypeOrSubclassOfGenericType( Type type )
		{
			var currentType = type;
			while( currentType != null && currentType != typeof( object ) )
			{
				if( currentType.IsGenericType )
					return true;
				currentType = currentType.BaseType;
			}
			return false;
		}

	}
}

