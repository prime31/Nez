using System;
using System.Collections.Generic;
using System.Reflection;


namespace Nez
{
	public static class ComponentTypeManager
	{
		private static Dictionary<Type,int> _componentTypesMask = new Dictionary<Type, int>();


		public static void initialize()
		{
			var currentDomain = typeof( string ).Assembly.GetType( "System.AppDomain" ).GetProperty( "CurrentDomain" ).GetGetMethod().Invoke( null, new object[] { } );
			var getAssemblies = currentDomain.GetType().GetMethod( "GetAssemblies", new Type[]{ } );
			var assemblies = getAssemblies.Invoke( currentDomain, new object[]{ } ) as Assembly[];

			foreach( var assembly in assemblies )
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( typeof( Component ).IsAssignableFrom( type ) )
					{
						add( type );
					}
				}
			}
		}


		public static void add( Type type )
		{
			int v;
			if( !_componentTypesMask.TryGetValue( type, out v ) )
				_componentTypesMask[type] = _componentTypesMask.Count;
		}


		public static int getIndexFor( Type type )
		{
			var v = -1;
			_componentTypesMask.TryGetValue( type, out v );
			return v;
		}


		public static IEnumerable<Type> getTypesFromBits( BitSet bits )
		{
			foreach( var keyValuePair in _componentTypesMask )
			{
				if( bits.get( keyValuePair.Value ) )
					yield return keyValuePair.Key;
			}   
		}

	}
}

