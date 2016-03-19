using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A group with a single child that sizes and positions the child using constraints. This provides layout similar to a
	/// {@link Table} with a single cell but is more lightweight.
	/// </summary>
	public class Container : Group
	{
		#region ILayout

		public override float minWidth
		{
			get { return getMinWidth(); }
		}

		public override float minHeight
		{
			get { return getPrefHeight(); }
		}

		public override float preferredWidth
		{
			get { return getPrefWidth(); }
		}

		public override float preferredHeight
		{
			get { return getPrefHeight(); }
		}

		public override float maxWidth
		{
			get { return getMaxWidth(); }
		}

		public override float maxHeight
		{
			get { return getMaxHeight(); }
		}

		#endregion


		Element _element;
		Value _minWidthValue = Value.minWidth, _minHeightValue = Value.minHeight;
		Value _prefWidthValue = Value.prefWidth, _prefHeightValue = Value.prefHeight;
		Value _maxWidthValue = Value.zero, _maxHeightValue = Value.zero;
		Value _padTop = Value.zero, _padLeft = Value.zero, _padBottom = Value.zero, _padRight = Value.zero;
		float _fillX, _fillY;
		int _align;
		IDrawable _background;
		bool _clip;
		bool _round = true;


		/// <summary>
		/// Creates a container with no element
		/// </summary>
		public Container()
		{
			setTouchable( Touchable.ChildrenOnly );
			transform = false;
		}


		public Container( Element element ) : this()
		{
			setElement( element );
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			validate();
			if( transform )
			{
				applyTransform( graphics, computeTransform() );
				drawBackground( graphics, parentAlpha, 0, 0 );
				if( _clip )
				{
					//graphics.flush();
					//float padLeft = this.padLeft.get( this ), padBottom = this.padBottom.get( this );
					//if( clipBegin( padLeft, padBottom,minWidthValueh() - padLeft - padRight.get( tmaxWidth				//	     getHeight() - padBottom - padTop.get( this ) ) )
					{
						drawChildren( graphics, parentAlpha );
						//graphics.flush();
						//clipEnd();
					}
				}
				else
				{
					drawChildren( graphics, parentAlpha );
				}
				resetTransform( graphics );
			}
			else
			{
				drawBackground( graphics, parentAlpha, getX(), getY() );
				base.draw( graphics, parentAlpha );
			}
		}


		/// <summary>
		/// Called to draw the background, before clipping is applied (if enabled). Default implementation draws the background drawable.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="parentAlpha">Parent alpha.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		protected void drawBackground( Graphics graphics, float parentAlpha, float x, float y )
		{
			if( _background == null )
				return;

			_background.draw( graphics, x, y, getWidth(), getHeight(), new Color( color, color.A * parentAlpha ) );
		}


		/// <summary>
		/// Sets the background drawable and adjusts the container's padding to match the background.
		/// </summary>
		/// <param name="background">Background.</param>
		public void setBackground( IDrawable background )
		{
			setBackground( background, true );
		}


		/// <summary>
		/// Sets the background drawable and, if adjustPadding is true, sets the container's padding to
		/// {@link Drawable#getBottomHeight()} , {@link Drawable#getTopHeight()}, {@link Drawable#getLeftWidth()}, and
		/// {@link Drawable#getRightWidth()}.
		/// If background is null, the background will be cleared and padding removed.
		/// </summary>
		/// <returns>The background.</returns>
		/// <param name="background">Background.</param>
		/// <param name="adjustPadding">If set to <c>true</c> adjust padding.</param>
		public Container setBackground( IDrawable background, bool adjustPadding )
		{
			if( this._background == background )
				return this;

			this._background = background;
			if( adjustPadding )
			{
				if( background == null )
					setPad( Value.zero );
				else
					setPad( background.topHeight, background.leftWidth, background.bottomHeight, background.rightWidth );
				invalidate();
			}
			return this;
		}


		public IDrawable getBackground()
		{
			return _background;
		}


		public override void layout()
		{
			if( _element == null )
				return;

			float padLeft = this._padLeft.get( this ), padBottom = this._padBottom.get( this );
			float containerWidth = getWidth() - padLeft - _padRight.get( this );
			float containerHeight = getHeight() - padBottom - _padTop.get( this );
			float minWidth = this._minWidthValue.get( _element ), minHeight = this._minHeightValue.get( _element );
			float prefWidth = this._prefWidthValue.get( _element ), prefHeight = this._prefHeightValue.get( _element );
			float maxWidth = this._maxWidthValue.get( _element ), maxHeight = this._maxHeightValue.get( _element );

			float width;
			if( _fillX > 0 )
				width = containerWidth * _fillX;
			else
				width = Math.Min( prefWidth, containerWidth );
			if( width < minWidth )
				width = minWidth;
			if( maxWidth > 0 && width > maxWidth )
				width = maxWidth;

			float height;
			if( _fillY > 0 )
				height = containerHeight * _fillY;
			else
				height = Math.Min( prefHeight, containerHeight );

			if( height < minHeight )
				height = minHeight;
			if( maxHeight > 0 && height > maxHeight )
				height = maxHeight;

			var x = padLeft;
			if( ( _align & AlignInternal.right ) != 0 )
				x += containerWidth - width;
			else if( ( _align & AlignInternal.left ) == 0 ) // center
				x += ( containerWidth - width ) / 2;

			float y = padBottom;
			if( ( _align & AlignInternal.top ) != 0 )
				y += containerHeight - height;
			else if( ( _align & AlignInternal.bottom ) == 0 ) // center
				y += ( containerHeight - height ) / 2;

			if( _round )
			{
				x = Mathf.round( x );
				y = Mathf.round( y );
				width = Mathf.round( width );
				height = Mathf.round( height );
			}

			_element.setBounds( x, y, width, height );
			if( _element is ILayout )
				( (ILayout)_element ).validate();
		}


		/// <summary>
		/// element may be null
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="element">element.</param>
		public virtual Element setElement( Element element )
		{
			if( element == this )
				throw new Exception( "element cannot be the Container." );
			if( element == this._element )
				return element;

			if( this._element != null )
				base.removeElement( this._element );
			this._element = element;
			if( element != null )
				return base.addElement( element );
			return null;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The element.</returns>
		public T getElement<T>() where T : Element
		{
			return _element as T;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The element.</returns>
		public Element getElement()
		{
			return _element;
		}


		public override bool removeElement( Element element )
		{
			if( element != this._element )
				return false;
			setElement( null );
			return true;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Container setSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			_minWidthValue = size;
			_minHeightValue = size;
			_prefWidthValue = size;
			_prefHeightValue = size;
			_maxWidthValue = size;
			_maxHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container setSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			_minWidthValue = width;
			_minHeightValue = height;
			_prefWidthValue = width;
			_prefHeightValue = height;
			_maxWidthValue = width;
			_maxHeightValue = height;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Container setSize( float size )
		{
			setSize( new Value.Fixed( size ) );
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public new Container setSize( float width, float height )
		{
			setSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, and maxWidth to the specified value
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="width">Width.</param>
		public Container setWidth( Value width )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			_minWidthValue = width;
			_prefWidthValue = width;
			_maxWidthValue = width;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, and maxWidth to the specified value
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="width">Width.</param>
		public new Container setWidth( float width )
		{
			setWidth( new Value.Fixed( width ) );
			return this;
		}


		/// <summary>
		/// Sets the minHeight, prefHeight, and maxHeight to the specified value.
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="height">Height.</param>
		public Container setHeight( Value height )
		{
			if( height == null )
				throw new Exception( "height cannot be null." );
			_minHeightValue = height;
			_prefHeightValue = height;
			_maxHeightValue = height;
			return this;
		}


		/// <summary>
		/// Sets the minHeight, prefHeight, and maxHeight to the specified value
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="height">Height.</param>
		public new Container setHeight( float height )
		{
			setHeight( new Value.Fixed( height ) );
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified value
		/// </summary>
		/// <returns>The minimum size.</returns>
		/// <param name="size">Size.</param>
		public Container setMinSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			_minWidthValue = size;
			_minHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the minimum size.
		/// </summary>
		/// <returns>The minimum size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container setMinSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			_minWidthValue = width;
			_minHeightValue = height;
			return this;
		}


		public Container setMinWidth( Value minWidth )
		{
			if( minWidth == null )
				throw new Exception( "minWidth cannot be null." );
			this._minWidthValue = minWidth;
			return this;
		}


		public Container setMinHeight( Value minHeight )
		{
			if( minHeight == null )
				throw new Exception( "minHeight cannot be null." );
			this._minHeightValue = minHeight;
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified value
		/// </summary>
		/// <returns>The minimum size.</returns>
		/// <param name="size">Size.</param>
		public Container setMinSize( float size )
		{
			setMinSize( new Value.Fixed( size ) );
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified values
		/// </summary>
		/// <returns>The minimum size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container setMinSize( float width, float height )
		{
			setMinSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		public Container setMinWidth( float minWidth )
		{
			this._minWidthValue = new Value.Fixed( minWidth );
			return this;
		}


		public Container setMinHeight( float minHeight )
		{
			this._minHeightValue = new Value.Fixed( minHeight );
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Container prefSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			_prefWidthValue = size;
			_prefHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container prefSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			_prefWidthValue = width;
			_prefHeightValue = height;
			return this;
		}


		public Container setPrefWidth( Value prefWidth )
		{
			if( prefWidth == null )
				throw new Exception( "prefWidth cannot be null." );
			this._prefWidthValue = prefWidth;
			return this;
		}


		public Container setPrefHeight( Value prefHeight )
		{
			if( prefHeight == null )
				throw new Exception( "prefHeight cannot be null." );
			this._prefHeightValue = prefHeight;
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified value.
		/// </summary>
		/// <returns>The preference size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container setPrefSize( float width, float height )
		{
			prefSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified values
		/// </summary>
		/// <returns>The preference size.</returns>
		/// <param name="size">Size.</param>
		public Container setPrefSize( float size )
		{
			prefSize( new Value.Fixed( size ) );
			return this;
		}


		public Container setPrefWidth( float prefWidth )
		{
			this._prefWidthValue = new Value.Fixed( prefWidth );
			return this;
		}


		public Container setPrefHeight( float prefHeight )
		{
			this._prefHeightValue = new Value.Fixed( prefHeight );
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified value.
		/// </summary>
		/// <returns>The max size.</returns>
		/// <param name="size">Size.</param>
		public Container setMaxSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			_maxWidthValue = size;
			_maxHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified values
		/// </summary>
		/// <returns>The max size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container setMaxSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			_maxWidthValue = width;
			_maxHeightValue = height;
			return this;
		}


		public Container setMaxWidth( Value maxWidth )
		{
			if( maxWidth == null )
				throw new Exception( "maxWidth cannot be null." );
			this._maxWidthValue = maxWidth;
			return this;
		}


		public Container setMaxHeight( Value maxHeight )
		{
			if( maxHeight == null )
				throw new Exception( "maxHeight cannot be null." );
			this._maxHeightValue = maxHeight;
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified value
		/// </summary>
		/// <returns>The max size.</returns>
		/// <param name="size">Size.</param>
		public Container setMaxSize( float size )
		{
			setMaxSize( new Value.Fixed( size ) );
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified values
		/// </summary>
		/// <returns>The max size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container setMaxSize( float width, float height )
		{
			setMaxSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		public Container setMaxWidth( float maxWidth )
		{
			this._maxWidthValue = new Value.Fixed( maxWidth );
			return this;
		}


		public Container setMaxHeight( float maxHeight )
		{
			this._maxHeightValue = new Value.Fixed( maxHeight );
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
		/// </summary>
		/// <returns>The pad.</returns>
		/// <param name="pad">Pad.</param>
		public Container setPad( Value pad )
		{
			if( pad == null )
				throw new Exception( "pad cannot be null." );
			_padTop = pad;
			_padLeft = pad;
			_padBottom = pad;
			_padRight = pad;
			return this;
		}


		public Container setPad( Value top, Value left, Value bottom, Value right )
		{
			if( top == null )
				throw new Exception( "top cannot be null." );
			if( left == null )
				throw new Exception( "left cannot be null." );
			if( bottom == null )
				throw new Exception( "bottom cannot be null." );
			if( right == null )
				throw new Exception( "right cannot be null." );
			_padTop = top;
			_padLeft = left;
			_padBottom = bottom;
			_padRight = right;
			return this;
		}


		public Container setPadTop( Value padTop )
		{
			if( padTop == null )
				throw new Exception( "padTop cannot be null." );
			this._padTop = padTop;
			return this;
		}


		public Container setPadLeft( Value padLeft )
		{
			if( padLeft == null )
				throw new Exception( "padLeft cannot be null." );
			this._padLeft = padLeft;
			return this;
		}


		public Container setPadBottom( Value padBottom )
		{
			if( padBottom == null )
				throw new Exception( "padBottom cannot be null." );
			this._padBottom = padBottom;
			return this;
		}


		public Container setPadRight( Value padRight )
		{
			if( padRight == null )
				throw new Exception( "padRight cannot be null." );
			this._padRight = padRight;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value
		/// </summary>
		/// <returns>The pad.</returns>
		/// <param name="pad">Pad.</param>
		public Container setPad( float pad )
		{
			Value value = new Value.Fixed( pad );
			_padTop = value;
			_padLeft = value;
			_padBottom = value;
			_padRight = value;
			return this;
		}


		public Container setPad( float top, float left, float bottom, float right )
		{
			_padTop = new Value.Fixed( top );
			_padLeft = new Value.Fixed( left );
			_padBottom = new Value.Fixed( bottom );
			_padRight = new Value.Fixed( right );
			return this;
		}


		public Container setPadTop( float padTop )
		{
			this._padTop = new Value.Fixed( padTop );
			return this;
		}


		public Container setPadLeft( float padLeft )
		{
			this._padLeft = new Value.Fixed( padLeft );
			return this;
		}


		public Container setPadBottom( float padBottom )
		{
			this._padBottom = new Value.Fixed( padBottom );
			return this;
		}


		public Container setPadRight( float padRight )
		{
			this._padRight = new Value.Fixed( padRight );
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1
		/// </summary>
		/// <returns>The fill.</returns>
		public Container setFill()
		{
			_fillX = 1f;
			_fillY = 1f;
			return this;
		}


		/// <summary>
		/// Sets fillX to 1
		/// </summary>
		/// <returns>The fill x.</returns>
		public Container setFillX()
		{
			_fillX = 1f;
			return this;
		}


		/// <summary>
		/// Sets fillY to 1
		/// </summary>
		/// <returns>The fill y.</returns>
		public Container setFillY()
		{
			_fillY = 1f;
			return this;
		}


		public Container setFill( float x, float y )
		{
			_fillX = x;
			_fillY = y;
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1 if true, 0 if false
		/// </summary>
		/// <returns>The fill.</returns>
		/// <param name="x">If set to <c>true</c> x.</param>
		/// <param name="y">If set to <c>true</c> y.</param>
		public Container setFill( bool x, bool y )
		{
			_fillX = x ? 1f : 0;
			_fillY = y ? 1f : 0;
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1 if true, 0 if false
		/// </summary>
		/// <returns>The fill.</returns>
		/// <param name="fill">If set to <c>true</c> fill.</param>
		public Container setFill( bool fill )
		{
			_fillX = fill ? 1f : 0;
			_fillY = fill ? 1f : 0;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the element within the container. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom},
		/// {@link Align#left}, {@link Align#right}, or any combination of those.
		/// </summary>
		/// <returns>The align.</returns>
		/// <param name="align">Align.</param>
		public Container setAlign( Align align )
		{
			_align = (int)align;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the element within the container to {@link Align#center}. This clears any other alignment.
		/// </summary>
		/// <returns>The center.</returns>
		public Container setAlignCenter()
		{
			_align = AlignInternal.center;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#top} and clears {@link Align#bottom} for the alignment of the element within the container.
		/// </summary>
		/// <returns>The top.</returns>
		public Container setTop()
		{
			_align |= AlignInternal.top;
			_align &= ~AlignInternal.bottom;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#left} and clears {@link Align#right} for the alignment of the element within the container.
		/// </summary>
		/// <returns>The left.</returns>
		public Container setLeft()
		{
			_align |= AlignInternal.left;
			_align &= ~AlignInternal.right;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#bottom} and clears {@link Align#top} for the alignment of the element within the container.
		/// </summary>
		/// <returns>The bottom.</returns>
		public Container setBottom()
		{
			_align |= AlignInternal.bottom;
			_align &= ~AlignInternal.top;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#right} and clears {@link Align#left} for the alignment of the element within the container.
		/// </summary>
		/// <returns>The right.</returns>
		public Container setRight()
		{
			_align |= AlignInternal.right;
			_align &= ~AlignInternal.left;
			return this;
		}


		public float getMinWidth()
		{
			return _minWidthValue.get( _element ) + _padLeft.get( this ) + _padRight.get( this );
		}


		public Value getMinHeightValue()
		{
			return _minHeightValue;
		}


		public float getMinHeight()
		{
			return _minHeightValue.get( _element ) + _padTop.get( this ) + _padBottom.get( this );
		}


		public Value getPrefWidthValue()
		{
			return _prefWidthValue;
		}


		public float getPrefWidth()
		{
			float v = _prefWidthValue.get( _element );
			if( _background != null )
				v = Math.Max( v, _background.minWidth );
			return Math.Max( getMinWidth(), v + _padLeft.get( this ) + _padRight.get( this ) );
		}


		public Value getPrefHeightValue()
		{
			return _prefHeightValue;
		}


		public float getPrefHeight()
		{
			float v = _prefHeightValue.get( _element );
			if( _background != null )
				v = Math.Max( v, _background.minHeight );
			return Math.Max( getMinHeight(), v + _padTop.get( this ) + _padBottom.get( this ) );
		}


		public Value getMaxWidthValue()
		{
			return _maxWidthValue;
		}


		public float getMaxWidth()
		{
			float v = _maxWidthValue.get( _element );
			if( v > 0 )
				v += _padLeft.get( this ) + _padRight.get( this );
			return v;
		}


		public Value getMaxHeightValue()
		{
			return _maxHeightValue;
		}


		public float getMaxHeight()
		{
			float v = _maxHeightValue.get( _element );
			if( v > 0 )
				v += _padTop.get( this ) + _padBottom.get( this );
			return v;
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad top value.</returns>
		public Value getPadTopValue()
		{
			return _padTop;
		}


		public float getPadTop()
		{
			return _padTop.get( this );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad left value.</returns>
		public Value getPadLeftValue()
		{
			return _padLeft;
		}


		public float getPadLeft()
		{
			return _padLeft.get( this );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad bottom value.</returns>
		public Value getPadBottomValue()
		{
			return _padBottom;
		}


		public float getPadBottom()
		{
			return _padBottom.get( this );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad right value.</returns>
		public Value getPadRightValue()
		{
			return _padRight;
		}


		public float getPadRight()
		{
			return _padRight.get( this );
		}


		/// <summary>
		/// Returns {@link #getPadLeft()} plus {@link #getPadRight()}.
		/// </summary>
		/// <returns>The pad x.</returns>
		public float getPadX()
		{
			return _padLeft.get( this ) + _padRight.get( this );
		}


		/// <summary>
		/// Returns {@link #getPadTop()} plus {@link #getPadBottom()}
		/// </summary>
		/// <returns>The pad y.</returns>
		public float getPadY()
		{
			return _padTop.get( this ) + _padBottom.get( this );
		}


		public float getFillX()
		{
			return _fillX;
		}


		public float getFillY()
		{
			return _fillY;
		}


		public int getAlign()
		{
			return _align;
		}


		/// <summary>
		/// If true (the default), positions and sizes are rounded to integers
		/// </summary>
		/// <param name="round">If set to <c>true</c> round.</param>
		public void setRound( bool round )
		{
			this._round = round;
		}


		/// <summary>
		/// Causes the contents to be clipped if they exceed the container bounds. Enabling clipping will set
		/// {@link #setTransform(bool)} to true
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		public void setClip( bool enabled )
		{
			_clip = enabled;
			transform = enabled;
			invalidate();
		}


		public bool getClip()
		{
			return _clip;
		}


		public override Element hit( Vector2 point )
		{
			if( _clip )
			{
				if( getTouchable() == Touchable.Disabled )
					return null;
				if( point.X < 0 || point.X >= getWidth() || point.Y < 0 || point.Y >= getHeight() )
					return null;
			}
			return base.hit( point );
		}


		public override void debugRender( Graphics graphics )
		{
			validate();
			if( transform )
			{
				applyTransform( graphics, computeTransform() );
				if( _clip )
				{
					//shapes.flush();
					//float padLeft = this.padLeft.get( this ), padBottom = this.padBottom.get( this );
					//bool draw = background == null ? clipBegin( 0, 0, getWidth(), getHeight() ) : clipBegin( padLeft, padBottom,
					//	            getWidth() - padLeft - padRight.get( this ), getHeight() - padBottom - padTop.get( this ) );
					var draw = true;
					if( draw )
					{
						debugRenderChildren( graphics, 1f );
						//clipEnd();
					}
				}
				else
				{
					debugRenderChildren( graphics, 1f );
				}
				resetTransform( graphics );
			}
			else
			{
				base.debugRender( graphics );
			}
		}

	}
}

