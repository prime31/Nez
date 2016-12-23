using System;


namespace Nez.UI
{
	public class Cell : IPoolable
	{
		static private int centeri = AlignInternal.center, topi = AlignInternal.top, bottomi = AlignInternal.bottom, lefti = AlignInternal.left,
			righti = AlignInternal.right;

		static private bool files;
		static private Cell defaults;

		internal Value minWidth, minHeight;
		internal Value prefWidth, prefHeight;
		internal Value maxWidth, maxHeight;
		internal Value spaceTop, spaceLeft, spaceBottom, spaceRight;
		internal Value padTop, padLeft, padBottom, padRight;
		internal float? fillX, fillY;
		internal int? align;
		internal int? expandX, expandY;
		internal int? colspan;
		internal bool? uniformX, uniformY;

		internal Element element;
		internal float elementX, elementY;
		internal float elementWidth, elementHeight;

		private Table table;
		internal bool endRow;
		internal int column, row;
		internal int cellAboveIndex;
		internal float computedPadTop, computedPadLeft, computedPadBottom, computedPadRight;


		public Cell()
		{
			reset();
		}


		internal void setLayout( Table table )
		{
			this.table = table;
		}


		/// <summary>
		/// Returns the element for this cell casted to T, or null.
		/// </summary>
		/// <returns>The element.</returns>
		public T getElement<T>() where T : Element
		{
			return element as T;
		}


		/// <summary>
		/// Returns true if the cell's element is not null.
		/// </summary>
		/// <returns><c>true</c>, if element was hased, <c>false</c> otherwise.</returns>
		public bool hasElement()
		{
			return element != null;
		}


		#region Chainable configuration

		/// <summary>
		/// Sets the element in this cell and adds the element to the cell's table. If null, removes any current element.
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="newelement">New element.</param>
		public Cell setElement( Element newElement )
		{
			if( element != newElement )
			{
				if( element != null )
					element.remove();
				element = newElement;
				if( newElement != null )
					table.addElement( newElement );
			}
			return this;
		}


