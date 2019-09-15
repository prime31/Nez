using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple DTO for managing a pair of objects
	/// </summary>
	public struct Pair<T> : IEquatable<Pair<T>> where T : class
	{
		public T First;
		public T Second;


		public Pair(T first, T second)
		{
			First = first;
			Second = second;
		}


		public void Clear()
		{
			First = Second = null;
		}


		public bool Equals(Pair<T> other)
		{
			// these two ways should be functionaly equivalent
			return First == other.First && Second == other.Second;

			//return EqualityComparer<T>.Default.Equals( first, other.first ) &&
			//	EqualityComparer<T>.Default.Equals( second, other.second );
		}


		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode(First) * 37 +
			       EqualityComparer<T>.Default.GetHashCode(Second);
		}
	}
}