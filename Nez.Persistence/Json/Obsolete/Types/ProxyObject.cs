using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace Nez.Persistence
{
	public sealed class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>
	{
		readonly Dictionary<string, Variant> dict;

		public string TypeHint
		{
			get
			{
				if( TryGetValue( Json.TypeHintPropertyName, out var item ) )
				{
					return item.ToString( CultureInfo.InvariantCulture );
				}

				return null;
			}
		}

		public string ReferenceId
		{
			get
			{
				if( TryGetValue( Json.RefPropertyName, out var item ) )
				{
					return item.ToString( CultureInfo.InvariantCulture );
				}

				return null;
			}
		}

		public string InstanceId
		{
			get
			{
				if( TryGetValue( Json.IdPropertyName, out var item ) )
				{
					return item.ToString( CultureInfo.InvariantCulture );
				}

				return null;
			}
		}


		public ProxyObject()
		{
			dict = new Dictionary<string, Variant>();
		}

		IEnumerator<KeyValuePair<string, Variant>> IEnumerable<KeyValuePair<string, Variant>>.GetEnumerator() => dict.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();

		public void Add( string key, Variant item ) => dict.Add( key, item );

		public bool TryGetValue( string key, out Variant item ) => dict.TryGetValue( key, out item );

		public override Variant this[string key]
		{
			get => dict[key];
			set => dict[key] = value;
		}

		public int Count => dict.Count;

		public Dictionary<string, Variant>.KeyCollection Keys => dict.Keys;

		public Dictionary<string, Variant>.ValueCollection Values => dict.Values;

	}
}