		/// <summary>
		/// Removes the current element for the cell, if any.
		/// </summary>
		/// <returns>The element.</returns>
		public Cell clearElement()
		{
			setElement( null );
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value.
		/// </summary>
		/// <param name="size">Size.</param>
		public Cell size( Value size )
		{
			Assert.isNotNull( size, "size cannot be null." );
			
			minWidth = size;
			minHeight = size;
			prefWidth = size;
			prefHeight = size;
			maxWidth = size;
			maxHeight = size;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell size( Value width, Value height )
		{
			Assert.isNotNull( width, "width cannot be null." );
			Assert.isNotNull( height, "height cannot be null." );

			minWidth = width;
			minHeight = height;
			prefWidth = width;
			prefHeight = height;
			maxWidth = width;
			maxHeight = height;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value.
		/// </summary>
		/// <param name="size">Size.</param>
		public Cell size( float size )
		{
			return this.size( new Value.Fixed( size ) );
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell size( float width, float height )
		{
			return size( new Value.Fixed( width ), new Value.Fixed( height ) );
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, and maxWidth to the specified value.
		/// </summary>
		/// <param name="width">Width.</param>
		public Cell width( Value width )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			minWidth = width;
			prefWidth = width;
			maxWidth = width;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, and maxWidth to the specified value.
		/// </summary>
		/// <param name="width">Width.</param>
		public Cell width( float width )
		{
			return this.width( new Value.Fixed( width ) );
		}


		/// <summary>
		/// Sets the minHeight, prefHeight, and maxHeight to the specified value.
		/// </summary>
		/// <param name="height">Height.</param>
		public Cell height( Value height )
		{
			if( height == null )
				throw new Exception( "height cannot be null." );
			minHeight = height;
			prefHeight = height;
			maxHeight = height;

			return this;
		}


		/// <summary>
		/// Sets the minHeight, prefHeight, and maxHeight to the specified value.
		/// </summary>
		/// <param name="height">Height.</param>
		public Cell height( float height )
		{
			return this.height( new Value.Fixed( height ) );
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Cell minSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			minWidth = size;
			minHeight = size;
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell minSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			minWidth = width;
			minHeight = height;
			return this;
		}


		public Cell setMinWidth( Value minWidth )
		{
			if( minWidth == null )
				throw new Exception( "minWidth cannot be null." );
			this.minWidth = minWidth;
			return this;
		}


		public Cell setMinHeight( Value minHeight )
		{
			if( minHeight == null )
				throw new Exception( "minHeight cannot be null." );
			this.minHeight = minHeight;
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Cell minSize( float size )
		{
			minSize( new Value.Fixed( size ) );
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell minSize( float width, float height )
		{
			minSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		public Cell setMinWidth( float minWidth )
		{
			this.minWidth = new Value.Fixed( minWidth );
			return this;
		}


		public Cell setMinHeight( float minHeight )
		{
			this.minHeight = new Value.Fixed( minHeight );
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Cell prefSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			prefWidth = size;
			prefHeight = size;
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell prefSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			prefWidth = width;
			prefHeight = height;
			return this;
		}


		public Cell setPrefWidth( Value prefWidth )
		{
			if( prefWidth == null )
				throw new Exception( "prefWidth cannot be null." );
			this.prefWidth = prefWidth;
			return this;
		}


		public Cell setPrefHeight( Value prefHeight )
		{
			if( prefHeight == null )
				throw new Exception( "prefHeight cannot be null." );
			this.prefHeight = prefHeight;
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell prefSize( float width, float height )
		{
			prefSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Cell prefSize( float size )
		{
			prefSize( new Value.Fixed( size ) );
			return this;
		}


		public Cell setPrefWidth( float prefWidth )
		{
			this.prefWidth = new Value.Fixed( prefWidth );
			return this;
		}


		public Cell setPrefHeight( float prefHeight )
		{
			this.prefHeight = new Value.Fixed( prefHeight );
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Cell maxSize( Value size )
		{
			if( size == null )
				throw new Exception( "size cannot be null." );
			maxWidth = size;
			maxHeight = size;
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell maxSize( Value width, Value height )
		{
			if( width == null )
				throw new Exception( "width cannot be null." );
			if( height == null )
				throw new Exception( "height cannot be null." );
			maxWidth = width;
			maxHeight = height;
			return this;
		}


		public Cell setMaxWidth( Value maxWidth )
		{
			if( maxWidth == null )
				throw new Exception( "maxWidth cannot be null." );
			this.maxWidth = maxWidth;
			return this;
		}


		public Cell setMaxHeight( Value maxHeight )
		{
			if( maxHeight == null )
				throw new Exception( "maxHeight cannot be null." );
			this.maxHeight = maxHeight;
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified value.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="size">Size.</param>
		public Cell maxSize( float size )
		{
			maxSize( new Value.Fixed( size ) );
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified values.
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Cell maxSize( float width, float height )
		{
			maxSize( new Value.Fixed( width ), new Value.Fixed( height ) );
			return this;
		}


		public Cell setMaxWidth( float maxWidth )
		{
			this.maxWidth = new Value.Fixed( maxWidth );
			return this;
		}


		public Cell setMaxHeight( float maxHeight )
		{
			this.maxHeight = new Value.Fixed( maxHeight );
			return this;
		}


		/// <summary>
		/// Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value.
		/// </summary>
		/// <param name="space">Space.</param>
		public Cell space( Value space )
		{
			if( space == null )
				throw new Exception( "space cannot be null." );
			spaceTop = space;
			spaceLeft = space;
			spaceBottom = space;
			spaceRight = space;
			return this;
		}


		public Cell space( Value top, Value left, Value bottom, Value right )
		{
			if( top == null )
				throw new Exception( "top cannot be null." );
			if( left == null )
				throw new Exception( "left cannot be null." );
			if( bottom == null )
				throw new Exception( "bottom cannot be null." );
			if( right == null )
				throw new Exception( "right cannot be null." );
			spaceTop = top;
			spaceLeft = left;
			spaceBottom = bottom;
			spaceRight = right;
			return this;
		}


		public Cell setSpaceTop( Value spaceTop )
		{
			if( spaceTop == null )
				throw new Exception( "spaceTop cannot be null." );
			this.spaceTop = spaceTop;
			return this;
		}


		public Cell setSpaceLeft( Value spaceLeft )
		{
			if( spaceLeft == null )
				throw new Exception( "spaceLeft cannot be null." );
			this.spaceLeft = spaceLeft;
			return this;
		}


		public Cell setSpaceBottom( Value spaceBottom )
		{
			if( spaceBottom == null )
				throw new Exception( "spaceBottom cannot be null." );
			this.spaceBottom = spaceBottom;
			return this;
		}


		public Cell setSpaceRight( Value spaceRight )
		{
			if( spaceRight == null )
				throw new Exception( "spaceRight cannot be null." );
			this.spaceRight = spaceRight;
			return this;
		}


		/// <summary>
		/// Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value.
		/// </summary>
		/// <param name="space">Space.</param>
		public Cell space( float space )
		{
			if( space < 0 )
				throw new Exception( "space cannot be < 0." );
			return this.space( new Value.Fixed( space ) );
		}


		public Cell space( float top, float left, float bottom, float right )
		{
			if( top < 0 )
				throw new Exception( "top cannot be < 0." );
			if( left < 0 )
				throw new Exception( "left cannot be < 0." );
			if( bottom < 0 )
				throw new Exception( "bottom cannot be < 0." );
			if( right < 0 )
				throw new Exception( "right cannot be < 0." );
			space( new Value.Fixed( top ), new Value.Fixed( left ), new Value.Fixed( bottom ), new Value.Fixed( right ) );
			return this;
		}


		public Cell setSpaceTop( float spaceTop )
		{
			if( spaceTop < 0 )
				throw new Exception( "spaceTop cannot be < 0." );
			this.spaceTop = new Value.Fixed( spaceTop );
			return this;
		}


		public Cell setSpaceLeft( float spaceLeft )
		{
			if( spaceLeft < 0 )
				throw new Exception( "spaceLeft cannot be < 0." );
			this.spaceLeft = new Value.Fixed( spaceLeft );
			return this;
		}


		public Cell setSpaceBottom( float spaceBottom )
		{
			if( spaceBottom < 0 )
				throw new Exception( "spaceBottom cannot be < 0." );
			this.spaceBottom = new Value.Fixed( spaceBottom );
			return this;
		}


		public Cell setSpaceRight( float spaceRight )
		{
			if( spaceRight < 0 )
				throw new Exception( "spaceRight cannot be < 0." );
			this.spaceRight = new Value.Fixed( spaceRight );
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
		/// </summary>
		/// <param name="pad">Pad.</param>
		public Cell pad( Value pad )
		{
			if( pad == null )
				throw new Exception( "pad cannot be null." );
			padTop = pad;
			padLeft = pad;
			padBottom = pad;
			padRight = pad;
			return this;
		}


		public Cell pad( Value top, Value left, Value bottom, Value right )
		{
			if( top == null )
				throw new Exception( "top cannot be null." );
			if( left == null )
				throw new Exception( "left cannot be null." );
			if( bottom == null )
				throw new Exception( "bottom cannot be null." );
			if( right == null )
				throw new Exception( "right cannot be null." );
			padTop = top;
			padLeft = left;
			padBottom = bottom;
			padRight = right;
			return this;
		}


		public Cell setPadTop( Value padTop )
		{
			if( padTop == null )
				throw new Exception( "padTop cannot be null." );
			this.padTop = padTop;
			return this;
		}


		public Cell setPadLeft( Value padLeft )
		{
			if( padLeft == null )
				throw new Exception( "padLeft cannot be null." );
			this.padLeft = padLeft;
			return this;
		}


		public Cell setPadBottom( Value padBottom )
		{
			if( padBottom == null )
				throw new Exception( "padBottom cannot be null." );
			this.padBottom = padBottom;
			return this;
		}


		public Cell setPadRight( Value padRight )
		{
			if( padRight == null )
				throw new Exception( "padRight cannot be null." );
			this.padRight = padRight;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
		/// </summary>
		/// <param name="pad">Pad.</param>
		public Cell pad( float pad )
		{
			return this.pad( new Value.Fixed( pad ) );
		}


		public Cell pad( float top, float left, float bottom, float right )
		{
			pad( new Value.Fixed( top ), new Value.Fixed( left ), new Value.Fixed( bottom ), new Value.Fixed( right ) );
			return this;
		}


		public Cell setPadTop( float padTop )
		{
			this.padTop = new Value.Fixed( padTop );
			return this;
		}


		public Cell setPadLeft( float padLeft )
		{
			this.padLeft = new Value.Fixed( padLeft );
			return this;
		}


		public Cell setPadBottom( float padBottom )
		{
			this.padBottom = new Value.Fixed( padBottom );
			return this;
		}


		public Cell setPadRight( float padRight )
		{
			this.padRight = new Value.Fixed( padRight );
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1
		/// </summary>
		public Cell fill()
		{
			fillX = 1f;
			fillY = 1f;
			return this;
		}


		/// <summary>
		/// Sets fillX to 1
		/// </summary>
		/// <returns>The fill x.</returns>
		public Cell setFillX()
		{
			fillX = 1f;
			return this;
		}


		/// <summary>
		/// Sets fillY to 1
		/// </summary>
		/// <returns>The fill y.</returns>
		public Cell setFillY()
		{
			fillY = 1f;
			return this;
		}


		public Cell fill( float x, float y )
		{
			fillX = x;
			fillY = y;
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1 if true, 0 if false.
		/// </summary>
		/// <param name="x">If set to <c>true</c> x.</param>
		/// <param name="y">If set to <c>true</c> y.</param>
		public Cell fill( bool x, bool y )
		{
			fillX = x ? 1f : 0f;
			fillY = y ? 1f : 0f;
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1 if true, 0 if false.
		/// </summary>
		/// <param name="fill">If set to <c>true</c> fill.</param>
		public Cell fill( bool fill )
		{
			fillX = fill ? 1f : 0f;
			fillY = fill ? 1f : 0f;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the element within the cell. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom},
		/// {@link Align#left}, {@link Align#right}, or any combination of those.
		/// </summary>
		/// <returns>The align.</returns>
		/// <param name="align">Align.</param>
		public Cell setAlign( Align align )
		{
			this.align = (int)align;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the element within the cell to {@link Align#center}. This clears any other alignment.
		/// </summary>
		public Cell center()
		{
			align = centeri;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#top} and clears {@link Align#bottom} for the alignment of the element within the cell.
		/// </summary>
		public Cell top()
		{
			if( align == null )
				align = topi;
			else
				align = ( align | AlignInternal.top ) & ~AlignInternal.bottom;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#left} and clears {@link Align#right} for the alignment of the element within the cell
		/// </summary>
		public Cell left()
		{
			if( align == null )
				align = lefti;
			else
				align = ( align | AlignInternal.left ) & ~AlignInternal.right;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#bottom} and clears {@link Align#top} for the alignment of the element within the cell
		/// </summary>
		public Cell bottom()
		{
			if( align == null )
				align = bottomi;
			else
				align = ( align | AlignInternal.bottom ) & ~AlignInternal.top;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#right} and clears {@link Align#left} for the alignment of the element within the cell
		/// </summary>
		public Cell right()
		{
			if( align == null )
				align = righti;
			else
				align = ( align | AlignInternal.right ) & ~AlignInternal.left;
			return this;
		}


		/// <summary>
		/// Sets expandX, expandY, fillX, and fillY to 1
		/// </summary>
		public Cell grow()
		{
			expandX = 1;
			expandY = 1;
			fillX = 1f;
			fillY = 1f;
			return this;
		}


		/// <summary>
		/// Sets expandX and fillX to 1
		/// </summary>
		/// <returns>The x.</returns>
		public Cell growX()
		{
			expandX = 1;
			fillX = 1f;
			return this;
		}


		/// <summary>
		/// Sets expandY and fillY to 1
		/// </summary>
		/// <returns>The y.</returns>
		public Cell growY()
		{
			expandY = 1;
			fillY = 1f;
			return this;
		}


		/// <summary>
		/// Sets expandX and expandY to 1
		/// </summary>
		public Cell expand()
		{
			expandX = 1;
			expandY = 1;
			return this;
		}


		/// <summary>
		/// Sets expandX to 1
		/// </summary>
		/// <returns>The expand x.</returns>
		public Cell setExpandX()
		{
			expandX = 1;
			return this;
		}


		/// <summary>
		/// Sets expandY to 1
		/// </summary>
		/// <returns>The expand y.</returns>
		public Cell setExpandY()
		{
			expandY = 1;
			return this;
		}


		public Cell expand( int x, int y )
		{
			expandX = x;
			expandY = y;
			return this;
		}


		/// <summary>
		/// Sets expandX and expandY to 1 if true, 0 if false
		/// </summary>
		/// <param name="x">If set to <c>true</c> x.</param>
		/// <param name="y">If set to <c>true</c> y.</param>
		public Cell expand( bool x, bool y )
		{
			expandX = x ? 1 : 0;
			expandY = y ? 1 : 0;
			return this;
		}


		public Cell setColspan( int colspan )
		{
			this.colspan = colspan;
			return this;
		}


		/// <summary>
		/// Sets uniformX and uniformY to true
		/// </summary>
		public Cell uniform()
		{
			uniformX = true;
			uniformY = true;
			return this;
		}


		/// <summary>
		/// Sets uniformX to true
		/// </summary>
		/// <returns>The uniform x.</returns>
		public Cell setUniformX()
		{
			uniformX = true;
			return this;
		}


		/// <summary>
		/// Sets uniformY to true
		/// </summary>
		/// <returns>The uniform y.</returns>
		public Cell setUniformY()
		{
			uniformY = true;
			return this;
		}


		public Cell uniform( bool x, bool y )
		{
			uniformX = x;
			uniformY = y;
			return this;
		}

		#endregion


		public void setElementBounds( float x, float y, float width, float height )
		{
			elementX = x;
			elementY = y;
			elementWidth = width;
			elementHeight = height;
		}


		public float getElementX()
		{
			return elementX;
		}


		public void setElementX( float elementX )
		{
			this.elementX = elementX;
		}


		public float getElementY()
		{
			return elementY;
		}


		public void setElementY( float elementY )
		{
			this.elementY = elementY;
		}


		public float getElementWidth()
		{
			return elementWidth;
		}


		public void setElementWidth( float elementWidth )
		{
			this.elementWidth = elementWidth;
		}


		public float getElementHeight()
		{
			return elementHeight;
		}


		public void setElementHeight( float elementHeight )
		{
			this.elementHeight = elementHeight;
		}


		public int getColumn()
		{
			return column;
		}


		public int getRow()
		{
			return row;
		}


		/// <summary>
		/// May be null if this cell is row defaults.
		/// </summary>
		/// <returns>The minimum width value.</returns>
		public Value getMinWidthValue()
		{
			return minWidth;
		}


		public float getMinWidth()
		{
			return minWidth.get( element );
		}


		/// <summary>
		/// May be null if this cell is row defaults
		/// </summary>
		/// <returns>The minimum height value.</returns>
		public Value getMinHeightValue()
		{
			return minHeight;
		}


		public float getMinHeight()
		{
			return minHeight.get( element );
		}


		/// <summary>
		/// May be null if this cell is row defaults.
		/// </summary>
		/// <returns>The preference width value.</returns>
		public Value getPrefWidthValue()
		{
			return prefWidth;
		}


		public float getPrefWidth()
		{
			return prefWidth.get( element );
		}


		/// <summary>
		/// May be null if this cell is row defaults.
		/// </summary>
		/// <returns>The preference height value.</returns>
		public Value getPrefHeightValue()
		{
			return prefHeight;
		}


		public float getPrefHeight()
		{
			return prefHeight.get( element );
		}


		/// <summary>
		/// May be null if this cell is row defaults
		/// </summary>
		/// <returns>The max width value.</returns>
		public Value getMaxWidthValue()
		{
			return maxWidth;
		}


		public float getMaxWidth()
		{
			return maxWidth.get( element );
		}


		/// <summary>
		/// May be null if this cell is row defaults
		/// </summary>
		/// <returns>The max height value.</returns>
		public Value getMaxHeightValue()
		{
			return maxHeight;
		}


		public float getMaxHeight()
		{
			return maxHeight.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The space top value.</returns>
		public Value getSpaceTopValue()
		{
			return spaceTop;
		}


		public float getSpaceTop()
		{
			return spaceTop.get( element );
		}


		/// <summary>
		/// May be null if this value is not set.
		/// </summary>
		/// <returns>The space left value.</returns>
		public Value getSpaceLeftValue()
		{
			return spaceLeft;
		}


		public float getSpaceLeft()
		{
			return spaceLeft.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The space bottom value.</returns>
		public Value getSpaceBottomValue()
		{
			return spaceBottom;
		}


		public float getSpaceBottom()
		{
			return spaceBottom.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The space right value.</returns>
		public Value getSpaceRightValue()
		{
			return spaceRight;
		}


		public float getSpaceRight()
		{
			return spaceRight.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad top value.</returns>
		public Value getPadTopValue()
		{
			return padTop;
		}


		public float getPadTop()
		{
			return padTop.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad left value.</returns>
		public Value getPadLeftValue()
		{
			return padLeft;
		}


		public float getPadLeft()
		{
			return padLeft.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad bottom value.</returns>
		public Value getPadBottomValue()
		{
			return padBottom;
		}


		public float getPadBottom()
		{
			return padBottom.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad right value.</returns>
		public Value getPadRightValue()
		{
			return padRight;
		}


		public float getPadRight()
		{
			return padRight.get( element );
		}


		/// <summary>
		/// Returns {@link #getPadLeft()} plus {@link #getPadRight()}
		/// </summary>
		/// <returns>The pad x.</returns>
		public float getPadX()
		{
			return padLeft.get( element ) + padRight.get( element );
		}


		/// <summary>
		/// Returns {@link #getPadTop()} plus {@link #getPadBottom()}
		/// </summary>
		/// <returns>The pad y.</returns>
		public float getPadY()
		{
			return padTop.get( element ) + padBottom.get( element );
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The fill x.</returns>
		public float? getFillX()
		{
			return fillX;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The fill y.</returns>
		public float? getFillY()
		{
			return fillY;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The align.</returns>
		public int? getAlign()
		{
			return align;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The expand x.</returns>
		public int? getExpandX()
		{
			return expandX;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The expand y.</returns>
		public int? getExpandY()
		{
			return expandY;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The colspan.</returns>
		public int? getColspan()
		{
			return colspan;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The uniform x.</returns>
		public bool? getUniformX()
		{
			return uniformX;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The uniform y.</returns>
		public bool? getUniformY()
		{
			return uniformY;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns><c>true</c>, if end row was ised, <c>false</c> otherwise.</returns>
		public bool isEndRow()
		{
			return endRow;
		}


		/// <summary>
		/// The actual amount of combined padding and spacing from the last layout.
		/// </summary>
		/// <returns>The computed pad top.</returns>
		public float getComputedPadTop()
		{
			return computedPadTop;
		}


		/// <summary>
		/// The actual amount of combined padding and spacing from the last layout.
		/// </summary>
		/// <returns>The computed pad left.</returns>
		public float getComputedPadLeft()
		{
			return computedPadLeft;
		}


		/// <summary>
		/// The actual amount of combined padding and spacing from the last layout
		/// </summary>
		/// <returns>The computed pad bottom.</returns>
		public float getComputedPadBottom()
		{
			return computedPadBottom;
		}


		/// <summary>
		/// The actual amount of combined padding and spacing from the last layout
		/// </summary>
		/// <returns>The computed pad right.</returns>
		public float getComputedPadRight()
		{
			return computedPadRight;
		}


		public void setRow()
		{
			table.row();
		}


		public Table getTable()
		{
			return table;
		}


		/// <summary>
		/// Returns the defaults to use for all cells. This can be used to avoid needing to set the same defaults for every table (eg,
		/// for spacing).
		/// </summary>
		/// <returns>The defaults.</returns>
		static public Cell getDefaults()
		{
			if( !files )
			{
				files = true;
				defaults = new Cell();
				defaults.minWidth = Value.minWidth;
				defaults.minHeight = Value.minHeight;
				defaults.prefWidth = Value.prefWidth;
				defaults.prefHeight = Value.prefHeight;
				defaults.maxWidth = Value.maxWidth;
				defaults.maxHeight = Value.maxHeight;
				defaults.spaceTop = Value.zero;
				defaults.spaceLeft = Value.zero;
				defaults.spaceBottom = Value.zero;
				defaults.spaceRight = Value.zero;
				defaults.padTop = Value.zero;
				defaults.padLeft = Value.zero;
				defaults.padBottom = Value.zero;
				defaults.padRight = Value.zero;
				defaults.fillX = 0f;
				defaults.fillY = 0f;
				defaults.align = centeri;
				defaults.expandX = 0;
				defaults.expandY = 0;
				defaults.colspan = 1;
				defaults.uniformX = null;
				defaults.uniformY = null;
			}
			return defaults;
		}


		/// <summary>
		/// Sets all constraint fields to null
		/// </summary>
		public void clear()
		{
			minWidth = null;
			minHeight = null;
			prefWidth = null;
			prefHeight = null;
			maxWidth = null;
			maxHeight = null;
			spaceTop = null;
			spaceLeft = null;
			spaceBottom = null;
			spaceRight = null;
			padTop = null;
			padLeft = null;
			padBottom = null;
			padRight = null;
			fillX = null;
			fillY = null;
			align = null;
			expandX = null;
			expandY = null;
			colspan = null;
			uniformX = null;
			uniformY = null;
		}


		/// <summary>
		/// Reset state so the cell can be reused, setting all constraints to their {@link #defaults() default} values.
		/// </summary>
		public void reset()
		{
			element = null;
			table = null;
			endRow = false;
			cellAboveIndex = -1;

			var defaults = getDefaults();
			if( defaults != null )
				set( defaults );
		}


		public void set( Cell cell )
		{
			minWidth = cell.minWidth;
			minHeight = cell.minHeight;
			prefWidth = cell.prefWidth;
			prefHeight = cell.prefHeight;
			maxWidth = cell.maxWidth;
			maxHeight = cell.maxHeight;
			spaceTop = cell.spaceTop;
			spaceLeft = cell.spaceLeft;
			spaceBottom = cell.spaceBottom;
			spaceRight = cell.spaceRight;
			padTop = cell.padTop;
			padLeft = cell.padLeft;
			padBottom = cell.padBottom;
			padRight = cell.padRight;
			fillX = cell.fillX;
			fillY = cell.fillY;
			align = cell.align;
			expandX = cell.expandX;
			expandY = cell.expandY;
			colspan = cell.colspan;
			uniformX = cell.uniformX;
			uniformY = cell.uniformY;
		}


		/// <summary>
		/// cell may be null
		/// </summary>
		/// <param name="cell">Cell.</param>
		public void merge( Cell cell )
		{
			if( cell == null )
				return;
			
			if( cell.minWidth != null )
				minWidth = cell.minWidth;
			if( cell.minHeight != null )
				minHeight = cell.minHeight;
			if( cell.prefWidth != null )
				prefWidth = cell.prefWidth;
			if( cell.prefHeight != null )
				prefHeight = cell.prefHeight;
			if( cell.maxWidth != null )
				maxWidth = cell.maxWidth;
			if( cell.maxHeight != null )
				maxHeight = cell.maxHeight;
			if( cell.spaceTop != null )
				spaceTop = cell.spaceTop;
			if( cell.spaceLeft != null )
				spaceLeft = cell.spaceLeft;
			if( cell.spaceBottom != null )
				spaceBottom = cell.spaceBottom;
			if( cell.spaceRight != null )
				spaceRight = cell.spaceRight;
			if( cell.padTop != null )
				padTop = cell.padTop;
			if( cell.padLeft != null )
				padLeft = cell.padLeft;
			if( cell.padBottom != null )
				padBottom = cell.padBottom;
			if( cell.padRight != null )
				padRight = cell.padRight;
			if( cell.fillX != null )
				fillX = cell.fillX;
			if( cell.fillY != null )
				fillY = cell.fillY;
			if( cell.align != null )
				align = cell.align;
			if( cell.expandX != null )
				expandX = cell.expandX;
			if( cell.expandY != null )
				expandY = cell.expandY;
			if( cell.colspan != null )
				colspan = cell.colspan;
			if( cell.uniformX != null )
				uniformX = cell.uniformX;
			if( cell.uniformY != null )
				uniformY = cell.uniformY;
		}


		public override string ToString()
		{
			return element != null ? element.ToString() : base.ToString();
		}

	}
}

