using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


namespace Nez.UI
{
	public class SelectBox<T> : Element, IInputListener where T : class
	{
		public Action<T> OnChanged;

		SelectBoxStyle style;
		List<T> _items = new List<T>();
		ArraySelection<T> _selection;
		SelectBoxList<T> _selectBoxList;
		float _prefWidth, _prefHeight;
		bool _isDisabled;
		bool _isMouseOver;


		public SelectBox(Skin skin) : this(skin.Get<SelectBoxStyle>())
		{
		}


		public SelectBox(Skin skin, string styleName = null) : this(skin.Get<SelectBoxStyle>(styleName))
		{
		}


		public SelectBox(SelectBoxStyle style)
		{
			_selection = new ArraySelection<T>(_items);
			SetStyle(style);
			SetSize(PreferredWidth, PreferredHeight);

			_selection.SetElement(this);
			_selection.SetRequired(true);
			_selectBoxList = new SelectBoxList<T>(this);
		}


		public override void Layout()
		{
			var bg = style.Background;
			var font = style.Font;

			if (bg != null)
				_prefHeight = Math.Max(bg.TopHeight + bg.BottomHeight + font.LineHeight - font.Padding.Bottom * 2f,
					bg.MinHeight);
			else
				_prefHeight = font.LineHeight - font.Padding.Bottom * 2;

			float maxItemWidth = 0;
			for (var i = 0; i < _items.Count; i++)
				maxItemWidth = Math.Max(font.MeasureString(_items[i].ToString()).X, maxItemWidth);


			_prefWidth = maxItemWidth;
			if (bg != null)
				_prefWidth += bg.LeftWidth + bg.RightWidth;

			var listStyle = style.ListStyle;
			var scrollStyle = style.ScrollStyle;
			float listWidth = maxItemWidth + listStyle.Selection.LeftWidth + listStyle.Selection.RightWidth;
			if (scrollStyle.Background != null)
				listWidth += scrollStyle.Background.LeftWidth + scrollStyle.Background.RightWidth;
			if (_selectBoxList == null || !_selectBoxList.IsScrollingDisabledY())
				listWidth += Math.Max(style.ScrollStyle.VScroll != null ? style.ScrollStyle.VScroll.MinWidth : 0,
					style.ScrollStyle.VScrollKnob != null ? style.ScrollStyle.VScrollKnob.MinWidth : 0);
			_prefWidth = Math.Max(_prefWidth, listWidth);
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			Validate();

			IDrawable background;
			if (_isDisabled && style.BackgroundDisabled != null)
				background = style.BackgroundDisabled;
			else if (_selectBoxList.HasParent() && style.BackgroundOpen != null)
				background = style.BackgroundOpen;
			else if (_isMouseOver && style.BackgroundOver != null)
				background = style.BackgroundOver;
			else if (style.Background != null)
				background = style.Background;
			else
				background = null;

			var font = style.Font;
			var fontColor = _isDisabled ? style.DisabledFontColor : style.FontColor;

			var color = GetColor();
			color = ColorExt.Create(color, (int)(color.A * parentAlpha));
			float x = GetX();
			float y = GetY();
			float width = GetWidth();
			float height = GetHeight();

			if (background != null)
				background.Draw(batcher, x, y, width, height, color);

			var selected = _selection.First();
			if (selected != null)
			{
				var str = selected.ToString();
				if (background != null)
				{
					width -= background.LeftWidth + background.RightWidth;
					height -= background.BottomHeight + background.TopHeight;
					x += background.LeftWidth;
					y += (int)(height / 2 + background.BottomHeight - font.LineHeight / 2);
				}
				else
				{
					y += (int)(height / 2 + font.LineHeight / 2);
				}

				fontColor = ColorExt.Create(fontColor, (int)(fontColor.A * parentAlpha));
				batcher.DrawString(font, str, new Vector2(x, y), fontColor);
			}
		}


		#region ILayout

		public override float PreferredWidth
		{
			get
			{
				Validate();
				return _prefWidth;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				Validate();
				return _prefHeight;
			}
		}

		#endregion


		#region IInputListener

		void IInputListener.OnMouseEnter()
		{
			_isMouseOver = true;
		}


		void IInputListener.OnMouseExit()
		{
			_isMouseOver = false;
		}


		bool IInputListener.OnMousePressed(Vector2 mousePos)
		{
			if (_isDisabled)
				return false;

			if (_selectBoxList.HasParent())
				HideList();
			else
				ShowList();

			return true;
		}


		void IInputListener.OnMouseMoved(Vector2 mousePos)
		{
		}


		void IInputListener.OnMouseUp(Vector2 mousePos)
		{
		}


		bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
		{
			return false;
		}

		#endregion


		#region config

		/// <summary>
		/// Set the max number of items to display when the select box is opened. Set to 0 (the default) to display as many as fit in
		/// the stage height.
		/// </summary>
		/// <returns>The max list count.</returns>
		/// <param name="maxListCount">Max list count.</param>
		public void SetMaxListCount(int maxListCount)
		{
			_selectBoxList.MaxListCount = maxListCount;
		}


		/// <summary>
		/// returns Max number of items to display when the box is opened, or <= 0 to display them all.
		/// </summary>
		/// <returns>The max list count.</returns>
		public int GetMaxListCount()
		{
			return _selectBoxList.MaxListCount;
		}


		internal override void SetStage(Stage stage)
		{
			if (stage == null)
				_selectBoxList.Hide();
			base.SetStage(stage);
		}


		public void SetStyle(SelectBoxStyle style)
		{
			Insist.IsNotNull(style, "style cannot be null");
			this.style = style;
			InvalidateHierarchy();
		}


