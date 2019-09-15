using System.Collections.Generic;
using System.Linq;


namespace Nez.UI
{
	public class ArraySelection<T> : Selection<T> where T : class
	{
		List<T> array;
		bool rangeSelect = true;
		int rangeStart;


		public ArraySelection(List<T> array)
		{
			this.array = array;
		}


		public override void Choose(T item)
		{
			Insist.IsNotNull(item, "item cannot be null");
			if (_isDisabled)
				return;

			var index = array.IndexOf(item);
			if (selected.Count > 0 && rangeSelect && multiple && InputUtils.IsShiftDown())
			{
				int oldRangeState = rangeStart;
				Snapshot();

				// Select new range.
				int start = rangeStart, end = index;
				if (start > end)
				{
					var temp = end;
					end = start;
					start = temp;
				}

				if (!InputUtils.IsControlDown())
					selected.Clear();
				for (int i = start; i <= end; i++)
					selected.Add(array[i]);

				if (FireChangeEvent())
				{
					rangeStart = oldRangeState;
					Revert();
				}

				Cleanup();
				return;
			}
			else
			{
				rangeStart = index;
			}

			base.Choose(item);
		}


		public bool GetRangeSelect()
		{
			return rangeSelect;
		}


		public void SetRangeSelect(bool rangeSelect)
		{
			this.rangeSelect = rangeSelect;
		}


		/// <summary>
		/// Removes objects from the selection that are no longer in the items array. If getRequired() is true and there is
		/// no selected item, the first item is selected.
		/// </summary>
		public void Validate()
		{
			if (array.Count == 0)
			{
				Clear();
				return;
			}

			for (var i = selected.Count - 1; i >= 0; i--)
			{
				var item = selected[i];
				if (!array.Contains(item))
					selected.Remove(item);
			}

			if (required && selected.Count == 0)
				Set(array.First());
		}
	}
}