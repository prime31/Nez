using System;
using System.Collections.Generic;
using System.Reflection;


namespace Nez
{
	public static class ComponentTypeManager
	{
		static Dictionary<Type,int> _componentTypesMask = new Dictionary<Type,int>();


		public static void add( Type type )
		{
			int v;
			if( !_componentTypesMask.TryGetValue( type, out v ) )
				_componentTypesMask[type] = _componentTypesMask.Count;
		}


		public static int getIndexFor( Type type )
		{
			var v = -1;
			if( !_componentTypesMask.TryGetValue( type, out v ) )
			{
				add( type );
				_componentTypesMask.TryGetValue( type, out v );
			}

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

