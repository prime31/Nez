using System;


namespace Nez
{
	/// <summary>
	/// helper class to fetch property delegates
	/// </summary>
	class ReflectionUtils
	{
		/// <summary>
		/// either returns a super fast Delegate to set the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T setterForProperty<T>( System.Object targetObject, string propertyName )
		{
			// first get the property
			#if NETFX_CORE
			var propInfo = targetObject.GetType().GetRuntimeProperty( propertyName );
			#else
			var propInfo = targetObject.GetType().GetProperty( propertyName );
			#endif

			Assert.isNotNull( propInfo, "could not find property with name: " + propertyName );

			#if NETFX_CORE
			// Windows Phone/Store new API
			return (T)(object)propInfo.SetMethod.CreateDelegate( typeof( T ), targetObject );
			#else
			return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, propInfo.GetSetMethod() );
			#endif
		}


		/// <summary>
		/// either returns a super fast Delegate to get the given property or null if it couldn't be found
		/// via reflection
		/// </summary>
		public static T getterForProperty<T>( System.Object targetObject, string propertyName )
		{
			// first get the property
			#if NETFX_CORE
			var propInfo = targetObject.GetType().GetRuntimeProperty( propertyName );
			#else
			var propInfo = targetObject.GetType().GetProperty( propertyName );
			#endif

			Assert.isNotNull( propInfo, "could not find property with name: " + propertyName );

			#if NETFX_CORE
			// Windows Phone/Store new API
			return (T)(object)propInfo.GetMethod.CreateDelegate( typeof( T ), targetObject );
			#else
			return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, propInfo.GetGetMethod() );
			#endif
		}

	}
}

