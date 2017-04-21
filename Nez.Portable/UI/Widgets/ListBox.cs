
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


namespace Nez.UI
{
	/// <summary>
	/// displays textual items and highlights the currently selected item
	/// </summary>
	public class ListBox<T> : Element, IInputListener where T : class
	{
		public event Action<T> onChanged;

		ListBoxStyle _style;
		List<T> _items = new List<T>();
		ArraySelection<T> _selection;
		float _prefWidth, _prefHeight;
		float _itemHeight;
		float _textOffsetX, _textOffsetY;
		Rectangle? _cullingArea;
		int _hoveredItemIndex = -1;
		bool _isMouseOverList;


		public ListBox( Skin skin, string styleName = null ) : this( skin.get<ListBoxStyle>( styleName ) )
		{ }


		public ListBox( ListBoxStyle style )
		{
			_selection = new ArraySelection<T>( _items );
			_selection.setElement( this );
			_selection.setRequired( true );

			setStyle( style );
			setSize( preferredWidth, preferredHeight );
		}


		#region ILayout

		public override float preferredWidth
		{
			get
			{
				validate();
				return _prefWidth;
			}
		}

		public override float preferredHeight
		{
			get
			{
				validate();
				return _prefHeight;
			}
		}

		#endregion


		#region IInputListener

		void IInputListener.onMouseEnter()
		{
			_isMouseOverList = true;
		}


		void IInputListener.onMouseExit()
		{
			_isMouseOverList = false;
			_hoveredItemIndex = -1;
		}


		bool IInputListener.onMousePressed( Vector2 mousePos )
		{
			if( _selection.isDisabled() || _items.Count == 0 )
				return false;

			var lastSelectedItem = _selection.getLastSelected();
			var index = getItemIndexUnderMousePosition( mousePos );
			index = Math.Max( 0, index );
			index = Math.Min( _items.Count - 1, index );
			_selection.choose( _items[index] );

			if( lastSelectedItem != _items[index] && onChanged != null )
				onChanged( _items[index] );

			return true;
		}


		void IInputListener.onMouseMoved( Vector2 mousePos )
		{}


		void IInputListener.onMouseUp( Vector2 mousePos )
		{}


		bool IInputListener.onMouseScrolled( int mouseWheelDelta )
		{
			return false;
		}


		int getItemIndexUnderMousePosition( Vector2 mousePos )
		{
			if( _selection.isDisabled() || _items.Count == 0 )
				return -1;

			var top = 0f;
			if( _style.background != null )
			{
				top += _style.background.topHeight + _style.background.bottomHeight;
				mousePos.Y += _style.background.bottomHeight;
			}

			var index = (int)( ( top + mousePos.Y ) / _itemHeight );
			if( index < 0 || index > _items.Count - 1 )
				return -1;

			return index;
		}

		#endregion


		public override void layout()
		{
			var font = _style.font;
			IDrawable selectedDrawable = _style.selection;

			_itemHeight = /*font.getCapHeight()*/ font.lineHeight - font.descent * 2;
			_itemHeight += selectedDrawable.topHeight + selectedDrawable.bottomHeight;

			_textOffsetX = selectedDrawable.leftWidth;
			_textOffsetY = selectedDrawable.topHeight - font.descent;

			_prefWidth = 0;
			for( var i = 0; i < _items.Count; i++ )
				_prefWidth = Math.Max( font.measureString( _items[i].ToString() ).X, _prefWidth );

			_prefWidth += selectedDrawable.leftWidth + selectedDrawable.rightWidth;
			_prefHeight = _items.Count * _itemHeight;

			var background = _style.background;
			if( background != null )
			{
				_prefWidth += background.leftWidth + background.rightWidth;
				_prefHeight += background.topHeight + background.bottomHeight;
			}
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			// update our hoved item if the mouse is over the list
			if( _isMouseOverList )
			{
				var mousePos = screenToLocalCoordinates( stage.getMousePosition() );
				_hoveredItemIndex = getItemIndexUnderMousePosition( mousePos );
			}

			validate();

			var font = _style.font;
			var selectedDrawable = _style.selection;

			var color = getColor();
			color = new Color( color, color.A * parentAlpha );

			float x = getX(), y = getY(), width = getWidth(), height = getHeight();
			var itemY = 0f;

			var background = _style.background;
			if( background != null )
			{
				background.draw( graphics, x, y, width, height, color );
				var leftWidth = background.leftWidth;
				x += leftWidth;
				itemY += background.topHeight;
				width -= leftWidth + background.rightWidth;
			}

			var unselectedFontColor = new Color( _style.fontColorUnselected, _style.fontColorUnselected.A * parentAlpha );
			var selectedFontColor = new Color( _style.fontColorSelected, _style.fontColorSelected.A * parentAlpha );
			var hoveredFontColor = new Color( _style.fontColorHovered, _style.fontColorHovered.A * parentAlpha );
			Color fontColor;
			for( var i = 0; i < _items.Count; i++ )
			{
				if( !_cullingArea.HasValue || ( itemY - _itemHeight <= _cullingArea.Value.Y + _cullingArea.Value.Height && itemY >= _cullingArea.Value.Y ) )
				{
					var item = _items[i];
					var selected = _selection.contains( item );
					if( selected )
					{
						selectedDrawable.draw( graphics, x, y + itemY, width, _itemHeight, color );
						fontColor = selectedFontColor;
					}
					else if( i == _hoveredItemIndex && _style.hoverSelection != null )
					{
						_style.hoverSelection.draw( graphics, x, y + itemY, width, _itemHeight, color );
						fontColor = hoveredFontColor;
					}
					else
					{
						fontColor = unselectedFontColor;
					}

					var textPos = new Vector2( x + _textOffsetX, y + itemY + _textOffsetY );
					graphics.batcher.drawString( font, item.ToString(), textPos, fontColor );
				}
				else if( itemY < _cullingArea.Value.Y )
				{
					break;
				}

				itemY += _itemHeight;
			}
		}


