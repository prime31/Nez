using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;

namespace Nez.UI
{
    public interface IListViewFilter
    {
        bool isValid(string searchFilter, object userData);
    }

    public class ListView<T> : Element, IInputListener where T : Element
    {
        public event Action<T> onChanged;

        ListViewStyle _style;
        List<T> _items = new List<T>();
        List<T> _filteredItems = new List<T>();
        ArraySelection<T> _selection;
        float _prefWidth, _prefHeight;
        float _itemHeight;
        float _offsetX, _offsetY;
        Rectangle _cullingArea;
        int _hoveredItemIndex = -1;
        bool _isMouseOverList;

        public IListViewFilter filter;

        public ListView(Skin skin) : this(skin.get<ListViewStyle>())
        {
        }

        public ListView(Skin skin, string styleName) : this(skin.get<ListViewStyle>(styleName))
        {
        }

        public ListView(ListViewStyle style)
        {
            _selection = new ArraySelection<T>(_items);
            _selection.setElement(this);
            _selection.setRequired(true);

            setStyle(style);
            setSize(preferredWidth, preferredHeight);
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


        bool IInputListener.onMousePressed(Vector2 mousePos)
        {
            if (_selection.isDisabled() || _filteredItems.Count == 0)
                return false;

            var lastSelectedItem = _selection.getLastSelected();
            var index = getItemIndexUnderMousePosition(mousePos);
            index = System.Math.Max(0, index);
            index = System.Math.Min(_filteredItems.Count - 1, index);
            _selection.choose(_filteredItems[index]);

            if (lastSelectedItem != _filteredItems[index] && onChanged != null)
                onChanged(_filteredItems[index]);

            return true;
        }

        void IInputListener.onMouseMoved(Vector2 mousePos)
        {
        }


        void IInputListener.onMouseUp(Vector2 mousePos)
        {
        }


        bool IInputListener.onMouseScrolled(int mouseWheelDelta)
        {
            return false;
        }


        int getItemIndexUnderMousePosition(Vector2 mousePos)
        {
            if (_selection.isDisabled() || _filteredItems.Count == 0)
                return -1;

            var top = 0f;
            if (_style.background != null)
            {
                top += _style.background.topHeight + _style.background.bottomHeight;
                mousePos.Y += _style.background.bottomHeight;
            }

            var index = (int) ((top + mousePos.Y) / _itemHeight);
            if (index < 0 || index > _filteredItems.Count - 1)
                return -1;

            return index;
        }

        #endregion


        public override void layout()
        {
            IDrawable selectedDrawable = _style.selection;

            _prefWidth = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                var e = _items[i] as Element;
                if (e == null) continue;
                _prefWidth = System.Math.Max(e.preferredWidth, _prefWidth);
                _itemHeight = System.Math.Max(e.preferredHeight, _itemHeight);
            }

            _prefWidth += selectedDrawable.leftWidth + selectedDrawable.rightWidth;
            _prefHeight = _items.Count * _itemHeight;

            var background = _style.background;
            if (background != null)
            {
                _prefWidth += background.leftWidth + background.rightWidth;
                _prefHeight += background.topHeight + background.bottomHeight;
            }
        }


        public void filterItems(string filterText)
        {
            _filteredItems.Clear();

            if (filterText.Length == 0)
                _filteredItems.AddRange(_items);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i] as Element;

