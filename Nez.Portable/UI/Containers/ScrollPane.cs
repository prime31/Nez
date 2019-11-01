using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A group that scrolls a child widget using scrollbars and/or mouse or touch dragging.
	///
	/// The widget is sized to its preferred size.If the widget's preferred width or height is less than the size of this scroll pane,
	/// it is set to the size of this scroll pane. Scrollbars appear when the widget is larger than the scroll pane.
	///
	/// The scroll pane's preferred size is that of the child widget. At this size, the child widget will not need to scroll, so
	/// scroll pane is typically sized by ignoring the preferred size in one or both directions.
	/// </summary>
	public class ScrollPane : Group, IInputListener
	{
		ScrollPaneStyle _style;
		Element _widget;

		Rectangle _hScrollBounds;
		Rectangle _vScrollBounds;
		Rectangle _hKnobBounds;
		Rectangle _vKnobBounds;
		Rectangle _widgetAreaBounds;

		float _scrollSpeed = 0.005f;
		bool _useNaturalScrolling = true;
		bool _scrollX, _scrollY = true;
		bool _vScrollOnRight = true;
		bool _hScrollOnBottom = true;
		float _amountX, _amountY;
		float _visualAmountX, _visualAmountY;
		float _maxX, _maxY;
		bool _touchScrollH, _touchScrollV;
		float _areaWidth, _areaHeight;
		bool _fadeScrollBars = true, _smoothScrolling = true;
		float _fadeAlpha, _fadeAlphaSeconds = 0.5f, _fadeDelay, _fadeDelaySeconds = 1f;

		float _velocityX, _velocityY;
		float _flingTimer;
		bool _overscrollX = true, _overscrollY = true;
		float _flingTime = 1f;
		float _overscrollDistance = 50, _overscrollSpeedMin = 30, _overscrollSpeedMax = 200;
		bool _forceScrollX, _forceScrollY;
		protected bool _disableX = true, _disableY;
		bool _clamp = true;
		bool _scrollbarsOnTop;
		bool _variableSizeKnobs = true;

		// input data
		Vector2 _lastMousePos;
		float _lastHandlePosition;


		public ScrollPane(Element widget) : this(widget, new ScrollPaneStyle())
		{
		}


		public ScrollPane(Element widget, Skin skin) : this(widget, skin.Get<ScrollPaneStyle>())
		{
		}


		public ScrollPane(Element widget, Skin skin, string styleName) : this(widget,
			skin.Get<ScrollPaneStyle>(styleName))
		{
		}


		public ScrollPane(Element widget, ScrollPaneStyle style)
		{
			Insist.IsNotNull(style, "style cannot be null");
			transform = true;
			_style = style;
			SetWidget(widget);
			SetSize(150, 150);
		}


		void ResetFade()
		{
			_fadeAlpha = _fadeAlphaSeconds;
			_fadeDelay = _fadeDelaySeconds;
		}


		/// <summary>
		/// If currently scrolling by tracking a touch down, stop scrolling.
		/// </summary>
		public void Cancel()
		{
			_touchScrollH = false;
			_touchScrollV = false;
		}


		void Clamp()
		{
			if (!_clamp)
				return;

			SetScrollX(_overscrollX
				? Mathf.Clamp(_amountX, -_overscrollDistance, _maxX + _overscrollDistance)
				: Mathf.Clamp(_amountX, 0, _maxX));
			SetScrollY(_overscrollY
				? Mathf.Clamp(_amountY, -_overscrollDistance, _maxY + _overscrollDistance)
				: Mathf.Clamp(_amountY, 0, _maxY));
		}


		#region ILayout

		public override float MinWidth { get; } = 0;

		public override float MinHeight { get; } = 0;

		public override float PreferredWidth
		{
			get
			{
				if (_widget is ILayout)
				{
					var width = ((ILayout)_widget).PreferredWidth;
					if (_style.Background != null) width += _style.Background.LeftWidth + _style.Background.RightWidth;
					if (_forceScrollY)
					{
						var scrollbarWidth = 0f;
						if (_style.VScrollKnob != null) scrollbarWidth = _style.VScrollKnob.MinWidth;
						if (_style.VScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, _style.VScroll.MinWidth);
						width += scrollbarWidth;
					}

					return width;
				}

				return 150;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				if (_widget is ILayout)
				{
					var height = ((ILayout)_widget).PreferredHeight;
					if (_style.Background != null)
						height += _style.Background.TopHeight + _style.Background.BottomHeight;
					if (_forceScrollX)
					{
						var scrollbarHeight = 0f;
						if (_style.HScrollKnob != null) scrollbarHeight = _style.HScrollKnob.MinHeight;
						if (_style.HScroll != null)
							scrollbarHeight = Math.Max(scrollbarHeight, _style.HScroll.MinHeight);
						height += scrollbarHeight;
					}

					return height;
				}

				return 150;
			}
		}

		public override void Layout()
		{
			var bg = _style.Background;
			var hScrollKnob = _style.HScrollKnob;
			var vScrollKnob = _style.VScrollKnob;

			float bgLeftWidth = 0, bgRightWidth = 0, bgTopHeight = 0, bgBottomHeight = 0;
			if (bg != null)
			{
				bgLeftWidth = bg.LeftWidth;
				bgRightWidth = bg.RightWidth;
				bgTopHeight = bg.TopHeight;
				bgBottomHeight = bg.BottomHeight;
			}

			var width = GetWidth();
			var height = GetHeight();

			var scrollbarHeight = 0f;
			if (hScrollKnob != null) scrollbarHeight = hScrollKnob.MinHeight;
			if (_style.HScroll != null) scrollbarHeight = Math.Max(scrollbarHeight, _style.HScroll.MinHeight);
			var scrollbarWidth = 0f;
			if (vScrollKnob != null) scrollbarWidth = vScrollKnob.MinWidth;
			if (_style.VScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, _style.VScroll.MinWidth);

			// Get available space size by subtracting background's padded area.
			_areaWidth = width - bgLeftWidth - bgRightWidth;
			_areaHeight = height - bgTopHeight - bgBottomHeight;

			if (_widget == null)
				return;

			// Get widget's desired width.
			float widgetWidth, widgetHeight;
			if (_widget is ILayout)
			{
				var layout = _widget as ILayout;
				widgetWidth = layout.PreferredWidth;
				widgetHeight = layout.PreferredHeight;
			}
			else
			{
				widgetWidth = _widget.GetWidth();
				widgetHeight = _widget.GetHeight();
			}

			// Determine if horizontal/vertical scrollbars are needed.
			_scrollX = _forceScrollX || (widgetWidth > _areaWidth && !_disableX);
			_scrollY = _forceScrollY || (widgetHeight > _areaHeight && !_disableY);

			var fade = _fadeScrollBars;
			if (!fade)
			{
				// Check again, now taking into account the area that's taken up by any enabled scrollbars.
				if (_scrollY)
				{
					_areaWidth -= scrollbarWidth;
					if (!_scrollX && widgetWidth > _areaWidth && !_disableX) _scrollX = true;
				}

				if (_scrollX)
				{
					_areaHeight -= scrollbarHeight;
					if (!_scrollY && widgetHeight > _areaHeight && !_disableY)
					{
						_scrollY = true;
						_areaWidth -= scrollbarWidth;
					}
				}
			}

			// the bounds of the scrollable area for the widget.
			_widgetAreaBounds = RectangleExt.FromFloats(bgLeftWidth, bgBottomHeight, _areaWidth, _areaHeight);

			if (fade)
			{
				// Make sure widget is drawn under fading scrollbars.
				if (_scrollX && _scrollY)
				{
					_areaHeight -= scrollbarHeight;
					_areaWidth -= scrollbarWidth;
				}
			}
			else
			{
				if (_scrollbarsOnTop)
				{
					// Make sure widget is drawn under non-fading scrollbars.
					if (_scrollX) _widgetAreaBounds.Height += (int)scrollbarHeight;
					if (_scrollY) _widgetAreaBounds.Width += (int)scrollbarWidth;
				}
				else
				{
					// Offset widget area y for horizontal scrollbar at bottom.
					if (_scrollX && _hScrollOnBottom) _widgetAreaBounds.Y += (int)scrollbarHeight;

					// Offset widget area x for vertical scrollbar at left.
					if (_scrollY && !_vScrollOnRight) _widgetAreaBounds.X += (int)scrollbarWidth;
				}
			}

			// If the widget is smaller than the available space, make it take up the available space.
			widgetWidth = _disableX ? _areaWidth : Math.Max(_areaWidth, widgetWidth);
			widgetHeight = _disableY ? _areaHeight : Math.Max(_areaHeight, widgetHeight);

			_maxX = widgetWidth - _areaWidth;
			_maxY = widgetHeight - _areaHeight;
			if (fade)
			{
				// Make sure widget is drawn under fading scrollbars.
				if (_scrollX && _scrollY)
				{
					_maxY -= scrollbarHeight;
					_maxX -= scrollbarWidth;
				}
			}

			SetScrollX(Mathf.Clamp(_amountX, 0, _maxX));
			SetScrollY(Mathf.Clamp(_amountY, 0, _maxY));

			// Set the bounds and scroll knob sizes if scrollbars are needed.
			if (_scrollX)
			{
				if (hScrollKnob != null)
				{
					var hScrollHeight = _style.HScroll != null ? _style.HScroll.MinHeight : hScrollKnob.MinHeight;

					// The corner gap where the two scroll bars intersect might have to flip from right to left.
					var boundsX = _vScrollOnRight ? bgLeftWidth : bgLeftWidth + scrollbarWidth;

					// Scrollbar on the top or bottom.
					var boundsY = _hScrollOnBottom ? bgBottomHeight : height - bgTopHeight - hScrollHeight;
					_hScrollBounds = RectangleExt.FromFloats(boundsX, boundsY, _areaWidth, hScrollHeight);
					if (_variableSizeKnobs)
						_hKnobBounds.Width = (int)Math.Max(hScrollKnob.MinWidth,
							(int)(_hScrollBounds.Width * _areaWidth / widgetWidth));
					else
						_hKnobBounds.Width = (int)hScrollKnob.MinWidth;

					_hKnobBounds.Height = (int)hScrollKnob.MinHeight;

					_hKnobBounds.X = _hScrollBounds.X +
									 (int)((_hScrollBounds.Width - _hKnobBounds.Width) * GetScrollPercentX());
					_hKnobBounds.Y = _hScrollBounds.Y;
				}
				else
				{
					_hScrollBounds = Rectangle.Empty;
					_hKnobBounds = Rectangle.Empty;
				}
			}

			if (_scrollY)
			{
				if (vScrollKnob != null)
				{
					var vScrollWidth = _style.VScroll != null ? _style.VScroll.MinWidth : vScrollKnob.MinWidth;

					// the small gap where the two scroll bars intersect might have to flip from bottom to top
					float boundsX, boundsY;
					if (_hScrollOnBottom)
						boundsY = height - bgTopHeight - _areaHeight;
					else
						boundsY = bgBottomHeight;

					// bar on the left or right
					if (_vScrollOnRight)
						boundsX = width - bgRightWidth - vScrollWidth;
					else
						boundsX = bgLeftWidth;

					_vScrollBounds = RectangleExt.FromFloats(boundsX, boundsY, vScrollWidth, _areaHeight);
					_vKnobBounds.Width = (int)vScrollKnob.MinWidth;
					if (_variableSizeKnobs)
						_vKnobBounds.Height = (int)Math.Max(vScrollKnob.MinHeight,
							(int)(_vScrollBounds.Height * _areaHeight / widgetHeight));
					else
						_vKnobBounds.Height = (int)vScrollKnob.MinHeight;

					if (_vScrollOnRight)
						_vKnobBounds.X = (int)(width - bgRightWidth - vScrollKnob.MinWidth);
					else
						_vKnobBounds.X = (int)bgLeftWidth;
					_vKnobBounds.Y = _vScrollBounds.Y +
									 (int)((_vScrollBounds.Height - _vKnobBounds.Height) * (1 - GetScrollPercentY()));
				}
				else
				{
					_vScrollBounds = Rectangle.Empty;
					_vKnobBounds = Rectangle.Empty;
				}
			}

			_widget.SetSize(widgetWidth, widgetHeight);
			if (_widget is ILayout)
				((ILayout)_widget).Validate();
		}

		#endregion


		#region IInputListener

		void IInputListener.OnMouseEnter()
		{
			ResetFade();
		}


		void IInputListener.OnMouseExit()
		{
			ResetFade();
		}


		bool IInputListener.OnMousePressed(Vector2 mousePos)
		{
			if (_scrollX && _hScrollBounds.Contains(mousePos))
			{
				ResetFade();
				if (_hKnobBounds.Contains(mousePos))
				{
					_lastMousePos = mousePos;
					_lastHandlePosition = _hKnobBounds.X;
					_touchScrollH = true;
					return true;
				}

				SetScrollX(_amountX + _areaWidth * (mousePos.X < _hKnobBounds.X ? -1 : 1));
				return true;
			}

			if (_scrollY && _vScrollBounds.Contains(mousePos))
			{
				ResetFade();
				if (_vKnobBounds.Contains(mousePos))
				{
					_lastMousePos = mousePos;
					_lastHandlePosition = _vKnobBounds.Y;
					_touchScrollV = true;
					return true;
				}

				SetScrollY(_amountY + _areaHeight * (mousePos.Y > _vKnobBounds.Y ? 1 : -1));
				return true;
			}

			return true;
		}


		void IInputListener.OnMouseMoved(Vector2 mousePos)
		{
			ResetFade();

			if (_touchScrollH)
			{
				var delta = mousePos.X - _lastMousePos.X;
				var scrollH = _lastHandlePosition + delta;
				_lastHandlePosition = scrollH;
				scrollH = Math.Max(_hScrollBounds.X, scrollH);
				scrollH = Math.Min(_hScrollBounds.X + _hScrollBounds.Width - _hKnobBounds.Width, scrollH);
				var total = _hScrollBounds.Width - _hKnobBounds.Width;
				if (total != 0)
					SetScrollPercentX((scrollH - _hScrollBounds.X) / total);
				_lastMousePos = mousePos;
			}
			else if (_touchScrollV)
			{
				var delta = mousePos.Y - _lastMousePos.Y;
				var scrollV = _lastHandlePosition + delta;
				_lastHandlePosition = scrollV;
				scrollV = Math.Max(_vScrollBounds.Y, scrollV);
				scrollV = Math.Min(_vScrollBounds.Y + _vScrollBounds.Height - _vKnobBounds.Height, scrollV);
				float total = _vScrollBounds.Height - _vKnobBounds.Height;
				if (total != 0)
				{
					var scrollAmount = (scrollV - _vScrollBounds.Y) / total;
					if (_useNaturalScrolling)
						SetScrollPercentY(scrollAmount);
					else
						SetScrollPercentY(1 - scrollAmount);
				}

				_lastMousePos = mousePos;
			}
		}


		void IInputListener.OnMouseUp(Vector2 mousePos)
		{
			Cancel();
		}


		bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
		{
			ResetFade();
			var scrollDirectionMultiplier = _useNaturalScrolling ? -1 : 1;
			if (_scrollY)
				SetScrollY(_amountY + mouseWheelDelta * _scrollSpeed * scrollDirectionMultiplier);
			else if (_scrollX)
				SetScrollX(_amountX + mouseWheelDelta * _scrollSpeed * scrollDirectionMultiplier);

			return true;
		}

		#endregion


		#region config

		public ScrollPane SetStyle(ScrollPaneStyle style)
		{
			Insist.IsNotNull(style, "style cannot be null");
			_style = style;
			InvalidateHierarchy();

			return this;
		}


		/// <summary>
		/// Returns the scroll pane's style. Modifying the returned style may not have an effect until {@link #setStyle(ScrollPaneStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public ScrollPaneStyle GetStyle()
		{
			return _style;
		}


		/// <summary>
		/// Sets the {@link Element} embedded in this scroll pane
		/// </summary>
		/// <param name="widget">Widget.</param>
		public ScrollPane SetWidget(Element widget)
		{
			if (_widget != null) RemoveElement(_widget);
			_widget = widget;
			if (widget != null) AddElement(widget);

			return this;
		}


		/// <summary>
		/// Returns the Element embedded in this scroll pane, or null
		/// </summary>
		/// <returns>The widget.</returns>
		public Element GetWidget()
		{
			return _widget;
		}


		/// <summary>
		/// sets the scroll speed when the mouse wheel is used to scroll the ScrollPane
		/// </summary>
		/// <param name="scrollSpeed">Scroll speed.</param>
		public ScrollPane SetScrollSpeed(float scrollSpeed)
		{
			_scrollSpeed = scrollSpeed;
			return this;
		}


		/// <summary>
		/// Returns the x scroll speed
		/// </summary>
		/// <returns>The scroll x.</returns>
		public float GetScrollSpeed()
		{
			return _scrollSpeed;
		}


		/// <summary>
		/// sets x scroll amount
		/// </summary>
		/// <param name="pixelsX">Pixels x.</param>
		public ScrollPane SetScrollX(float pixelsX)
		{
			_amountX = Mathf.Clamp(pixelsX, 0, _maxX);
			return this;
		}


		/// <summary>
		/// Returns the x scroll position in pixels, where 0 is the left of the scroll pane.
		/// </summary>
		/// <returns>The scroll x.</returns>
		public float GetScrollX()
		{
			return _amountX;
		}


		/// <summary>
		/// Called whenever the y scroll amount is changed
		/// </summary>
		/// <param name="pixelsY">Pixels y.</param>
		public ScrollPane SetScrollY(float pixelsY)
		{
			_amountY = Mathf.Clamp(pixelsY, 0, _maxY);
			return this;
		}


		/// <summary>
		/// Returns the y scroll position in pixels, where 0 is the top of the scroll pane.
		/// </summary>
		/// <returns>The scroll y.</returns>
		public float GetScrollY()
		{
			return _amountY;
		}


		/// <summary>
		/// sets how the mouse wheel/trackpad operates. Natural scrolling moves the contents of a window the same direction as
		/// your fingers.
		/// </summary>
		/// <param name="useNaturalScrolling">Use natural scrolling.</param>
		public ScrollPane SetUseNaturalScrolling(bool useNaturalScrolling)
		{
			_useNaturalScrolling = useNaturalScrolling;
			return this;
		}


		public bool GetUseNaturalScrolling()
		{
			return _useNaturalScrolling;
		}


		/// <summary>
		/// Sets the visual scroll amount equal to the scroll amount. This can be used when setting the scroll amount without animating.
		/// </summary>
		/// <returns>The visual scroll.</returns>
		public ScrollPane UpdateVisualScroll()
		{
			_visualAmountX = _amountX;
			_visualAmountY = _amountY;
			return this;
		}


		public float GetVisualScrollX()
		{
			return !_scrollX ? 0 : _visualAmountX;
		}


		public float GetVisualScrollY()
		{
			return !_scrollY ? 0 : _visualAmountY;
		}


		public float GetVisualScrollPercentX()
		{
			return Mathf.Clamp(_visualAmountX / _maxX, 0, 1);
		}


		public float GetVisualScrollPercentY()
		{
			return Mathf.Clamp(_visualAmountY / _maxY, 0, 1);
		}


		public float GetScrollPercentX()
		{
			return Mathf.Clamp(_amountX / _maxX, 0, 1);
		}


		public void SetScrollPercentX(float percentX)
		{
			SetScrollX(_maxX * Mathf.Clamp(percentX, 0, 1));
		}


		public float GetScrollPercentY()
		{
			return Mathf.Clamp(_amountY / _maxY, 0, 1);
		}


		public void SetScrollPercentY(float percentY)
		{
			SetScrollY(_maxY * Mathf.Clamp(percentY, 0, 1));
		}


		/// <summary>
		/// Returns the maximum scroll value in the x direction.
		/// </summary>
		/// <returns>The max x.</returns>
		public float GetMaxX()
		{
			return _maxX;
		}


		/// <summary>
		/// Returns the maximum scroll value in the y direction.
		/// </summary>
		/// <returns>The max y.</returns>
		public float GetMaxY()
		{
			return _maxY;
		}


		public float GetScrollBarHeight()
		{
			if (!_scrollX)
				return 0;

			var barheight = 0f;
			if (_style.HScrollKnob != null) barheight = _style.HScrollKnob.MinHeight;
			if (_style.HScroll != null) barheight = Math.Max(barheight, _style.HScroll.MinHeight);
			return barheight;
		}


		public float GetScrollBarWidth()
		{
			if (!_scrollY)
				return 0;

			var barWidth = 0f;
			if (_style.VScrollKnob != null) barWidth = _style.VScrollKnob.MinWidth;
			if (_style.VScroll != null) barWidth = Math.Max(barWidth, _style.VScroll.MinWidth);
			return barWidth;
		}


		/// <summary>
		/// Returns the width of the scrolled viewport.
		/// </summary>
		/// <returns>The scroll width.</returns>
		public float GetScrollWidth()
		{
			return _areaWidth;
		}


		/// <summary>
		/// Returns the height of the scrolled viewport.
		/// </summary>
		/// <returns>The scroll height.</returns>
		public float GetScrollHeight()
		{
			return _areaHeight;
		}


		/// <summary>
		/// Returns true if the widget is larger than the scroll pane horizontally.
		/// </summary>
		/// <returns>The scroll x.</returns>
		public bool IsScrollX()
		{
			return _scrollX;
		}


		/// <summary>
		/// Returns true if the widget is larger than the scroll pane vertically.
		/// </summary>
		/// <returns>The scroll y.</returns>
		public bool IsScrollY()
		{
			return _scrollY;
		}


		/// <summary>
		/// Disables scrolling in a direction. The widget will be sized to the FlickScrollPane in the disabled direction.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public ScrollPane SetScrollingDisabled(bool x, bool y)
		{
			_disableX = x;
			_disableY = y;
			return this;
		}


		public bool IsScrollingDisabledX()
		{
			return _disableX;
		}


		public bool IsScrollingDisabledY()
		{
			return _disableY;
		}


		public bool IsLeftEdge()
		{
			return !_scrollX || _amountX <= 0;
		}


		public bool IsRightEdge()
		{
			return !_scrollX || _amountX >= _maxX;
		}


		public bool IsTopEdge()
		{
			return !_scrollY || _amountY <= 0;
		}


		public bool IsBottomEdge()
		{
			return !_scrollY || _amountY >= _maxY;
		}


		public bool IsFlinging()
		{
			return _flingTimer > 0;
		}


		public void SetVelocityX(float velocityX)
		{
			_velocityX = velocityX;
		}


		/// <summary>
		/// Gets the flick scroll x velocity
		/// </summary>
		/// <returns>The velocity x.</returns>
		public float GetVelocityX()
		{
			return _velocityX;
		}


		public ScrollPane SetVelocityY(float velocityY)
		{
			_velocityY = velocityY;
			return this;
		}


		/// <summary>
		/// Gets the flick scroll y velocity
		/// </summary>
		/// <returns>The velocity y.</returns>
		public float GetVelocityY()
		{
			return _velocityY;
		}


		/// <summary>
		/// For flick scroll, if true the widget can be scrolled slightly past its bounds and will animate back to its bounds
		/// when scrolling is stopped. Default is true.
		/// </summary>
		/// <param name="overscrollX">Overscroll x.</param>
		/// <param name="overscrollY">Overscroll y.</param>
		public ScrollPane SetOverscroll(bool overscrollX, bool overscrollY)
		{
			_overscrollX = overscrollX;
			_overscrollY = overscrollY;
			return this;
		}


		/// <summary>
		/// For flick scroll, sets the overscroll distance in pixels and the speed it returns to the widget's bounds in seconds.
		/// Default is 50, 30, 200.
		/// </summary>
		/// <param name="distance">Distance.</param>
		/// <param name="speedMin">Speed minimum.</param>
		/// <param name="speedMax">Speed max.</param>
		public ScrollPane SetupOverscroll(float distance, float speedMin, float speedMax)
		{
			_overscrollDistance = distance;
			_overscrollSpeedMin = speedMin;
			_overscrollSpeedMax = speedMax;
			return this;
		}


		/// <summary>
		/// Forces enabling scrollbars (for non-flick scroll) and overscrolling (for flick scroll) in a direction, even if the
		/// contents do not exceed the bounds in that direction.
		/// </summary>
		/// <returns>The force scroll.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public ScrollPane SetForceScroll(bool x, bool y)
		{
			_forceScrollX = x;
			_forceScrollY = y;
			return this;
		}


		public bool IsForceScrollX()
		{
			return _forceScrollX;
		}


		public bool IsForceScrollY()
		{
			return _forceScrollY;
		}


		/// <summary>
		/// For flick scroll, sets the amount of time in seconds that a fling will continue to scroll. Default is 1.
		/// </summary>
		/// <param name="flingTime">Fling time.</param>
		public ScrollPane SetFlingTime(float flingTime)
		{
			_flingTime = flingTime;
			return this;
		}


		/// <summary>
		/// For flick scroll, prevents scrolling out of the widget's bounds. Default is true.
		/// </summary>
		/// <param name="clamp">Clamp.</param>
		public ScrollPane SetClamp(bool clamp)
		{
			_clamp = clamp;
			return this;
		}


		/// <summary>
		/// Set the position of the vertical and horizontal scroll bars.
		/// </summary>
		/// <param name="bottom">Bottom.</param>
		/// <param name="right">Right.</param>
		public ScrollPane SetScrollBarPositions(bool bottom, bool right)
		{
			_hScrollOnBottom = bottom;
			_vScrollOnRight = right;
			return this;
		}


		/// <summary>
		/// When true the scrollbars don't reduce the scrollable size and fade out after some time of not being used.
		/// </summary>
		/// <param name="fadeScrollBars">Fade scroll bars.</param>
		public ScrollPane SetFadeScrollBars(bool fadeScrollBars)
		{
			if (_fadeScrollBars == fadeScrollBars) return this;

			_fadeScrollBars = fadeScrollBars;
			if (!fadeScrollBars)
				_fadeAlpha = _fadeAlphaSeconds;
			Invalidate();
			return this;
		}


		public ScrollPane SetupFadeScrollBars(float fadeAlphaSeconds, float fadeDelaySeconds)
		{
			_fadeAlphaSeconds = fadeAlphaSeconds;
			_fadeDelaySeconds = fadeDelaySeconds;
			return this;
		}


		public ScrollPane SetSmoothScrolling(bool smoothScrolling)
		{
			_smoothScrolling = smoothScrolling;
			return this;
		}


		/// <summary>
		/// When false (the default), the widget is clipped so it is not drawn under the scrollbars. When true, the widget is clipped
		/// to the entire scroll pane bounds and the scrollbars are drawn on top of the widget. If {@link #setFadeScrollBars(boolean)}
		/// is true, the scroll bars are always drawn on top.
		/// </summary>
		/// <param name="scrollbarsOnTop">Scrollbars on top.</param>
		public ScrollPane SetScrollbarsOnTop(bool scrollbarsOnTop)
		{
			_scrollbarsOnTop = scrollbarsOnTop;
			Invalidate();
			return this;
		}


		public bool GetVariableSizeKnobs()
		{
			return _variableSizeKnobs;
		}


		/// <summary>
		/// If true, the scroll knobs are sized based on getMaxX() or getMaxY(). If false, the scroll knobs are sized
		/// based on Drawable#getMinWidth() or Drawable#getMinHeight(). Default is true.
		/// </summary>
		/// <param name="variableSizeKnobs">Variable size knobs.</param>
		public ScrollPane SetVariableSizeKnobs(bool variableSizeKnobs)
		{
			_variableSizeKnobs = variableSizeKnobs;
			return this;
		}

		#endregion


		/// <summary>
		/// Sets the scroll offset so the specified rectangle is fully in view, if possible. Coordinates are in the scroll pane
		/// widget's coordinate system.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void ScrollTo(float x, float y, float width, float height)
		{
			ScrollTo(x, y, width, height, false, false);
		}


		/// <summary>
		/// Sets the scroll offset so the specified rectangle is fully in view, and optionally centered vertically and/or horizontally,
		/// if possible. Coordinates are in the scroll pane widget's coordinate system.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="centerHorizontal">Center horizontal.</param>
		/// <param name="centerVertical">Center vertical.</param>
		public void ScrollTo(float x, float y, float width, float height, bool centerHorizontal, bool centerVertical)
		{
			var amountX = _amountX;
			if (centerHorizontal)
			{
				amountX = x - _areaWidth / 2 + width / 2;
			}
			else
			{
				if (x + width > amountX + _areaWidth) amountX = x + width - _areaWidth;
				if (x < amountX) amountX = x;
			}

			SetScrollX(amountX);

			var amountY = _amountY;
			if (centerVertical)
			{
				amountY = _maxY - y + _areaHeight / 2 - height / 2;
			}
			else
			{
				if (amountY > _maxY - y - height + _areaHeight) amountY = _maxY - y - height + _areaHeight;
				if (amountY < _maxY - y) amountY = _maxY - y;
			}

			SetScrollY(amountY);
		}


		public override Element Hit(Vector2 point)
		{
			// first we do a bounds check, then check our x and y scroll bars
			if (point.X < 0 || point.X >= GetWidth() || point.Y < 0 || point.Y >= GetHeight())
				return null;
			if (_scrollX && _hScrollBounds.Contains(point))
				return this;
			if (_scrollY && _vScrollBounds.Contains(point))
				return this;

			return base.Hit(point);
		}


		#region Internal getter/setters

		/// <summary>
		/// Called whenever the visual x scroll amount is changed
		/// </summary>
		/// <returns>The visual scroll x.</returns>
		/// <param name="pixelsX">Pixels x.</param>
		protected void SetVisualScrollX(float pixelsX)
		{
			_visualAmountX = pixelsX;
		}


		/// <summary>
		/// Called whenever the visual y scroll amount is changed
		/// </summary>
		/// <returns>The visual scroll y.</returns>
		/// <param name="pixelsY">Pixels y.</param>
		protected void SetVisualScrollY(float pixelsY)
		{
			_visualAmountY = pixelsY;
		}

		#endregion


		protected virtual void Update()
		{
			if (_fadeAlpha > 0 && _fadeScrollBars && !_touchScrollH && !_touchScrollV)
			{
				_fadeDelay -= Time.UnscaledDeltaTime;
				if (_fadeDelay <= 0)
					_fadeAlpha = Math.Max(0, _fadeAlpha - Time.UnscaledDeltaTime);
			}

			if (_flingTimer > 0)
			{
				ResetFade();

				var alpha = _flingTimer / _flingTime;
				_amountX -= _velocityX * alpha * Time.UnscaledDeltaTime;
				_amountY -= _velocityY * alpha * Time.UnscaledDeltaTime;
				Clamp();

				// Stop fling if hit overscroll distance.
				if (_amountX == -_overscrollDistance) _velocityX = 0;
				if (_amountX >= _maxX + _overscrollDistance) _velocityX = 0;
				if (_amountY == -_overscrollDistance) _velocityY = 0;
				if (_amountY >= _maxY + _overscrollDistance) _velocityY = 0;

				_flingTimer -= Time.UnscaledDeltaTime;
				if (_flingTimer <= 0)
				{
					_velocityX = 0;
					_velocityY = 0;
				}
			}

			if (_smoothScrolling && _flingTimer <= 0 &&

				// Scroll smoothly when grabbing the scrollbar if one pixel of scrollbar movement is > 10% of the scroll area.
				((!_touchScrollH ||
				  (_scrollX && _maxX / (_hScrollBounds.Width - _hKnobBounds.Width) > _areaWidth * 0.1f))
				 && (!_touchScrollV ||
					 (_scrollY && _maxY / (_vScrollBounds.Height - _vKnobBounds.Height) > _areaHeight * 0.1f)))
			)
			{
				if (_visualAmountX != _amountX)
				{
					ResetFade();
					if (_visualAmountX < _amountX)
						SetVisualScrollX(Math.Min(_amountX,
							_visualAmountX + Math.Max(2000 * Time.UnscaledDeltaTime,
								(_amountX - _visualAmountX) * 7 * Time.UnscaledDeltaTime)));
					else
						SetVisualScrollX(Math.Max(_amountX,
							_visualAmountX - Math.Max(2000 * Time.UnscaledDeltaTime,
								(_visualAmountX - _amountX) * 7 * Time.UnscaledDeltaTime)));
				}

				if (_visualAmountY != _amountY)
				{
					ResetFade();
					if (_visualAmountY < _amountY)
						SetVisualScrollY(Math.Min(_amountY,
							_visualAmountY + Math.Max(2000 * Time.UnscaledDeltaTime,
								(_amountY - _visualAmountY) * 7 * Time.UnscaledDeltaTime)));
					else
						SetVisualScrollY(Math.Max(_amountY,
							_visualAmountY - Math.Max(2000 * Time.UnscaledDeltaTime,
								(_visualAmountY - _amountY) * 7 * Time.UnscaledDeltaTime)));
				}
			}
			else
			{
				if (_visualAmountX != _amountX)
					SetVisualScrollX(_amountX);
				if (_visualAmountY != _amountY)
					SetVisualScrollY(_amountY);
			}

			if (_overscrollX && _scrollX)
			{
				if (_amountX < 0)
				{
					ResetFade();
					_amountX += (_overscrollSpeedMin +
								 (_overscrollSpeedMax - _overscrollSpeedMin) * -_amountX / _overscrollDistance) *
								Time.UnscaledDeltaTime;
					if (_amountX > 0) SetScrollX(0);
				}
				else if (_amountX > _maxX)
				{
					ResetFade();
					_amountX -= (_overscrollSpeedMin
								 + (_overscrollSpeedMax - _overscrollSpeedMin) * -(_maxX - _amountX) /
								 _overscrollDistance) * Time.UnscaledDeltaTime;
					if (_amountX < _maxX) SetScrollX(_maxX);
				}
			}

			if (_overscrollY && _scrollY)
			{
				if (_amountY < 0)
				{
					ResetFade();
					_amountY += (_overscrollSpeedMin +
								 (_overscrollSpeedMax - _overscrollSpeedMin) * -_amountY / _overscrollDistance) *
								Time.UnscaledDeltaTime;
					if (_amountY > 0)
						SetScrollY(0);
				}
				else if (_amountY > _maxY)
				{
					ResetFade();
					_amountY -= (_overscrollSpeedMin + (_overscrollSpeedMax - _overscrollSpeedMin) *
								 -(_maxY - _amountY) / _overscrollDistance) * Time.UnscaledDeltaTime;
					if (_amountY < _maxY)
						SetScrollY(_maxY);
				}
			}
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			if (_widget == null)
				return;

			Update();
			Validate();

			// setup transform for this group.
			if (transform)
				ApplyTransform(batcher, ComputeTransform());

			if (_scrollX)
				_hKnobBounds.X = _hScrollBounds.X +
								 (int)((_hScrollBounds.Width - _hKnobBounds.Width) * GetVisualScrollPercentX());
			if (_scrollY)
				_vKnobBounds.Y = _vScrollBounds.Y +
								 (int)((_vScrollBounds.Height - _vKnobBounds.Height) * GetVisualScrollPercentY());

			// calculate the widget's position depending on the scroll state and available widget area.
			float eleY = _widgetAreaBounds.Y;
			if (!_scrollY)
				eleY -= _maxY;
			else
				eleY -= _visualAmountY;

			float eleX = _widgetAreaBounds.Y;
			if (_scrollX)
				eleX -= (int)_visualAmountX;

			if (!_fadeScrollBars && _scrollbarsOnTop)
			{
				if (_scrollX && _hScrollOnBottom)
				{
					var scrollbarHeight = 0f;
					if (_style.HScrollKnob != null) scrollbarHeight = _style.HScrollKnob.MinHeight;
					if (_style.HScroll != null) scrollbarHeight = Math.Max(scrollbarHeight, _style.HScroll.MinHeight);
					eleY += scrollbarHeight;
				}

				if (_scrollY && !_vScrollOnRight)
				{
					var scrollbarWidth = 0f;
					if (_style.HScrollKnob != null) scrollbarWidth = _style.HScrollKnob.MinWidth;
					if (_style.HScroll != null) scrollbarWidth = Math.Max(scrollbarWidth, _style.HScroll.MinWidth);
					eleX += scrollbarWidth;
				}
			}

			_widget.SetPosition(eleX, eleY);

			if (_widget is ICullable)
			{
				var cull = new Rectangle(
					(int)(-_widget.GetX() + _widgetAreaBounds.X),
					(int)(-_widget.GetY() + _widgetAreaBounds.Y),
					_widgetAreaBounds.Width,
					_widgetAreaBounds.Height);
				((ICullable)_widget).SetCullingArea(cull);
			}

			// draw the background
			var color = GetColor();
			color = ColorExt.Create(color, (int)(color.A * parentAlpha));
			if (_style.Background != null)
				_style.Background.Draw(batcher, 0, 0, GetWidth(), GetHeight(), color);

			// caculate the scissor bounds based on the batch transform, the available widget area and the camera transform. We need to
			// project those to screen coordinates for OpenGL to consume.
			var scissor =
				ScissorStack.CalculateScissors(_stage?.Camera, batcher.TransformMatrix, _widgetAreaBounds);
			if (ScissorStack.PushScissors(scissor))
			{
				batcher.EnableScissorTest(true);
				DrawChildren(batcher, parentAlpha);
				batcher.EnableScissorTest(false);
				ScissorStack.PopScissors();
			}

			// render scrollbars and knobs on top
			var alpha = (float)color.A;
			color.A = (byte)(alpha * (_fadeAlpha / _fadeAlphaSeconds));
			if (_scrollX && _scrollY)
			{
				if (_style.Corner != null)
					_style.Corner.Draw(batcher, _hScrollBounds.X + _hScrollBounds.Width, _hScrollBounds.Y,
						_vScrollBounds.Width, _vScrollBounds.Y, color);
			}

			if (_scrollX)
			{
				if (_style.HScroll != null)
					_style.HScroll.Draw(batcher, _hScrollBounds.X, _hScrollBounds.Y, _hScrollBounds.Width,
						_hScrollBounds.Height, color);
				if (_style.HScrollKnob != null)
					_style.HScrollKnob.Draw(batcher, _hKnobBounds.X, _hKnobBounds.Y, _hKnobBounds.Width,
						_hKnobBounds.Height, color);
			}

			if (_scrollY)
			{
				if (_style.VScroll != null)
					_style.VScroll.Draw(batcher, _vScrollBounds.X, _vScrollBounds.Y, _vScrollBounds.Width,
						_vScrollBounds.Height, color);
				if (_style.VScrollKnob != null)
					_style.VScrollKnob.Draw(batcher, _vKnobBounds.X, _vKnobBounds.Y, _vKnobBounds.Width,
						_vKnobBounds.Height, color);
			}

			if (transform)
				ResetTransform(batcher);
		}


		public override void DebugRender(Batcher batcher)
		{
			if (transform)
				ApplyTransform(batcher, ComputeTransform());

			var scissor =
				ScissorStack.CalculateScissors(_stage?.Camera, batcher.TransformMatrix, _widgetAreaBounds);
			if (ScissorStack.PushScissors(scissor))
			{
				batcher.EnableScissorTest(true);
				DebugRenderChildren(batcher, 1f);
				batcher.EnableScissorTest(false);
				ScissorStack.PopScissors();
			}

			if (transform)
				ResetTransform(batcher);
		}


		/// <summary>
		/// Generate fling gesture
		/// </summary>
		/// <param name="flingTime">Fling time.</param>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public void Fling(float flingTime, float velocityX, float velocityY)
		{
			_flingTimer = flingTime;
			_velocityX = velocityX;
			_velocityY = velocityY;
		}
	}


	public class ScrollPaneStyle
	{
		/** Optional. */
		public IDrawable Background, Corner;

		/** Optional. */
		public IDrawable HScroll, HScrollKnob;

		/** Optional. */
		public IDrawable VScroll, VScrollKnob;


		public ScrollPaneStyle()
		{
		}


		public ScrollPaneStyle(IDrawable background, IDrawable hScroll, IDrawable hScrollKnob, IDrawable vScroll,
							   IDrawable vScrollKnob)
		{
			Background = background;
			HScroll = hScroll;
			HScrollKnob = hScrollKnob;
			VScroll = vScroll;
			VScrollKnob = vScrollKnob;
		}


		public ScrollPaneStyle(ScrollPaneStyle style)
		{
			Background = style.Background;
			HScroll = style.HScroll;
			HScrollKnob = style.HScrollKnob;
			VScroll = style.VScroll;
			VScrollKnob = style.VScrollKnob;
		}


		public ScrollPaneStyle Clone()
		{
			return new ScrollPaneStyle
			{
				Background = Background,
				Corner = Corner,
				HScroll = HScroll,
				HScrollKnob = HScrollKnob,
				VScroll = VScroll,
				VScrollKnob = VScrollKnob
			};
		}
	}
}