using System.Collections.Generic;
using System.Linq;


namespace Nez.UI
{
	public class Selection<T> where T : class
	{
		Element element;
		protected List<T> selected = new List<T>();
		List<T> old = new List<T>();
		protected bool _isDisabled;
		bool toggle;
		protected bool multiple;
		protected bool required;
		bool programmaticChangeEvents = true;
		T lastSelected;


		/// <summary>
		/// An Element to fire ChangeEvent on when the selection changes, or null
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="element">element.</param>
		public Element SetElement(Element element)
		{
			this.element = element;
			return element;
		}


		/// <summary>
		/// Selects or deselects the specified item based on how the selection is configured, whether ctrl is currently pressed, etc.
		/// This is typically invoked by user interaction.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void Choose(T item)
		{
			Insist.IsNotNull(item, "item cannot be null");
			if (_isDisabled)
				return;

			Snapshot();

			try
			{
				if ((toggle || (!required && selected.Count == 1) || InputUtils.IsControlDown()) &&
				    selected.Contains(item))
				{
					if (required && selected.Count == 1)
						return;

					selected.Remove(item);
					lastSelected = null;
				}
				else
				{
					bool modified = false;
					if (!multiple || (!toggle && !InputUtils.IsControlDown()))
					{
						if (selected.Count == 1 && selected.Contains(item))
							return;

						modified = selected.Count > 0;
						selected.Clear();
					}

					if (!selected.AddIfNotPresent(item) && !modified)
						return;

					lastSelected = item;
				}

				if (FireChangeEvent())
					Revert();
				else
					Changed();
			}
			finally
			{
				Cleanup();
			}
		}


		public bool HasItems()
		{
			return selected.Count > 0;
		}


		public bool IsEmpty()
		{
			return selected.Count == 0;
		}


		public int Size()
		{
			return selected.Count;
		}


		public List<T> Items()
		{
			return selected;
		}


		/// <summary>
		/// Returns the first selected item, or null
		/// </summary>
		public T First()
		{
			return selected.FirstOrDefault();
		}


		protected void Snapshot()
		{
			old.Clear();
			old.AddRange(selected);
		}


		protected void Revert()
		{
			selected.Clear();
			selected.AddRange(old);
		}


		protected void Cleanup()
		{
			old.Clear();
		}


		/// <summary>
		/// Sets the selection to only the specified item
		/// </summary>
		/// <param name="item">Item.</param>
		public Selection<T> Set(T item)
		{
			Insist.IsNotNull(item, "item cannot be null.");

			if (selected.Count == 1 && selected.First() == item)
				return this;

			Snapshot();
			selected.Clear();
			selected.Add(item);

			if (programmaticChangeEvents && FireChangeEvent())
			{
				Revert();
			}
			else
			{
				lastSelected = item;
				Changed();
			}

			Cleanup();
			return this;
		}


		public Selection<T> SetAll(List<T> items)
		{
			var added = false;
			Snapshot();
			lastSelected = null;
			selected.Clear();
			for (var i = 0; i < items.Count; i++)
			{
				var item = items[i];
				Insist.IsNotNull(item, "item cannot be null");
				added = selected.AddIfNotPresent(item);
			}

			if (added)
			{
				if (programmaticChangeEvents && FireChangeEvent())
				{
					Revert();
				}
				else if (items.Count > 0)
				{
					lastSelected = items.Last();
					Changed();
				}
			}

			Cleanup();
			return this;
		}


		/// <summary>
		/// Adds the item to the selection
		/// </summary>
		/// <param name="item">Item.</param>
		public void Add(T item)
		{
			Insist.IsNotNull(item, "item cannot be null");
			if (!selected.AddIfNotPresent(item))
				return;

			if (programmaticChangeEvents && FireChangeEvent())
			{
				selected.Remove(item);
			}
			else
			{
				lastSelected = item;
				Changed();
			}
		}


		public void AddAll(List<T> items)
		{
			var added = false;
			Snapshot();
			for (var i = 0; i < items.Count; i++)
			{
				var item = items[i];
				Insist.IsNotNull(item, "item cannot be null");
				added = selected.AddIfNotPresent(item);
			}

			if (added)
			{
				if (programmaticChangeEvents && FireChangeEvent())
				{
					Revert();
				}
				else
				{
					lastSelected = items.LastOrDefault();
					Changed();
				}
			}

			Cleanup();
		}


