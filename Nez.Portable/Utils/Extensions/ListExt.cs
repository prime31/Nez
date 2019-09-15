using System.Collections.Generic;


namespace Nez
{
	public static class ListExt
	{
		/// <summary>
		/// shuffles the list in place
		/// </summary>
		/// <param name="list">List.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void Shuffle<T>(this IList<T> list)
		{
			var n = list.Count;
			while (n > 1)
			{
				n--;
				int k = Random.Range(0, n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}


		/// <summary>
		/// returns false if the item is already in the List and true if it was successfully added.
		/// </summary>
		/// <returns>The if not present.</returns>
		/// <param name="list">List.</param>
		/// <param name="item">Item.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool AddIfNotPresent<T>(this IList<T> list, T item)
		{
			if (list.Contains(item))
				return false;

			list.Add(item);
			return true;
		}


		/// <summary>
		/// returns the last item in the list. List should have at least one item.
		/// </summary>
		/// <param name="list">List.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T LastItem<T>(this IList<T> list)
		{
			return list[list.Count - 1];
		}


		/// <summary>
		/// gets a random item from the list. Does not empty check the list!
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="list">List.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T RandomItem<T>(this IList<T> list)
		{
			return list[Random.Range(0, list.Count)];
		}


		/// <summary>
		/// gets random items from the list. Does not empty check the list or verify that list count is greater than item count! The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="list">List.</param>
		/// <param name="itemCount">The number of random items to return from the list.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> RandomItems<T>(this IList<T> list, int itemCount)
		{
			var set = new HashSet<T>();
			while (set.Count != itemCount)
			{
				var item = list.RandomItem();
				if (!set.Contains(item))
					set.Add(item);
			}

			var items = ListPool<T>.Obtain();
			items.AddRange(set);
			return items;
		}
	}
}