		/// <summary>
		/// Returns the select box's style. Modifying the returned style may not have an effect until setStyle(SelectBoxStyle)
		/// is called.
		/// </summary>
		/// <returns>The style.</returns>
		public SelectBoxStyle GetStyle()
		{
			return style;
		}


		/// <summary>
		/// Set the backing Array that makes up the choices available in the SelectBox
		/// </summary>
		/// <returns>The items.</returns>
		/// <param name="newItems">New items.</param>
		public void SetItems(params T[] newItems)
		{
			SetItems(new List<T>(newItems));
		}


		/// <summary>
		/// Sets the items visible in the select box
		/// </summary>
		/// <returns>The items.</returns>
		/// <param name="newItems">New items.</param>
		public void SetItems(List<T> newItems)
		{
			Insist.IsNotNull(newItems, "newItems cannot be null");
			float oldPrefWidth = PreferredWidth;

			_items.Clear();
			_items.AddRange(newItems);
			_selection.Validate();
			_selectBoxList.ListBox.SetItems(_items);

			Invalidate();
			Validate();
			if (oldPrefWidth != _prefWidth)
			{
				InvalidateHierarchy();
				SetSize(_prefWidth, _prefHeight);
			}
		}


		public void ClearItems()
		{
			if (_items.Count == 0)
				return;

			_items.Clear();
			_selection.Clear();
			InvalidateHierarchy();
		}


		/// <summary>
		/// Returns the internal items array. If modified, setItems(Array) must be called to reflect the changes.
		/// </summary>
		/// <returns>The items.</returns>
		public List<T> GetItems()
		{
			return _items;
		}


		/// <summary>
		/// Get the set of selected items, useful when multiple items are selected returns a Selection object containing the
		/// selected elements
		/// </summary>
		/// <returns>The selection.</returns>
		public ArraySelection<T> GetSelection()
		{
			return _selection;
		}


		/// <summary>
		/// Returns the first selected item, or null. For multiple selections use SelectBox#getSelection()
		/// </summary>
		/// <returns>The selected.</returns>
		public T GetSelected()
		{
			return _selection.First();
		}


		/// <summary>
		/// Sets the selection to only the passed item, if it is a possible choice, else selects the first item.
		/// </summary>
		/// <returns>The selected.</returns>
		/// <param name="item">Item.</param>
		public void SetSelected(T item)
		{
			if (_items.Contains(item))
				_selection.Set(item);
			else if (_items.Count > 0)
				_selection.Set(_items[0]);
			else
				_selection.Clear();
		}


		/// <summary>
		/// returns The index of the first selected item. The top item has an index of 0. Nothing selected has an index of -1.
		/// </summary>
		/// <returns>The selected index.</returns>
		public int GetSelectedIndex()
		{
			var selected = _selection.Items();
			return selected.Count == 0 ? -1 : _items.IndexOf(selected.First());
		}


		/// <summary>
		/// Sets the selection to only the selected index
		/// </summary>
		/// <returns>The selected index.</returns>
		/// <param name="index">Index.</param>
		public void SetSelectedIndex(int index)
		{
			_selection.Set(_items[index]);
		}


		public void SetDisabled(bool disabled)
		{
			if (disabled && !_isDisabled)
				HideList();
			_isDisabled = disabled;
		}


		public bool IsDisabled()
		{
			return _isDisabled;
		}


		public void ShowList()
		{
			if (_items.Count == 0)
				return;

			_selectBoxList.Show(GetStage());
		}


		public void HideList()
		{
			_selectBoxList.Hide();
		}


		/// <summary>
		/// Returns the ListBox shown when the select box is open
		/// </summary>
		/// <returns>The list.</returns>
		public ListBox<T> GetListBox()
		{
			return _selectBoxList.ListBox;
		}


		/// <summary>
		/// Disables scrolling of the list shown when the select box is open.
		/// </summary>
		/// <returns>The scrolling disabled.</returns>
		/// <param name="y">The y coordinate.</param>
		public void SetScrollingDisabled(bool y)
		{
			_selectBoxList.SetScrollingDisabled(true, y);
			InvalidateHierarchy();
		}


		/// <summary>
		/// Returns the scroll pane containing the list that is shown when the select box is open.
		/// </summary>
		/// <returns>The scroll pane.</returns>
		public ScrollPane GetScrollPane()
		{
			return _selectBoxList;
		}


		public void OnShow(Element selectBoxList, bool below)
		{
			//_selectBoxList.getColor().A = 0;
			//_selectBoxList.addAction( fadeIn( 0.3f, Interpolation.fade ) );
		}


		public void OnHide(Element selectBoxList)
		{
			//selectBoxList.getColor().A = 255;
			//selectBoxList.addAction( sequence( fadeOut( 0.15f, Interpolation.fade ), removeActor() ) );
			selectBoxList.Remove();
		}

		#endregion
	}


	public class SelectBoxStyle
	{
		public BitmapFont Font;

		public Color FontColor = Color.White;

		/** Optional */
		public Color DisabledFontColor;

		/** Optional */
		public IDrawable Background;
		public ScrollPaneStyle ScrollStyle;

		public ListBoxStyle ListStyle;

		/** Optional */
		public IDrawable BackgroundOver, BackgroundOpen, BackgroundDisabled;


		public SelectBoxStyle()
		{
			Font = Graphics.Instance.BitmapFont;
		}

		public SelectBoxStyle(BitmapFont font, Color fontColor, IDrawable background, ScrollPaneStyle scrollStyle,
							  ListBoxStyle listStyle)
		{
			Font = font;
			FontColor = fontColor;
			Background = background;
			ScrollStyle = scrollStyle;
			ListStyle = listStyle;
		}
	}
}