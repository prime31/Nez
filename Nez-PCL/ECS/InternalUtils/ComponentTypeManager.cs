using System;
using System.Collections.Generic;
using System.Reflection;


namespace Nez
{
	public static class ComponentTypeManager
	{
		private static Dictionary<Type, int> _componentTypesMask = new Dictionary<Type, int>();

		public static void add(Type type) 
		{
			int v;
			if( !_componentTypesMask.TryGetValue( type, out v ) )
			{
				_componentTypesMask[type] = _componentTypesMask.Count;
			}
		}

		public static int getIndexFor(Type type) 
		{
			int v = -1;
			_componentTypesMask.TryGetValue( type, out v );
			return v;
		}

		public static IEnumerable<Type> getTypesFromBits(BitSet bits)
		{
			foreach (KeyValuePair<Type, int> keyValuePair in _componentTypesMask)
			{
				if (bits.Get(keyValuePair.Value))
				{
					yield return keyValuePair.Key;
				}
			}   
		}

		public static void initialize()
		{
			var currentdomain = typeof(string).Assembly.GetType ("System.AppDomain").GetProperty("CurrentDomain").GetGetMethod().Invoke (null, new object[] {});
			var getassemblies = currentdomain.GetType ().GetMethod ("GetAssemblies", new Type[]{ });
			var assemblies = getassemblies.Invoke (currentdomain, new object[]{ }) as Assembly[];

			// HACK: make sure this works with PCL change below
			//foreach( var type in Assembly.GetEntryAssembly().GetTypes() )
			foreach (var assembly in assemblies) {
				foreach (var type in assembly.GetTypes()) {
					if (typeof(Component).IsAssignableFrom (type)) {
						add (type);
					}
				}
			}
		}
	}
}