		public void Remove(T item)
		{
			Insist.IsNotNull(item, "item cannot be null");
			if (!selected.Remove(item))
				return;

			if (programmaticChangeEvents && FireChangeEvent())
			{
				selected.Add(item);
			}
			else
			{
				lastSelected = null;
				Changed();
			}
		}


		public void RemoveAll(List<T> items)
		{
			var removed = false;
			Snapshot();
			for (var i = 0; i < items.Count; i++)
			{
				var item = items[i];
				Insist.IsNotNull(item, "item cannot be null");
				removed = selected.Remove(item);
			}

			if (removed)
			{
				if (programmaticChangeEvents && FireChangeEvent())
				{
					Revert();
				}
				else
				{
					lastSelected = null;
					Changed();
				}
			}

			Cleanup();
		}


		public void Clear()
		{
			if (selected.Count == 0)
				return;

			Snapshot();
			selected.Clear();
			if (programmaticChangeEvents && FireChangeEvent())
			{
				Revert();
			}
			else
			{
				lastSelected = null;
				Changed();
			}

			Cleanup();
		}


		/// <summary>
		/// Called after the selection changes. The default implementation does nothing.
		/// </summary>
		protected virtual void Changed()
		{
		}


		/// <summary>
		/// Fires a change event on the selection's Element, if any. Called internally when the selection changes, depending on
		/// setProgrammaticChangeEvents(bool)
		/// </summary>
		public bool FireChangeEvent()
		{
			if (element == null)
				return false;

			// TODO: if actual events are ever used switch over to this
			//var changeEvent = Pools.obtain( ChangeEvent.class);
			//try {
			//	return actor.fire(changeEvent);
			//} finally {
			//	Pools.free(changeEvent);
			//}

			return false;
		}


		public bool Contains(T item)
		{
			if (item == null)
				return false;

			return selected.Contains(item);
		}


		/// <summary>
		/// Makes a best effort to return the last item selected, else returns an arbitrary item or null if the selection is empty.
		/// </summary>
		/// <returns>The last selected.</returns>
		public T GetLastSelected()
		{
			if (lastSelected != null)
				return lastSelected;

			return selected.FirstOrDefault();
		}


		/// <summary>
		/// If true, prevents choose(Object) from changing the selection. Default is false.
		/// </summary>
		/// <param name="isDisabled">Is disabled.</param>
		public Selection<T> SetDisabled(bool isDisabled)
		{
			_isDisabled = isDisabled;
			return this;
		}


		public bool IsDisabled()
		{
			return _isDisabled;
		}


		public bool GetToggle()
		{
			return toggle;
		}


		/// <summary>
		/// If true, prevents choose(Object) from clearing the selection. Default is false.
		/// </summary>
		/// <param name="toggle">Toggle.</param>
		public Selection<T> SetToggle(bool toggle)
		{
			this.toggle = toggle;
			return this;
		}


		public bool GetMultiple()
		{
			return multiple;
		}


		/// <summary>
		/// If true, allows choose(Object) to select multiple items. Default is false.
		/// </summary>
		/// <param name="multiple">Multiple.</param>
		public Selection<T> SetMultiple(bool multiple)
		{
			this.multiple = multiple;
			return this;
		}


		public bool GetRequired()
		{
			return required;
		}


		/// <summary>
		/// If true, prevents choose(Object) from selecting none. Default is false.
		/// </summary>
		/// <param name="required">Required.</param>
		public Selection<T> SetRequired(bool required)
		{
			this.required = required;
			return this;
		}


		/// <summary>
		/// If false, only choose(Object) will fire a change event. Default is true.
		/// </summary>
		/// <param name="programmaticChangeEvents">Programmatic change events.</param>
		public Selection<T> SetProgrammaticChangeEvents(bool programmaticChangeEvents)
		{
			this.programmaticChangeEvents = programmaticChangeEvents;
			return this;
		}


		public override string ToString()
		{
			return selected.ToString();
		}
	}
}