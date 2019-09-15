using System;
using System.Collections.Generic;


namespace Nez
{
	public static class ComponentTypeManager
	{
		static Dictionary<Type, int> _componentTypesMask = new Dictionary<Type, int>();


		public static void Add(Type type)
		{
			int v;
			if (!_componentTypesMask.TryGetValue(type, out v))
				_componentTypesMask[type] = _componentTypesMask.Count;
		}


		public static int GetIndexFor(Type type)
		{
			var v = -1;
			if (!_componentTypesMask.TryGetValue(type, out v))
			{
				Add(type);
				_componentTypesMask.TryGetValue(type, out v);
			}

			return v;
		}


		public static IEnumerable<Type> GetTypesFromBits(BitSet bits)
		{
			foreach (var keyValuePair in _componentTypesMask)
			{
				if (bits.Get(keyValuePair.Value))
					yield return keyValuePair.Key;
			}
		}
	}
}