                if (filter.isValid(filterText, item.userData))
                    _filteredItems.Add(item as T);
            }
        }

        public override void draw(Graphics graphics, float parentAlpha)
        {
            // update our hoved item if the mouse is over the list
            if (_isMouseOverList)
            {
                var mousePos = screenToLocalCoordinates(stage.getMousePosition());
                _hoveredItemIndex = getItemIndexUnderMousePosition(mousePos);
            }

            validate();

            var selectedDrawable = _style.selection;

            var color = getColor();
            color = new Color(color, (int) (color.A * parentAlpha));

            float x = getX(), y = getY(), width = getWidth(), height = getHeight();
            var itemY = 0f;

            var background = _style.background;
            if (background != null)
            {
                background.draw(graphics, x, y, width, height, color);
                var leftWidth = background.leftWidth;
                x += leftWidth;
                itemY += background.topHeight;
                width -= leftWidth + background.rightWidth;
            }

            _cullingArea = new RectangleF(0, 0, parent.getWidth(), parent.getHeight());
            for (var i = 0; i < _filteredItems.Count; i++)
            {
                var item = _filteredItems[i];
                var e = item as Element;

                var pos = new Vector2(x + _offsetX, y + itemY + _offsetY + _itemHeight / 2);

                var selected = _selection.contains(item);
                if (selected)
                {
                    selectedDrawable.draw(graphics, x, y + itemY, width, _itemHeight, color);
                }
                else if (i == _hoveredItemIndex && _style.hoverSelection != null)
                {
                    _style.hoverSelection.draw(graphics, x, y + itemY, width, _itemHeight, color);
                }

                if (_cullingArea.Contains(pos))
                {
                    e.setPosition(pos.X, pos.Y);
                    e.draw(graphics, parentAlpha);
                }

                itemY += _itemHeight;
            }
        }


        #region config

        public ListView<T> setStyle(ListViewStyle style)
        {
            Assert.isNotNull(style, "style cannot be null");
            _style = style;
            invalidateHierarchy();
            return this;
        }

        public ListViewStyle getStyle()
        {
            return _style;
        }


        public ArraySelection<T> getSelection()
        {
            return _selection;
        }

        public T getSelected()
        {
            return _selection.first();
        }

        public ListView<T> setSelected(T item)
        {
            if (_items.Contains(item))
                _selection.set(item);
            else if (_selection.getRequired() && _items.Count > 0)
                _selection.set(_items[0]);
            else
                _selection.clear();

            return this;
        }

        public int getSelectedIndex()
        {
            var selected = _selection.items();
            return selected.Count == 0 ? -1 : _items.IndexOf(selected[0]);
        }

        public ListView<T> setSelectedIndex(int index)
        {
            Assert.isFalse(index < -1 || index >= _items.Count,
                "index must be >= -1 and < " + _items.Count + ": " + index);

            if (index == -1)
                _selection.clear();
            else
                _selection.set(_items[index]);

            return this;
        }


        public ListView<T> setItems(params T[] newItems)
        {
            setItems(new List<T>(newItems));
            return this;
        }

        public ListView<T> setItems(IList<T> newItems)
        {
            Assert.isNotNull(newItems, "newItems cannot be null");
            float oldPrefWidth = _prefWidth, oldPrefHeight = _prefHeight;

            _items.Clear();
            _items.AddRange(newItems);
            _selection.validate();

            invalidate();
            validate();
            if (oldPrefWidth != _prefWidth || oldPrefHeight != _prefHeight)
            {
                invalidateHierarchy();
                setSize(_prefWidth, _prefHeight);
            }
            _filteredItems.AddRange(_items);
            return this;
        }


        public void clearItems()
        {
            if (_items.Count == 0)
                return;

            _items.Clear();
            _filteredItems.Clear();
            _selection.clear();
            invalidateHierarchy();
        }

        public List<T> getItems()
        {
            return _items;
        }


        public float getItemHeight()
        {
            return _itemHeight;
        }


        public ListView<T> setCullingArea(Rectangle cullingArea)
        {
            _cullingArea = cullingArea;
            return this;
        }

        #endregion
    }

    public class ListViewStyle
    {
        public IDrawable selection;

        /** Optional */
        public IDrawable hoverSelection;

        /** Optional */
        public IDrawable background;

        public ListViewStyle()
        {
        }

        public ListViewStyle(BitmapFont font, Color fontColorSelected, Color fontColorUnselected, IDrawable selection)
        {
            this.selection = selection;
        }

        public ListViewStyle clone()
        {
            return new ListViewStyle
            {
                selection = selection,
                hoverSelection = hoverSelection,
                background = background
            };
        }
    }
}