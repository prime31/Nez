using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple DTO for managing a pair of objects
	/// </summary>
	public class Pair<T> : IEquatable<Pair<T>> where T : class
	{
		public T first;
		public T second;


		public Pair()
		{
		}


		public Pair( T first, T second )
		{
			this.first = first;
			this.second = second;
		}


		public void set( T first, T second )
		{
			this.first = first;
			this.second = second;
		}


		public void clear()
		{
			first = second = null;
		}


		public bool Equals( Pair<T> other )
		{
			if( other == null )
				return false;
			
			return EqualityComparer<T>.Default.Equals( first, other.first ) &&
				EqualityComparer<T>.Default.Equals( second, other.second );
		}


		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode( first ) * 37 + EqualityComparer<T>.Default.GetHashCode( second );
		}
	
	}
}

