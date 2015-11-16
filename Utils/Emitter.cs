using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple event emitter that is designed to have its generic contraint be either an int or an enum
	/// </summary>
	public class Emitter<T> where T : struct, IConvertible, IComparable, IFormattable
	{
		Dictionary<T,List<Action>> _messageTable;


		public Emitter()
		{
			_messageTable = new Dictionary<T,List<Action>>();
		}


		public Emitter( IEqualityComparer<T> customComparer )
		{
			_messageTable = new Dictionary<T,List<Action>>( customComparer );
		}


		public void addObserver( T eventType, Action handler )
		{
			List<Action> list = null;
			if( !_messageTable.TryGetValue( eventType, out list ) )
			{
				list = new List<Action>();
				_messageTable.Add( eventType, list );
			}

			Debug.assertIsFalse( list.Contains( handler ), "You are trying to add the same observer twice" );
			list.Add( handler );
		}


		public void removeObserver( T eventType, Action handler )
		{
			// we purposely do this in unsafe fashion so that it will throw an Exception if someone tries to remove a handler that
			// was never added
			_messageTable[eventType].Remove( handler );
		}


		public void emit( T eventType )
		{
			List<Action> list = null;
			if( _messageTable.TryGetValue( eventType, out list ) )
			{
				for( var i = list.Count - 1; i >= 0; i-- )
					list[i]();
			}
		}

	}
}

