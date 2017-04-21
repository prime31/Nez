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
		public Element setElement( Element element )
		{
			this.element = element;
			return element;
		}


		/// <summary>
		/// Selects or deselects the specified item based on how the selection is configured, whether ctrl is currently pressed, etc.
		/// This is typically invoked by user interaction.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void choose( T item )
		{
			Assert.isNotNull( item, "item cannot be null" );
			if( _isDisabled )
				return;
			snapshot();

			try
			{
				if( ( toggle || ( !required && selected.Count == 1 ) || InputUtils.isControlDown() ) && selected.Contains( item ) )
				{
					if( required && selected.Count == 1 )
						return;
					selected.Remove( item );
					lastSelected = null;
				}
				else
				{
					bool modified = false;
					if( !multiple || ( !toggle && !InputUtils.isControlDown() ) )
					{
						if( selected.Count == 1 && selected.Contains( item ) )
							return;
						modified = selected.Count > 0;
						selected.Clear();
					}

					if( !selected.addIfNotPresent( item ) && !modified )
						return;
					lastSelected = item;
				}

				if( fireChangeEvent() )
					revert();
				else
					changed();
			}
			finally
			{
				cleanup();
			}
		}


		public bool hasItems()
		{
			return selected.Count > 0;
		}


		public bool isEmpty()
		{
			return selected.Count == 0;
		}


		public int size()
		{
			return selected.Count;
		}


		public List<T> items()
		{
			return selected;
		}


		/// <summary>
		/// Returns the first selected item, or null
		/// </summary>
		public T first()
		{
			return selected.FirstOrDefault();
		}


		protected void snapshot()
		{
			old.Clear();
			old.AddRange( selected );
		}


		protected void revert()
		{
			selected.Clear();
			selected.AddRange( old );
		}


		protected void cleanup()
		{
			old.Clear();
		}


		/// <summary>
		/// Sets the selection to only the specified item
		/// </summary>
		/// <param name="item">Item.</param>
		public Selection<T> set( T item )
		{
			Assert.isNotNull( item, "item cannot be null." );

			if( selected.Count == 1 && selected.First() == item )
				return this;

			snapshot();
			selected.Clear();
			selected.Add( item );

			if( programmaticChangeEvents && fireChangeEvent() )
			{
				revert();
			}
			else
			{
				lastSelected = item;
				changed();
			}
			cleanup();
			return this;
		}


		public Selection<T> setAll( List<T> items )
		{
			var added = false;
			snapshot();
			lastSelected = null;
			selected.Clear();
			for( var i = 0; i < items.Count; i++ )
			{
				var item = items[i];
				Assert.isNotNull( item, "item cannot be null" );
				added = selected.addIfNotPresent( item );
			}

			if( added )
			{
				if( programmaticChangeEvents && fireChangeEvent() )
				{
					revert();
				}
				else if( items.Count > 0 )
				{
					lastSelected = items.Last();
					changed();
				}
			}
			cleanup();
			return this;
		}


		/// <summary>
		/// Adds the item to the selection
		/// </summary>
		/// <param name="item">Item.</param>
		public void add( T item )
		{
			Assert.isNotNull( item, "item cannot be null" );
			if( !selected.addIfNotPresent( item ) )
				return;

			if( programmaticChangeEvents && fireChangeEvent() )
			{
				selected.Remove( item );
			}
			else
			{
				lastSelected = item;
				changed();
			}
		}


		public void addAll( List<T> items )
		{
			var added = false;
			snapshot();
			for( var i = 0; i < items.Count; i++ )
			{
				var item = items[i];
				Assert.isNotNull( item, "item cannot be null" );
				added = selected.addIfNotPresent( item );
			}
			if( added )
			{
				if( programmaticChangeEvents && fireChangeEvent() )
				{
					revert();
				}
				else
				{
					lastSelected = items.LastOrDefault();
					changed();
				}
			}
			cleanup();
		}


		public void remove( T item )
		{
			Assert.isNotNull( item, "item cannot be null" );
			if( !selected.Remove( item ) )
				return;

			if( programmaticChangeEvents && fireChangeEvent() )
			{
				selected.Add( item );
			}
			else
			{
				lastSelected = null;
				changed();
			}
		}


		public void removeAll( List<T> items )
		{
			var removed = false;
			snapshot();
			for( var i = 0; i < items.Count; i++ )
			{
				var item = items[i];
				Assert.isNotNull( item, "item cannot be null" );
				removed = selected.Remove( item );
			}

			if( removed )
			{
				if( programmaticChangeEvents && fireChangeEvent() )
				{
					revert();
				}
				else
				{
					lastSelected = null;
					changed();
				}
			}
			cleanup();
		}


		public void clear()
		{
			if( selected.Count == 0 )
				return;

			snapshot();
			selected.Clear();
			if( programmaticChangeEvents && fireChangeEvent() )
			{
				revert();
			}
			else
			{
				lastSelected = null;
				changed();
			}
			cleanup();
		}


		/// <summary>
		/// Called after the selection changes. The default implementation does nothing.
		/// </summary>
		protected virtual void changed()
		{}


		/// <summary>
		/// Fires a change event on the selection's Element, if any. Called internally when the selection changes, depending on
		/// setProgrammaticChangeEvents(bool)
		/// </summary>
		public bool fireChangeEvent()
		{
			if( element == null )
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


		public bool contains( T item )
		{
			if( item == null )
				return false;
			return selected.Contains( item );
		}


		/// <summary>
		/// Makes a best effort to return the last item selected, else returns an arbitrary item or null if the selection is empty.
		/// </summary>
		/// <returns>The last selected.</returns>
		public T getLastSelected()
		{
			if( lastSelected != null )
				return lastSelected;

			return selected.FirstOrDefault();
		}


		/// <summary>
		/// If true, prevents choose(Object) from changing the selection. Default is false.
		/// </summary>
		/// <param name="isDisabled">Is disabled.</param>
		public Selection<T> setDisabled( bool isDisabled )
		{
			_isDisabled = isDisabled;
			return this;
		}


		public bool isDisabled()
		{
			return _isDisabled;
		}


		public bool getToggle()
		{
			return toggle;
		}


		/// <summary>
		/// If true, prevents choose(Object) from clearing the selection. Default is false.
		/// </summary>
		/// <param name="toggle">Toggle.</param>
		public Selection<T> setToggle( bool toggle )
		{
			this.toggle = toggle;
			return this;
		}


		public bool getMultiple()
		{
			return multiple;
		}


		/// <summary>
		/// If true, allows choose(Object) to select multiple items. Default is false.
		/// </summary>
		/// <param name="multiple">Multiple.</param>
		public Selection<T> setMultiple( bool multiple )
		{
			this.multiple = multiple;
			return this;
		}


		public bool getRequired()
		{
			return required;
		}


		/// <summary>
		/// If true, prevents choose(Object) from selecting none. Default is false.
		/// </summary>
		/// <param name="required">Required.</param>
		public Selection<T> setRequired( bool required )
		{
			this.required = required;
			return this;
		}


		/// <summary>
		/// If false, only choose(Object) will fire a change event. Default is true.
		/// </summary>
		/// <param name="programmaticChangeEvents">Programmatic change events.</param>
		public Selection<T> setProgrammaticChangeEvents( bool programmaticChangeEvents )
		{
			this.programmaticChangeEvents = programmaticChangeEvents;
			return this;
		}


		public string toString()
		{
			return selected.ToString();
		}

	}
}