		#region config

		public ListBox<T> setStyle( ListBoxStyle style )
		{
			Assert.isNotNull( style, "style cannot be null" );
			_style = style;
			invalidateHierarchy();
			return this;
		}


		/// <summary>
		/// Returns the list's style. Modifying the returned style may not have an effect until setStyle(ListStyle) is called
		/// </summary>
		/// <returns>The style.</returns>
		public ListBoxStyle getStyle()
		{
			return _style;
		}


		public ArraySelection<T> getSelection()
		{
			return _selection;
		}


		/// <summary>
		/// Returns the first selected item, or null
		/// </summary>
		/// <returns>The selected.</returns>
		public T getSelected()
		{
			return _selection.first();
		}


		/// <summary>
		/// Sets the selection to only the passed item, if it is a possible choice.
		/// </summary>
		/// <param name="item">Item.</param>
		public ListBox<T> setSelected( T item )
		{
			if( _items.Contains( item ) )
				_selection.set( item );
			else if( _selection.getRequired() && _items.Count > 0 )
				_selection.set( _items[0] );
			else
				_selection.clear();

			return this;
		}


		/// <summary>
		/// gets the index of the first selected item. The top item has an index of 0. Nothing selected has an index of -1.
		/// </summary>
		/// <returns>The selected index.</returns>
		public int getSelectedIndex()
		{
			var selected = _selection.items();
			return selected.Count == 0 ? -1 : _items.IndexOf( selected[0] );
		}


		/// <summary>
		/// Sets the selection to only the selected index
		/// </summary>
		/// <param name="index">Index.</param>
		public ListBox<T> setSelectedIndex( int index )
		{
			Assert.isFalse( index < -1 || index >= _items.Count, "index must be >= -1 and < " + _items.Count + ": " + index );

			if( index == -1 )
				_selection.clear();
			else
				_selection.set( _items[index] );

			return this;
		}


		public ListBox<T> setItems( params T[] newItems )
		{
			setItems( new List<T>( newItems ) );
			return this;
		}


		/// <summary>
		/// Sets the items visible in the list, clearing the selection if it is no longer valid. If a selection is
		/// ArraySelection#getRequired(), the first item is selected.
		/// </summary>
		/// <param name="newItems">New items.</param>
		public ListBox<T> setItems( IList<T> newItems )
		{
			Assert.isNotNull( newItems, "newItems cannot be null" );
			float oldPrefWidth = _prefWidth, oldPrefHeight = _prefHeight;

			_items.Clear();
			_items.AddRange( newItems );
			_selection.validate();

			invalidate();
			validate();
			if( oldPrefWidth != _prefWidth || oldPrefHeight != _prefHeight )
			{
				invalidateHierarchy();
				setSize( _prefWidth, _prefHeight );
			}
			return this;
		}


		public void clearItems()
		{
			if( _items.Count == 0 )
				return;
			
			_items.Clear();
			_selection.clear();
			invalidateHierarchy();
		}


		/// <summary>
		/// Returns the internal items array. If modified, {@link #setItems(Array)} must be called to reflect the changes.
		/// </summary>
		/// <returns>The items.</returns>
		public List<T> getItems()
		{
			return _items;
		}


		public float getItemHeight()
		{
			return _itemHeight;
		}


		public ListBox<T> setCullingArea( Rectangle cullingArea )
		{
			_cullingArea = cullingArea;
			return this;
		}

		#endregion

	}


	public class ListBoxStyle
	{
		public BitmapFont font;
		public Color fontColorSelected = Color.Black;
		public Color fontColorUnselected = Color.White;
		public Color fontColorHovered = Color.Black;
		public IDrawable selection;
		/** Optional */
		public IDrawable hoverSelection;
		/** Optional */
		public IDrawable background;


		public ListBoxStyle()
		{
			font = Graphics.instance.bitmapFont;
		}


		public ListBoxStyle( BitmapFont font, Color fontColorSelected, Color fontColorUnselected, IDrawable selection )
		{
			this.font = font;
			this.fontColorSelected = fontColorSelected;
			this.fontColorUnselected = fontColorUnselected;
			this.selection = selection;
		}


		public ListBoxStyle clone()
		{
			return new ListBoxStyle
			{
				font = font,
				fontColorSelected = fontColorSelected,
				fontColorUnselected = fontColorUnselected,
				selection = selection,
				hoverSelection = hoverSelection,
				background = background
			};
		}

	}

}

