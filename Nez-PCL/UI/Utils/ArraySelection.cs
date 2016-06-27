using System;
using System.Collections.Generic;
using System.Linq;


namespace Nez.UI
{
	public class ArraySelection<T> : Selection<T> where T : class
	{
		List<T> array;
		bool rangeSelect = true;
		int rangeStart;


		public ArraySelection( List<T> array )
		{
			this.array = array;
		}


		public override void choose( T item )
		{
			Assert.isNotNull( item, "item cannot be null" );
			if( _isDisabled )
				return;
			
			var index = array.IndexOf( item );
			if( selected.Count > 0 && rangeSelect && multiple && InputUtils.isShiftDown() )
			{
				int oldRangeState = rangeStart;
				snapshot();
				// Select new range.
				int start = rangeStart, end = index;
				if( start > end )
				{
					var temp = end;
					end = start;
					start = temp;
				}

				if( !InputUtils.isControlDown() )
					selected.Clear();
				for( int i = start; i <= end; i++ )
					selected.Add( array[i] );

				if( fireChangeEvent() )
				{
					rangeStart = oldRangeState;
					revert();
				}
				cleanup();
				return;
			}
			else
			{
				rangeStart = index;
			}

			base.choose( item );
		}


		public bool getRangeSelect()
		{
			return rangeSelect;
		}


		public void setRangeSelect( bool rangeSelect )
		{
			this.rangeSelect = rangeSelect;
		}


		/// <summary>
		/// Removes objects from the selection that are no longer in the items array. If getRequired() is true and there is
		/// no selected item, the first item is selected.
		/// </summary>
		public void validate()
		{
			if( array.Count == 0 )
			{
				clear();
				return;
			}

			for( var i = selected.Count - 1; i >= 0; i-- )
			{
				var item = selected[i];
				if( !array.Contains( item ) )
					selected.Remove( item );
			}

			if( required && selected.Count == 0 )
				set( array.First() );
		}
	}
}

