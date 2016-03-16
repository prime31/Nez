using System;
using System.Reflection;


namespace Nez
{
	/// <summary>
	/// helper class to fetch property delegates
	/// </summary>
	class ReflectionUtils
	{
		public static FieldInfo getFieldInfo<T>( System.Object targetObject, string fieldName )
		{
			FieldInfo fieldInfo = null;
			var type = targetObject.GetType();
			do
			{
				fieldInfo = type.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
				type = type.BaseType;
			} while ( fieldInfo == null && type != null );

			return fieldInfo;
		}


		public static PropertyInfo getPropertyInfo( System.Object targetObject, string propertyName )
		{
			#if NETFX_CORE
			return targetObject.GetType().GetRuntimeProperty( propertyName );
			#else
			return targetObject.GetType().GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			#endif
		}


		public static MethodInfo getMethodInfo( System.Object targetObject, string methodName )
		{
			#if NETFX_CORE
			return targetObject.GetType().GetRuntimeMethod( propertyName );
			#else
			return targetObject.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			#endif
		}


		public static T createDelegate<T>( System.Object targetObject, MethodInfo methodInfo )
		{
			#if NETFX_CORE
			// Windows Phone/Store new API
			throw NotImplementedException();
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

			return createDelegate<T>( targetObject, propInfo.GetSetMethod( true ) );
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

			return createDelegate<T>( targetObject, propInfo.GetGetMethod( true ) );
		}

	}
}

