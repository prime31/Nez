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


		public ScrollPane( Element widget ) : this( widget, new ScrollPaneStyle() )
		{ }


		public ScrollPane( Element widget, Skin skin ) : this( widget, skin.get<ScrollPaneStyle>() )
		{ }


		public ScrollPane( Element widget, Skin skin, string styleName ) : this( widget, skin.get<ScrollPaneStyle>( styleName ) )
		{ }


		public ScrollPane( Element widget, ScrollPaneStyle style )
		{
			Assert.isNotNull( style, "style cannot be null" );
			transform = true;
			_style = style;
			setWidget( widget );
			setSize( 150, 150 );
		}


		void resetFade()
		{
			_fadeAlpha = _fadeAlphaSeconds;
			_fadeDelay = _fadeDelaySeconds;
		}


		/// <summary>
		/// If currently scrolling by tracking a touch down, stop scrolling.
		/// </summary>
		public void cancel()
		{
			_touchScrollH = false;
			_touchScrollV = false;
		}


		void clamp()
		{
			if( !_clamp )
				return;

			setScrollX( _overscrollX ? Mathf.clamp( _amountX, -_overscrollDistance, _maxX + _overscrollDistance )
				: Mathf.clamp( _amountX, 0, _maxX ) );
			setScrollY( _overscrollY ? Mathf.clamp( _amountY, -_overscrollDistance, _maxY + _overscrollDistance )
				: Mathf.clamp( _amountY, 0, _maxY ) );
		}


		#region ILayout

		public override float minWidth { get; } = 0;

		public override float minHeight { get; } = 0;

		public override float preferredWidth
		{
			get
			{
				if( _widget is ILayout )
				{
					var width = ( (ILayout)_widget ).preferredWidth;
					if( _style.background != null ) width += _style.background.leftWidth + _style.background.rightWidth;
					if( _forceScrollY )
					{
						var scrollbarWidth = 0f;
						if( _style.vScrollKnob != null ) scrollbarWidth = _style.vScrollKnob.minWidth;
						if( _style.vScroll != null ) scrollbarWidth = Math.Max( scrollbarWidth, _style.vScroll.minWidth );
						width += scrollbarWidth;
					}
					return width;
				}
				return 150;
			}
		}

		public override float preferredHeight
		{
			get
			{
				if( _widget is ILayout )
				{
					var height = ( (ILayout)_widget ).preferredHeight;
					if( _style.background != null ) height += _style.background.topHeight + _style.background.bottomHeight;
					if( _forceScrollX )
					{
						var scrollbarHeight = 0f;
						if( _style.hScrollKnob != null ) scrollbarHeight = _style.hScrollKnob.minHeight;
						if( _style.hScroll != null ) scrollbarHeight = Math.Max( scrollbarHeight, _style.hScroll.minHeight );
						height += scrollbarHeight;
					}
					return height;
				}
				return 150;
			}
		}

		public override void layout()
		{
			var bg = _style.background;
			var hScrollKnob = _style.hScrollKnob;
			var vScrollKnob = _style.vScrollKnob;

			float bgLeftWidth = 0, bgRightWidth = 0, bgTopHeight = 0, bgBottomHeight = 0;
			if( bg != null )
			{
				bgLeftWidth = bg.leftWidth;
				bgRightWidth = bg.rightWidth;
				bgTopHeight = bg.topHeight;
				bgBottomHeight = bg.bottomHeight;
			}

			var width = getWidth();
			var height = getHeight();

			var scrollbarHeight = 0f;
			if( hScrollKnob != null ) scrollbarHeight = hScrollKnob.minHeight;
			if( _style.hScroll != null ) scrollbarHeight = Math.Max( scrollbarHeight, _style.hScroll.minHeight );
			var scrollbarWidth = 0f;
			if( vScrollKnob != null ) scrollbarWidth = vScrollKnob.minWidth;
			if( _style.vScroll != null ) scrollbarWidth = Math.Max( scrollbarWidth, _style.vScroll.minWidth );

			// Get available space size by subtracting background's padded area.
			_areaWidth = width - bgLeftWidth - bgRightWidth;
			_areaHeight = height - bgTopHeight - bgBottomHeight;

			if( _widget == null )
				return;

			// Get widget's desired width.
			float widgetWidth, widgetHeight;
			if( _widget is ILayout )
			{
				var layout = _widget as ILayout;
				widgetWidth = layout.preferredWidth;
				widgetHeight = layout.preferredHeight;
			}
			else
			{
				widgetWidth = _widget.getWidth();
				widgetHeight = _widget.getHeight();
			}

			// Determine if horizontal/vertical scrollbars are needed.
			_scrollX = _forceScrollX || ( widgetWidth > _areaWidth && !_disableX );
			_scrollY = _forceScrollY || ( widgetHeight > _areaHeight && !_disableY );

			var fade = _fadeScrollBars;
			if( !fade )
			{
				// Check again, now taking into account the area that's taken up by any enabled scrollbars.
				if( _scrollY )
				{
					_areaWidth -= scrollbarWidth;
					if( !_scrollX && widgetWidth > _areaWidth && !_disableX ) _scrollX = true;
				}
				if( _scrollX )
				{
					_areaHeight -= scrollbarHeight;
					if( !_scrollY && widgetHeight > _areaHeight && !_disableY )
					{
						_scrollY = true;
						_areaWidth -= scrollbarWidth;
					}
				}
			}

			// the bounds of the scrollable area for the widget.
			_widgetAreaBounds = RectangleExt.fromFloats( bgLeftWidth, bgBottomHeight, _areaWidth, _areaHeight );

			if( fade )
			{
				// Make sure widget is drawn under fading scrollbars.
				if( _scrollX && _scrollY )
				{
					_areaHeight -= scrollbarHeight;
					_areaWidth -= scrollbarWidth;
				}
			}
			else
			{
				if( _scrollbarsOnTop )
				{
					// Make sure widget is drawn under non-fading scrollbars.
					if( _scrollX ) _widgetAreaBounds.Height += (int)scrollbarHeight;
					if( _scrollY ) _widgetAreaBounds.Width += (int)scrollbarWidth;
				}
				else
				{
					// Offset widget area y for horizontal scrollbar at bottom.
					if( _scrollX && _hScrollOnBottom ) _widgetAreaBounds.Y += (int)scrollbarHeight;
					// Offset widget area x for vertical scrollbar at left.
					if( _scrollY && !_vScrollOnRight ) _widgetAreaBounds.X += (int)scrollbarWidth;
				}
			}

			// If the widget is smaller than the available space, make it take up the available space.
			widgetWidth = _disableX ? _areaWidth : Math.Max( _areaWidth, widgetWidth );
			widgetHeight = _disableY ? _areaHeight : Math.Max( _areaHeight, widgetHeight );

			_maxX = widgetWidth - _areaWidth;
			_maxY = widgetHeight - _areaHeight;
			if( fade )
			{
				// Make sure widget is drawn under fading scrollbars.
				if( _scrollX && _scrollY )
				{
					_maxY -= scrollbarHeight;
					_maxX -= scrollbarWidth;
				}
			}
			setScrollX( Mathf.clamp( _amountX, 0, _maxX ) );
			setScrollY( Mathf.clamp( _amountY, 0, _maxY ) );

			// Set the bounds and scroll knob sizes if scrollbars are needed.
			if( _scrollX )
			{
				if( hScrollKnob != null )
				{
					var hScrollHeight = _style.hScroll != null ? _style.hScroll.minHeight : hScrollKnob.minHeight;
					// The corner gap where the two scroll bars intersect might have to flip from right to left.
					var boundsX = _vScrollOnRight ? bgLeftWidth : bgLeftWidth + scrollbarWidth;
					// Scrollbar on the top or bottom.
					var boundsY = _hScrollOnBottom ? bgBottomHeight : height - bgTopHeight - hScrollHeight;
					_hScrollBounds = RectangleExt.fromFloats( boundsX, boundsY, _areaWidth, hScrollHeight );
					if( _variableSizeKnobs )
						_hKnobBounds.Width = (int)Math.Max( hScrollKnob.minWidth, (int)( _hScrollBounds.Width * _areaWidth / widgetWidth ) );
					else
						_hKnobBounds.Width = (int)hScrollKnob.minWidth;

					_hKnobBounds.Height = (int)hScrollKnob.minHeight;

					_hKnobBounds.X = _hScrollBounds.X + (int)( ( _hScrollBounds.Width - _hKnobBounds.Width ) * getScrollPercentX() );
					_hKnobBounds.Y = _hScrollBounds.Y;
				}
				else
				{
					_hScrollBounds = Rectangle.Empty;
					_hKnobBounds = Rectangle.Empty;
				}
			}

			if( _scrollY )
			{
				if( vScrollKnob != null )
				{
					var vScrollWidth = _style.vScroll != null ? _style.vScroll.minWidth : vScrollKnob.minWidth;
					// the small gap where the two scroll bars intersect might have to flip from bottom to top
					float boundsX, boundsY;
					if( _hScrollOnBottom )
						boundsY = height - bgTopHeight - _areaHeight;
					else
						boundsY = bgBottomHeight;

					// bar on the left or right
					if( _vScrollOnRight )
						boundsX = width - bgRightWidth - vScrollWidth;
					else
						boundsX = bgLeftWidth;

					_vScrollBounds = RectangleExt.fromFloats( boundsX, boundsY, vScrollWidth, _areaHeight );
					_vKnobBounds.Width = (int)vScrollKnob.minWidth;
					if( _variableSizeKnobs )
						_vKnobBounds.Height = (int)Math.Max( vScrollKnob.minHeight, (int)( _vScrollBounds.Height * _areaHeight / widgetHeight ) );
					else
						_vKnobBounds.Height = (int)vScrollKnob.minHeight;

					if( _vScrollOnRight )
						_vKnobBounds.X = (int)( width - bgRightWidth - vScrollKnob.minWidth );
					else
						_vKnobBounds.X = (int)bgLeftWidth;
					_vKnobBounds.Y = _vScrollBounds.Y + (int)( ( _vScrollBounds.Height - _vKnobBounds.Height ) * ( 1 - getScrollPercentY() ) );
				}
				else
				{
					_vScrollBounds = Rectangle.Empty;
					_vKnobBounds = Rectangle.Empty;
				}
			}

			_widget.setSize( widgetWidth, widgetHeight );
			if( _widget is ILayout )
				( (ILayout)_widget ).validate();
		}

		#endregion


		#region IInputListener

		void IInputListener.onMouseEnter()
		{
			resetFade();
		}


		void IInputListener.onMouseExit()
		{
			resetFade();
		}


		bool IInputListener.onMousePressed( Vector2 mousePos )
		{
			if( _scrollX && _hScrollBounds.Contains( mousePos ) )
			{
				resetFade();
				if( _hKnobBounds.Contains( mousePos ) )
				{
					_lastMousePos = mousePos;
					_lastHandlePosition = _hKnobBounds.X;
					_touchScrollH = true;
					return true;
				}
				setScrollX( _amountX + _areaWidth * ( mousePos.X < _hKnobBounds.X ? -1 : 1 ) );
				return true;
			}

			if( _scrollY && _vScrollBounds.Contains( mousePos ) )
			{
				resetFade();
				if( _vKnobBounds.Contains( mousePos ) )
				{
					_lastMousePos = mousePos;
					_lastHandlePosition = _vKnobBounds.Y;
					_touchScrollV = true;
					return true;
				}
				setScrollY( _amountY + _areaHeight * ( mousePos.Y > _vKnobBounds.Y ? 1 : -1 ) );
				return true;
			}
			return true;
		}


		void IInputListener.onMouseMoved( Vector2 mousePos )
		{
			resetFade();

			if( _touchScrollH )
			{
				var delta = mousePos.X - _lastMousePos.X;
				var scrollH = _lastHandlePosition + delta;
				_lastHandlePosition = scrollH;
				scrollH = Math.Max( _hScrollBounds.X, scrollH );
				scrollH = Math.Min( _hScrollBounds.X + _hScrollBounds.Width - _hKnobBounds.Width, scrollH );
				var total = _hScrollBounds.Width - _hKnobBounds.Width;
				if( total != 0 )
					setScrollPercentX( ( scrollH - _hScrollBounds.X ) / total );
				_lastMousePos = mousePos;
			}
			else if( _touchScrollV )
			{
				var delta = mousePos.Y - _lastMousePos.Y;
				var scrollV = _lastHandlePosition + delta;
				_lastHandlePosition = scrollV;
				scrollV = Math.Max( _vScrollBounds.Y, scrollV );
				scrollV = Math.Min( _vScrollBounds.Y + _vScrollBounds.Height - _vKnobBounds.Height, scrollV );
				float total = _vScrollBounds.Height - _vKnobBounds.Height;
				if( total != 0 )
				{
					var scrollAmount = ( scrollV - _vScrollBounds.Y ) / total;
					if( _useNaturalScrolling )
						setScrollPercentY( scrollAmount );
					else
						setScrollPercentY( 1 - scrollAmount );
				}
				_lastMousePos = mousePos;
			}
		}


		void IInputListener.onMouseUp( Vector2 mousePos )
		{
			cancel();
		}


		bool IInputListener.onMouseScrolled( int mouseWheelDelta )
		{
			resetFade();
			var scrollDirectionMultiplier = _useNaturalScrolling ? -1 : 1;
			if( _scrollY )
				setScrollY( _amountY + mouseWheelDelta * _scrollSpeed * scrollDirectionMultiplier );
			else if( _scrollX )
				setScrollX( _amountX + mouseWheelDelta * _scrollSpeed * scrollDirectionMultiplier );

			return true;
		}

		#endregion


		#region config

		public ScrollPane setStyle( ScrollPaneStyle style )
		{
			Assert.isNotNull( style, "style cannot be null" );
			_style = style;
			invalidateHierarchy();

			return this;
		}


		/// <summary>
		/// Returns the scroll pane's style. Modifying the returned style may not have an effect until {@link #setStyle(ScrollPaneStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public ScrollPaneStyle getStyle()
		{
			return _style;
		}


		/// <summary>
		/// Sets the {@link Element} embedded in this scroll pane
		/// </summary>
		/// <param name="widget">Widget.</param>
		public ScrollPane setWidget( Element widget )
		{
			if( _widget != null ) removeElement( _widget );
			_widget = widget;
			if( widget != null ) addElement( widget );

			return this;
		}


		/// <summary>
		/// Returns the Element embedded in this scroll pane, or null
		/// </summary>
		/// <returns>The widget.</returns>
		public Element getWidget()
		{
			return _widget;
		}


		/// <summary>
		/// sets the scroll speed when the mouse wheel is used to scroll the ScrollPane
		/// </summary>
		/// <param name="scrollSpeed">Scroll speed.</param>
		public ScrollPane setScrollSpeed( float scrollSpeed )
		{
			_scrollSpeed = scrollSpeed;
			return this;
		}


		/// <summary>
		/// Returns the x scroll speed
		/// </summary>
		/// <returns>The scroll x.</returns>
		public float getScrollSpeed()
		{
			return _scrollSpeed;
		}


		/// <summary>
		/// sets x scroll amount
		/// </summary>
		/// <param name="pixelsX">Pixels x.</param>
		public ScrollPane setScrollX( float pixelsX )
		{
			_amountX = Mathf.clamp( pixelsX, 0, _maxX );
			return this;
		}


		/// <summary>
		/// Returns the x scroll position in pixels, where 0 is the left of the scroll pane.
		/// </summary>
		/// <returns>The scroll x.</returns>
		public float getScrollX()
		{
			return _amountX;
		}


		/// <summary>
		/// Called whenever the y scroll amount is changed
		/// </summary>
		/// <param name="pixelsY">Pixels y.</param>
		public ScrollPane setScrollY( float pixelsY )
		{
			_amountY = Mathf.clamp( pixelsY, 0, _maxY );
			return this;
		}


		/// <summary>
		/// Returns the y scroll position in pixels, where 0 is the top of the scroll pane.
		/// </summary>
		/// <returns>The scroll y.</returns>
		public float getScrollY()
		{
			return _amountY;
		}


		/// <summary>
		/// sets how the mouse wheel/trackpad operates. Natural scrolling moves the contents of a window the same direction as
		/// your fingers.
		/// </summary>
		/// <param name="useNaturalScrolling">Use natural scrolling.</param>
		public ScrollPane setUseNaturalScrolling( bool useNaturalScrolling )
		{
			_useNaturalScrolling = useNaturalScrolling;
			return this;
		}


		public bool getUseNaturalScrolling()
		{
			return _useNaturalScrolling;
		}


		/// <summary>
		/// Sets the visual scroll amount equal to the scroll amount. This can be used when setting the scroll amount without animating.
		/// </summary>
		/// <returns>The visual scroll.</returns>
		public ScrollPane updateVisualScroll()
		{
			_visualAmountX = _amountX;
			_visualAmountY = _amountY;
			return this;
		}


		public float getVisualScrollX()
		{
			return !_scrollX ? 0 : _visualAmountX;
		}


		public float getVisualScrollY()
		{
			return !_scrollY ? 0 : _visualAmountY;
		}


		public float getVisualScrollPercentX()
		{
			return Mathf.clamp( _visualAmountX / _maxX, 0, 1 );
		}


		public float getVisualScrollPercentY()
		{
			return Mathf.clamp( _visualAmountY / _maxY, 0, 1 );
		}


		public float getScrollPercentX()
		{
			return Mathf.clamp( _amountX / _maxX, 0, 1 );
		}


		public void setScrollPercentX( float percentX )
		{
			setScrollX( _maxX * Mathf.clamp( percentX, 0, 1 ) );
		}


		public float getScrollPercentY()
		{
			return Mathf.clamp( _amountY / _maxY, 0, 1 );
		}


		public void setScrollPercentY( float percentY )
		{
			setScrollY( _maxY * Mathf.clamp( percentY, 0, 1 ) );
		}


		/// <summary>
		/// Returns the maximum scroll value in the x direction.
		/// </summary>
		/// <returns>The max x.</returns>
		public float getMaxX()
		{
			return _maxX;
		}


		/// <summary>
		/// Returns the maximum scroll value in the y direction.
		/// </summary>
		/// <returns>The max y.</returns>
		public float getMaxY()
		{
			return _maxY;
		}


		public float getScrollBarHeight()
		{
			if( !_scrollX )
				return 0;
			var barheight = 0f;
			if( _style.hScrollKnob != null ) barheight = _style.hScrollKnob.minHeight;
			if( _style.hScroll != null ) barheight = Math.Max( barheight, _style.hScroll.minHeight );
			return barheight;
		}


		public float getScrollBarWidth()
		{
			if( !_scrollY )
				return 0;
			var barWidth = 0f;
			if( _style.vScrollKnob != null ) barWidth = _style.vScrollKnob.minWidth;
			if( _style.vScroll != null ) barWidth = Math.Max( barWidth, _style.vScroll.minWidth );
			return barWidth;
		}


		/// <summary>
		/// Returns the width of the scrolled viewport.
		/// </summary>
		/// <returns>The scroll width.</returns>
		public float getScrollWidth()
		{
			return _areaWidth;
		}


		/// <summary>
		/// Returns the height of the scrolled viewport.
		/// </summary>
		/// <returns>The scroll height.</returns>
		public float getScrollHeight()
		{
			return _areaHeight;
		}


		/// <summary>
		/// Returns true if the widget is larger than the scroll pane horizontally.
		/// </summary>
		/// <returns>The scroll x.</returns>
		public bool isScrollX()
		{
			return _scrollX;
		}


		/// <summary>
		/// Returns true if the widget is larger than the scroll pane vertically.
		/// </summary>
		/// <returns>The scroll y.</returns>
		public bool isScrollY()
		{
			return _scrollY;
		}


		/// <summary>
		/// Disables scrolling in a direction. The widget will be sized to the FlickScrollPane in the disabled direction.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public ScrollPane setScrollingDisabled( bool x, bool y )
		{
			_disableX = x;
			_disableY = y;
			return this;
		}


		public bool isScrollingDisabledX()
		{
			return _disableX;
		}


		public bool isScrollingDisabledY()
		{
			return _disableY;
		}


		public bool isLeftEdge()
		{
			return !_scrollX || _amountX <= 0;
		}


		public bool isRightEdge()
		{
			return !_scrollX || _amountX >= _maxX;
		}


		public bool isTopEdge()
		{
			return !_scrollY || _amountY <= 0;
		}


		public bool isBottomEdge()
		{
			return !_scrollY || _amountY >= _maxY;
		}


		public bool isFlinging()
		{
			return _flingTimer > 0;
		}


		public void setVelocityX( float velocityX )
		{
			_velocityX = velocityX;
		}


		/// <summary>
		/// Gets the flick scroll x velocity
		/// </summary>
		/// <returns>The velocity x.</returns>
		public float getVelocityX()
		{
			return _velocityX;
		}


		public ScrollPane setVelocityY( float velocityY )
		{
			_velocityY = velocityY;
			return this;
		}


		/// <summary>
		/// Gets the flick scroll y velocity
		/// </summary>
		/// <returns>The velocity y.</returns>
		public float getVelocityY()
		{
			return _velocityY;
		}


		/// <summary>
		/// For flick scroll, if true the widget can be scrolled slightly past its bounds and will animate back to its bounds
		/// when scrolling is stopped. Default is true.
		/// </summary>
		/// <param name="overscrollX">Overscroll x.</param>
		/// <param name="overscrollY">Overscroll y.</param>
		public ScrollPane setOverscroll( bool overscrollX, bool overscrollY )
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
		public ScrollPane setupOverscroll( float distance, float speedMin, float speedMax )
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
		public ScrollPane setForceScroll( bool x, bool y )
		{
			_forceScrollX = x;
			_forceScrollY = y;
			return this;
		}


		public bool isForceScrollX()
		{
			return _forceScrollX;
		}


		public bool isForceScrollY()
		{
			return _forceScrollY;
		}


		/// <summary>
		/// For flick scroll, sets the amount of time in seconds that a fling will continue to scroll. Default is 1.
		/// </summary>
		/// <param name="flingTime">Fling time.</param>
		public ScrollPane setFlingTime( float flingTime )
		{
			_flingTime = flingTime;
			return this;
		}


		/// <summary>
		/// For flick scroll, prevents scrolling out of the widget's bounds. Default is true.
		/// </summary>
		/// <param name="clamp">Clamp.</param>
		public ScrollPane setClamp( bool clamp )
		{
			_clamp = clamp;
			return this;
		}


		/// <summary>
		/// Set the position of the vertical and horizontal scroll bars.
		/// </summary>
		/// <param name="bottom">Bottom.</param>
		/// <param name="right">Right.</param>
		public ScrollPane setScrollBarPositions( bool bottom, bool right )
		{
			_hScrollOnBottom = bottom;
			_vScrollOnRight = right;
			return this;
		}


		/// <summary>
		/// When true the scrollbars don't reduce the scrollable size and fade out after some time of not being used.
		/// </summary>
		/// <param name="fadeScrollBars">Fade scroll bars.</param>
		public ScrollPane setFadeScrollBars( bool fadeScrollBars )
		{
			if( _fadeScrollBars == fadeScrollBars ) return this;
			_fadeScrollBars = fadeScrollBars;
			if( !fadeScrollBars )
				_fadeAlpha = _fadeAlphaSeconds;
			invalidate();
			return this;
		}


		public ScrollPane setupFadeScrollBars( float fadeAlphaSeconds, float fadeDelaySeconds )
		{
			_fadeAlphaSeconds = fadeAlphaSeconds;
			_fadeDelaySeconds = fadeDelaySeconds;
			return this;
		}


		public ScrollPane setSmoothScrolling( bool smoothScrolling )
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
		public ScrollPane setScrollbarsOnTop( bool scrollbarsOnTop )
		{
			_scrollbarsOnTop = scrollbarsOnTop;
			invalidate();
			return this;
		}


		public bool getVariableSizeKnobs()
		{
			return _variableSizeKnobs;
		}


		/// <summary>
		/// If true, the scroll knobs are sized based on getMaxX() or getMaxY(). If false, the scroll knobs are sized
		/// based on Drawable#getMinWidth() or Drawable#getMinHeight(). Default is true.
		/// </summary>
		/// <param name="variableSizeKnobs">Variable size knobs.</param>
		public ScrollPane setVariableSizeKnobs( bool variableSizeKnobs )
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
		public void scrollTo( float x, float y, float width, float height )
		{
			scrollTo( x, y, width, height, false, false );
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
		public void scrollTo( float x, float y, float width, float height, bool centerHorizontal, bool centerVertical )
		{
			var amountX = _amountX;
			if( centerHorizontal )
			{
				amountX = x - _areaWidth / 2 + width / 2;
			}
			else
			{
				if( x + width > amountX + _areaWidth ) amountX = x + width - _areaWidth;
				if( x < amountX ) amountX = x;
			}
			setScrollX( amountX );

			var amountY = _amountY;
			if( centerVertical )
			{
				amountY = _maxY - y + _areaHeight / 2 - height / 2;
			}
			else
			{
				if( amountY > _maxY - y - height + _areaHeight ) amountY = _maxY - y - height + _areaHeight;
				if( amountY < _maxY - y ) amountY = _maxY - y;
			}
			setScrollY( amountY );
		}


		public override Element hit( Vector2 point )
		{
			// first we do a bounds check, then check our x and y scroll bars
			if( point.X < 0 || point.X >= getWidth() || point.Y < 0 || point.Y >= getHeight() )
				return null;
			if( _scrollX && _hScrollBounds.Contains( point ) )
				return this;
			if( _scrollY && _vScrollBounds.Contains( point ) )
				return this;

			return base.hit( point );
		}


		#region Internal getter/setters

		/// <summary>
		/// Called whenever the visual x scroll amount is changed
		/// </summary>
		/// <returns>The visual scroll x.</returns>
		/// <param name="pixelsX">Pixels x.</param>
		protected void setVisualScrollX( float pixelsX )
		{
			_visualAmountX = pixelsX;
		}


		/// <summary>
		/// Called whenever the visual y scroll amount is changed
		/// </summary>
		/// <returns>The visual scroll y.</returns>
		/// <param name="pixelsY">Pixels y.</param>
		protected void setVisualScrollY( float pixelsY )
		{
			_visualAmountY = pixelsY;
		}

		#endregion


		protected virtual void update()
		{
			if( _fadeAlpha > 0 && _fadeScrollBars && !_touchScrollH && !_touchScrollV )
			{
				_fadeDelay -= Time.unscaledDeltaTime;
				if( _fadeDelay <= 0 )
					_fadeAlpha = Math.Max( 0, _fadeAlpha - Time.unscaledDeltaTime );
			}

			if( _flingTimer > 0 )
			{
				resetFade();

				var alpha = _flingTimer / _flingTime;
				_amountX -= _velocityX * alpha * Time.unscaledDeltaTime;
				_amountY -= _velocityY * alpha * Time.unscaledDeltaTime;
				clamp();

				// Stop fling if hit overscroll distance.
				if( _amountX == -_overscrollDistance ) _velocityX = 0;
				if( _amountX >= _maxX + _overscrollDistance ) _velocityX = 0;
				if( _amountY == -_overscrollDistance ) _velocityY = 0;
				if( _amountY >= _maxY + _overscrollDistance ) _velocityY = 0;

				_flingTimer -= Time.unscaledDeltaTime;
				if( _flingTimer <= 0 )
				{
					_velocityX = 0;
					_velocityY = 0;
				}
			}

			if( _smoothScrolling && _flingTimer <= 0 &&
			   // Scroll smoothly when grabbing the scrollbar if one pixel of scrollbar movement is > 10% of the scroll area.
			   ( ( !_touchScrollH || ( _scrollX && _maxX / ( _hScrollBounds.Width - _hKnobBounds.Width ) > _areaWidth * 0.1f ) )
				&& ( !_touchScrollV || ( _scrollY && _maxY / ( _vScrollBounds.Height - _vKnobBounds.Height ) > _areaHeight * 0.1f ) ) )
			)
			{
				if( _visualAmountX != _amountX )
				{
					resetFade();
					if( _visualAmountX < _amountX )
						setVisualScrollX( Math.Min( _amountX, _visualAmountX + Math.Max( 2000 * Time.unscaledDeltaTime, ( _amountX - _visualAmountX ) * 7 * Time.unscaledDeltaTime ) ) );
					else
						setVisualScrollX( Math.Max( _amountX, _visualAmountX - Math.Max( 2000 * Time.unscaledDeltaTime, ( _visualAmountX - _amountX ) * 7 * Time.unscaledDeltaTime ) ) );
				}
				if( _visualAmountY != _amountY )
				{
					resetFade();
					if( _visualAmountY < _amountY )
						setVisualScrollY( Math.Min( _amountY, _visualAmountY + Math.Max( 2000 * Time.unscaledDeltaTime, ( _amountY - _visualAmountY ) * 7 * Time.unscaledDeltaTime ) ) );
					else
						setVisualScrollY( Math.Max( _amountY, _visualAmountY - Math.Max( 2000 * Time.unscaledDeltaTime, ( _visualAmountY - _amountY ) * 7 * Time.unscaledDeltaTime ) ) );
				}
			}
			else
			{
				if( _visualAmountX != _amountX )
					setVisualScrollX( _amountX );
				if( _visualAmountY != _amountY )
					setVisualScrollY( _amountY );
			}

			if( _overscrollX && _scrollX )
			{
				if( _amountX < 0 )
				{
					resetFade();
					_amountX += ( _overscrollSpeedMin + ( _overscrollSpeedMax - _overscrollSpeedMin ) * -_amountX / _overscrollDistance ) * Time.unscaledDeltaTime;
					if( _amountX > 0 ) setScrollX( 0 );
				}
				else if( _amountX > _maxX )
				{
					resetFade();
					_amountX -= ( _overscrollSpeedMin
						+ ( _overscrollSpeedMax - _overscrollSpeedMin ) * -( _maxX - _amountX ) / _overscrollDistance ) * Time.unscaledDeltaTime;
					if( _amountX < _maxX ) setScrollX( _maxX );
				}
			}
			if( _overscrollY && _scrollY )
			{
				if( _amountY < 0 )
				{
					resetFade();
					_amountY += ( _overscrollSpeedMin + ( _overscrollSpeedMax - _overscrollSpeedMin ) * -_amountY / _overscrollDistance ) * Time.unscaledDeltaTime;
					if( _amountY > 0 )
						setScrollY( 0 );
				}
				else if( _amountY > _maxY )
				{
					resetFade();
					_amountY -= ( _overscrollSpeedMin + ( _overscrollSpeedMax - _overscrollSpeedMin ) * -( _maxY - _amountY ) / _overscrollDistance ) * Time.unscaledDeltaTime;
					if( _amountY < _maxY )
						setScrollY( _maxY );
				}
			}
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			if( _widget == null )
				return;

			update();
			validate();

			// setup transform for this group.
			if( transform )
				applyTransform( graphics, computeTransform() );

			if( _scrollX )
				_hKnobBounds.X = _hScrollBounds.X + (int)( ( _hScrollBounds.Width - _hKnobBounds.Width ) * getVisualScrollPercentX() );
			if( _scrollY )
				_vKnobBounds.Y = _vScrollBounds.Y + (int)( ( _vScrollBounds.Height - _vKnobBounds.Height ) * getVisualScrollPercentY() );

			// calculate the widget's position depending on the scroll state and available widget area.
			float eleY = _widgetAreaBounds.Y;
			if( !_scrollY )
				eleY -= _maxY;
			else
				eleY -= _visualAmountY;

			float eleX = _widgetAreaBounds.Y;
			if( _scrollX )
				eleX -= (int)_visualAmountX;

			if( !_fadeScrollBars && _scrollbarsOnTop )
			{
				if( _scrollX && _hScrollOnBottom )
				{
					var scrollbarHeight = 0f;
					if( _style.hScrollKnob != null ) scrollbarHeight = _style.hScrollKnob.minHeight;
					if( _style.hScroll != null ) scrollbarHeight = Math.Max( scrollbarHeight, _style.hScroll.minHeight );
					eleY += scrollbarHeight;
				}
				if( _scrollY && !_vScrollOnRight )
				{
					var scrollbarWidth = 0f;
					if( _style.hScrollKnob != null ) scrollbarWidth = _style.hScrollKnob.minWidth;
					if( _style.hScroll != null ) scrollbarWidth = Math.Max( scrollbarWidth, _style.hScroll.minWidth );
					eleX += scrollbarWidth;
				}
			}

			_widget.setPosition( eleX, eleY );

			if( _widget is ICullable )
			{
				var cull = new Rectangle(
					(int) (-_widget.getX() + _widgetAreaBounds.X),
					(int) (-_widget.getY() + _widgetAreaBounds.Y),
					_widgetAreaBounds.Width,
					_widgetAreaBounds.Height);
				((ICullable) _widget).setCullingArea(cull);
			}
			
			// draw the background
			var color = getColor();
			color = new Color( color, (int)(color.A * parentAlpha) );
			if( _style.background != null )
				_style.background.draw( graphics, 0, 0, getWidth(), getHeight(), color );

			// caculate the scissor bounds based on the batch transform, the available widget area and the camera transform. We need to
			// project those to screen coordinates for OpenGL to consume.
			var scissor = ScissorStack.calculateScissors( stage?.camera, graphics.batcher.transformMatrix, _widgetAreaBounds );
			if( ScissorStack.pushScissors( scissor ) )
			{
				graphics.batcher.enableScissorTest( true );
				drawChildren( graphics, parentAlpha );
				graphics.batcher.enableScissorTest( false );
				ScissorStack.popScissors();
			}

			// render scrollbars and knobs on top
			var alpha = (float)color.A;
			color.A = (byte)( alpha * ( _fadeAlpha / _fadeAlphaSeconds ) );
			if( _scrollX && _scrollY )
			{
				if( _style.corner != null )
					_style.corner.draw( graphics, _hScrollBounds.X + _hScrollBounds.Width, _hScrollBounds.Y, _vScrollBounds.Width, _vScrollBounds.Y, color );
			}
			if( _scrollX )
			{
				if( _style.hScroll != null )
					_style.hScroll.draw( graphics, _hScrollBounds.X, _hScrollBounds.Y, _hScrollBounds.Width, _hScrollBounds.Height, color );
				if( _style.hScrollKnob != null )
					_style.hScrollKnob.draw( graphics, _hKnobBounds.X, _hKnobBounds.Y, _hKnobBounds.Width, _hKnobBounds.Height, color );
			}
			if( _scrollY )
			{
				if( _style.vScroll != null )
					_style.vScroll.draw( graphics, _vScrollBounds.X, _vScrollBounds.Y, _vScrollBounds.Width, _vScrollBounds.Height, color );
				if( _style.vScrollKnob != null )
					_style.vScrollKnob.draw( graphics, _vKnobBounds.X, _vKnobBounds.Y, _vKnobBounds.Width, _vKnobBounds.Height, color );
			}

			if( transform )
				resetTransform( graphics );
		}


		public override void debugRender( Graphics graphics )
		{
			if( transform )
				applyTransform( graphics, computeTransform() );

			var scissor = ScissorStack.calculateScissors( stage?.camera, graphics.batcher.transformMatrix, _widgetAreaBounds );
			if( ScissorStack.pushScissors( scissor ) )
			{
				graphics.batcher.enableScissorTest( true );
				debugRenderChildren( graphics, 1f );
				graphics.batcher.enableScissorTest( false );
				ScissorStack.popScissors();
			}

			if( transform )
				resetTransform( graphics );
		}


		/// <summary>
		/// Generate fling gesture
		/// </summary>
		/// <param name="flingTime">Fling time.</param>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public void fling( float flingTime, float velocityX, float velocityY )
		{
			_flingTimer = flingTime;
			_velocityX = velocityX;
			_velocityY = velocityY;
		}

	}


	public class ScrollPaneStyle
	{
		/** Optional. */
		public IDrawable background, corner;
		/** Optional. */
		public IDrawable hScroll, hScrollKnob;
		/** Optional. */
		public IDrawable vScroll, vScrollKnob;


		public ScrollPaneStyle()
		{ }


		public ScrollPaneStyle( IDrawable background, IDrawable hScroll, IDrawable hScrollKnob, IDrawable vScroll, IDrawable vScrollKnob )
		{
			this.background = background;
			this.hScroll = hScroll;
			this.hScrollKnob = hScrollKnob;
			this.vScroll = vScroll;
			this.vScrollKnob = vScrollKnob;
		}


		public ScrollPaneStyle( ScrollPaneStyle style )
		{
			background = style.background;
			hScroll = style.hScroll;
			hScrollKnob = style.hScrollKnob;
			vScroll = style.vScroll;
			vScrollKnob = style.vScrollKnob;
		}


		public ScrollPaneStyle clone()
		{
			return new ScrollPaneStyle
			{
				background = background,
				corner = corner,
				hScroll = hScroll,
				hScrollKnob = hScrollKnob,
				vScroll = vScroll,
				vScrollKnob = vScrollKnob
			};
		}

	}

}

