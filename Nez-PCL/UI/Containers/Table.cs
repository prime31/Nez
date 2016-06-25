using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;


namespace Nez.UI
{
	/// <summary>
	/// A group that sizes and positions children using table constraints. By default, {@link #getTouchable()} is
	/// {@link Touchable#childrenOnly}.
	/// 
	/// The preferred and minimum sizes are that of the chdebugn when laid out in columns and rows.
	/// </summary>
	public class Table : Group
	{
		public enum TableDebug
		{
			None,
			All,
			Table,
			Cell,
			Element
		}

		static public Color debugTableColor = new Color( 0, 0, 255, 255 );
		static public Color debugCellColor = new Color( 255, 0, 0, 255 );
		static public Color debugElementColor = new Color( 0, 255, 0, 255 );
		static private float[] _columnWeightedWidth, _rowWeightedHeight;

		public override float minWidth
		{
			get
			{
				if( _sizeInvalid )
					computeSize();
				return _tableMinWidth;	
			}
		}

		public override float minHeight
		{
			get
			{
				if( _sizeInvalid )
					computeSize();
				return _tableMinHeight;
			}
		}

		public override float preferredWidth
		{
			get
			{
				if( _sizeInvalid )
					computeSize();
				var width = _tablePrefWidth;
				if( _background != null )
					return Math.Max( width, _background.minWidth );
				return width;		
			}
		}

		public override float preferredHeight
		{
			get
			{
				if( _sizeInvalid )
					computeSize();
				var height = _tablePrefHeight;
				if( _background != null )
					return Math.Max( height, _background.minHeight );
				return height;
			}
		}


		int _columns, _rows;
		bool _implicitEndRow;

		List<Cell> _cells = new List<Cell>( 4 );
		Cell _cellDefaults;
		List<Cell> _columnDefaults = new List<Cell>( 2 );
		Cell _rowDefaults;

		bool _sizeInvalid = true;
		float[] _columnMinWidth, _rowMinHeight;
		float[] _columnPrefWidth, _rowPrefHeight;
		float _tableMinWidth, _tableMinHeight;
		float _tablePrefWidth, _tablePrefHeight;
		float[] _columnWidth, _rowHeight;
		float[] _expandWidth, _expandHeight;

		Value _padTop = backgroundTop, _padLeft = backgroundLeft, _padBottom = backgroundBottom, _padRight = backgroundRight;
		int _align = AlignInternal.center;

		TableDebug _tableDebug = TableDebug.None;
		List<DebugRectangleF> _debugRects;

		protected IDrawable _background;
		public bool clip;
		bool _round = true;


		public Table()
		{
			_cellDefaults = obtainCell();

			transform = false;
			touchable = Touchable.ChildrenOnly;
		}


		Cell obtainCell()
		{
			var cell = Pool<Cell>.obtain();
			cell.setLayout( this );
			return cell;
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			validate();
			if( transform )
			{
				applyTransform( graphics, computeTransform() );
				drawBackground( graphics, parentAlpha, 0, 0 );
				// TODO: clipping support
//				if( clip )
//				{
//					graphics.flush();
//					float padLeft = this.padLeft.get( this ), padBottom = this.padBottom.get( this );
//					if( clipBegin( padLeft, padBottom, getWidth() - padLeft - padRight.get( this ),
//						    getHeight() - padBottom - padTop.get( this ) ) )
//					{
//						drawChildren( graphics, parentAlpha );
//						graphics.flush();
//						clipEnd();
//					}
//				}
//				else
//				{
					drawChildren( graphics, parentAlpha );
//				}
				resetTransform( graphics );
			}
			else
			{
				drawBackground( graphics, parentAlpha, x, y );
				base.draw( graphics, parentAlpha );
			}
		}


		/// <summary>
		/// Called to draw the background, before clipping is applied (if enabled). Default implementation draws the background
		/// drawable.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="parentAlpha">Parent alpha.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		protected virtual void drawBackground( Graphics graphics, float parentAlpha, float x, float y )
		{
			if( _background == null )
				return;
			
			_background.draw( graphics, x, y, width, height, new Color( color, color.A * parentAlpha ) );
		}


		public IDrawable getBackground()
		{
			return _background;
		}


		public override Element hit( Vector2 point )
		{
			if( clip )
			{
				if( touchable == Touchable.Disabled )
					return null;
				if( point.X < 0 || point.X >= width || point.Y < 0 || point.Y >= height )
					return null;
			}

			return base.hit( point );
		}


		public override void invalidate()
		{
			_sizeInvalid = true;
			base.invalidate();
		}


		/// <summary>
		/// Adds a new cell to the table with the specified element.
		/// </summary>
		/// <param name="element">element.</param>
		public Cell add( Element element )
		{
			var cell = obtainCell();
			cell.element = element;

			// the row was ended for layout, not by the user, so revert it.
			if( _implicitEndRow )
			{
				_implicitEndRow = false;
				_rows--;
				_cells.Last().endRow = false;
			}
				
			var cellCount = _cells.Count;
			if( cellCount > 0 )
			{
				// Set cell column and row.
				var lastCell = _cells.Last();
				if( !lastCell.endRow )
				{
					cell.column = lastCell.column + lastCell.colspan.Value;
					cell.row = lastCell.row;
				}
				else
				{
					cell.column = 0;
					cell.row = lastCell.row + 1;
				}

				// set the index of the cell above.
				if( cell.row > 0 )
				{
					for( var i = cellCount - 1; i >= 0; i-- )
					{
						var other = _cells[i];
						for( int column = other.column, nn = column + other.colspan.Value; column < nn; column++ )
						{
							if( column == cell.column )
							{
								cell.cellAboveIndex = i;
								goto outer;
							}
						}
					}
					outer:
					{}
				}
			}
			else
			{
				cell.column = 0;
				cell.row = 0;
			}
			_cells.Add( cell );

			cell.set( _cellDefaults );
			if( cell.column < _columnDefaults.Count )
			{
				var columnCell = _columnDefaults[cell.column];
				if( columnCell != null )
					cell.merge( columnCell );
			}
			cell.merge( _rowDefaults );

			if( element != null )
				addElement( element );

			return cell;
		}


		/// <summary>
		/// Adds a new cell with a label
		/// </summary>
		/// <param name="text">Text.</param>
		public Cell add( string text )
		{
			return add( new Label( text ) );
		}


		/// <summary>
		/// Adds a cell without an element
		/// </summary>
		public Cell add()
		{
			return add( (Element)null );
		}


		/// <summary>
		/// Adds a new cell to the table with the specified elements in a {@link Stack}.
		/// </summary>
		/// <param name="elements">Elements.</param>
		public Cell stack( params Element[] elements )
		{
			var stack = new Stack();

			foreach( var element in elements )
				stack.add( element );
			
			return add( stack );
		}


		public override bool removeElement( Element element )
		{
			if( !base.removeElement( element ) )
				return false;
			
			var cell = getCell( element );
			if( cell != null )
				cell.element = null;
			return true;
		}


		/// <summary>
		/// Removes all elements and cells from the table
		/// </summary>
		public override void clearChildren()
		{
			for( int i = _cells.Count - 1; i >= 0; i-- )
			{
				var cell = _cells[i];
				var element = cell.element;
				if( element != null )
					element.remove();

				Pool<Cell>.free( cell );
			}
				
			_cells.Clear();
			_rows = 0;
			_columns = 0;

			if( _rowDefaults != null )
				Pool<Cell>.free( _rowDefaults );

			_rowDefaults = null;
			_implicitEndRow = false;

			base.clearChildren();
		}


		/// <summary>
		/// Removes all elements and cells from the table (same as {@link #clear()}) and additionally resets all table properties and
		/// cell, column, and row defaults.
		/// </summary>
		public void reset()
		{
			clear();
			_padTop = backgroundTop;
			_padLeft = backgroundLeft;
			_padBottom = backgroundBottom;
			_padRight = backgroundRight;
			_align = AlignInternal.center;
			_tableDebug = TableDebug.None;

			_cellDefaults.reset();

			for( int i = 0, n = _columnDefaults.Count; i < n; i++ )
			{
				var columnCell = _columnDefaults[i];
				if( columnCell != null )
					Pool<Cell>.free( columnCell );
			}
			_columnDefaults.Clear();
		}


		/// <summary>
		/// Indicates that subsequent cells should be added to a new row and returns the cell values that will be used as the defaults
		/// for all cells in the new row.
		/// </summary>
		public Cell row()
		{
			if( _cells.Count > 0 )
			{
				endRow();
				invalidate();
			}

			if( _rowDefaults != null )
				Pool<Cell>.free( _rowDefaults );
			
			_rowDefaults = Pool<Cell>.obtain();
			_rowDefaults.clear();
			return _rowDefaults;
		}


		void endRow()
		{
			var rowColumns = 0;
			for( var i = _cells.Count - 1; i >= 0; i-- )
			{
				var cell = _cells[i];
				if( cell.endRow )
					break;
				rowColumns += cell.colspan.Value;
			}

			_columns = Math.Max( _columns, rowColumns );
			_rows++;
			_cells.Last().endRow = true;
		}


		/// <summary>
		/// Gets the cell values that will be used as the defaults for all cells in the specified column. Columns are indexed starting at 0
		/// </summary>
		/// <returns>The column defaults.</returns>
		/// <param name="column">Column.</param>
		public Cell getColumnDefaults( int column )
		{
			var cell = _columnDefaults.Count > column ? _columnDefaults[column] : null;
			if( cell == null )
			{
				cell = obtainCell();
				cell.clear();
				if( column >= _columnDefaults.Count )
				{
					for( int i = _columnDefaults.Count; i < column; i++ )
						_columnDefaults.Add( null );
					_columnDefaults.Add( cell );
				}
				else
				{
					_columnDefaults[column] = cell;
				}
			}
			return cell;
		}


		#region Chainable Configuration

		/// <summary>
		/// The cell values that will be used as the defaults for all cells.
		/// </summary>
		public Cell defaults()
		{
			return _cellDefaults;
		}
			

		public Table setFillParent( bool fillParent )
		{
			this.fillParent = fillParent;
			return this;
		}


		/// <summary>
		/// background may be null to clear the background.
		/// </summary>
		/// <returns>The background.</returns>
		/// <param name="background">Background.</param>
		public Table setBackground( IDrawable background )
		{
			if( this._background == background )
				return this;

			float padTopOld = getPadTop(), padLeftOld = getPadLeft(), padBottomOld = getPadBottom(), padRightOld = getPadRight();
			this._background = background;
			float padTopNew = getPadTop(), padLeftNew = getPadLeft(), padBottomNew = getPadBottom(), padRightNew = getPadRight();
			if( padTopOld + padBottomOld != padTopNew + padBottomNew || padLeftOld + padRightOld != padLeftNew + padRightNew )
				invalidateHierarchy();
			else if( padTopOld != padTopNew || padLeftOld != padLeftNew || padBottomOld != padBottomNew || padRightOld != padRightNew )
				invalidate();

			return this;
		}


		/// <summary>
		/// If true (the default), positions and sizes are rounded to integers.
		/// </summary>
		/// <param name="round">If set to <c>true</c> round.</param>
		public Table round( bool round )
		{
			_round = round;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value.
		/// </summary>
		/// <param name="pad">Pad.</param>
		public Table pad( Value pad )
		{
			if( pad == null )
				throw new Exception( "pad cannot be null." );
			
			_padTop = pad;
			_padLeft = pad;
			_padBottom = pad;
			_padRight = pad;
			_sizeInvalid = true;

			return this;
		}


		public Table pad( Value top, Value left, Value bottom, Value right )
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
			_sizeInvalid = true;

			return this;
		}


		/// <summary>
		/// Padding at the top edge of the table.
		/// </summary>
		/// <returns>The top.</returns>
		/// <param name="padTop">Pad top.</param>
		public Table padTop( Value padTop )
		{
			if( padTop == null )
				throw new Exception( "padTop cannot be null." );
			_padTop = padTop;
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Padding at the left edge of the table.
		/// </summary>
		/// <returns>The left.</returns>
		/// <param name="padLeft">Pad left.</param>
		public Table padLeft( Value padLeft )
		{
			if( padLeft == null )
				throw new Exception( "padLeft cannot be null." );
			_padLeft = padLeft;
			_sizeInvalid = true;

			return this;
		}


		/// <summary>
		/// Padding at the bottom edge of the table.
		/// </summary>
		/// <returns>The bottom.</returns>
		/// <param name="padBottom">Pad bottom.</param>
		public Table padBottom( Value padBottom )
		{
			if( padBottom == null )
				throw new Exception( "padBottom cannot be null." );
			_padBottom = padBottom;
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Padding at the right edge of the table.
		/// </summary>
		/// <returns>The right.</returns>
		/// <param name="padRight">Pad right.</param>
		public Table padRight( Value padRight )
		{
			if( padRight == null )
				throw new Exception( "padRight cannot be null." );
			_padRight = padRight;
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value.
		/// </summary>
		/// <param name="pad">Pad.</param>
		public Table pad( float pad )
		{
			this.pad( new Value.Fixed( pad ) );
			return this;
		}


		public Table pad( float top, float left, float bottom, float right )
		{
			_padTop = new Value.Fixed( top );
			_padLeft = new Value.Fixed( left );
			_padBottom = new Value.Fixed( bottom );
			_padRight = new Value.Fixed( right );
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Padding at the top edge of the table.
		/// </summary>
		/// <returns>The top.</returns>
		/// <param name="padTop">Pad top.</param>
		public Table padTop( float padTop )
		{
			_padTop = new Value.Fixed( padTop );
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Padding at the left edge of the table.
		/// </summary>
		/// <returns>The left.</returns>
		/// <param name="padLeft">Pad left.</param>
		public Table padLeft( float padLeft )
		{
			_padLeft = new Value.Fixed( padLeft );
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Padding at the bottom edge of the table.
		/// </summary>
		/// <returns>The bottom.</returns>
		/// <param name="padBottom">Pad bottom.</param>
		public Table padBottom( float padBottom )
		{
			_padBottom = new Value.Fixed( padBottom );
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Padding at the right edge of the table.
		/// </summary>
		/// <returns>The right.</returns>
		/// <param name="padRight">Pad right.</param>
		public Table padRight( float padRight )
		{
			_padRight = new Value.Fixed( padRight );
			_sizeInvalid = true;
			return this;
		}


		/// <summary>
		/// Alignment of the logical table within the table element. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom}
		/// {@link Align#left}, {@link Align#right}, or any combination of those.
		/// </summary>
		/// <param name="align">Align.</param>
		public Table align( int align )
		{
			this._align = align;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the logical table within the table element to {@link Align#center}. This clears any other alignment.
		/// </summary>
		public Table center()
		{
			_align = AlignInternal.center;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#top} and clears {@link Align#bottom} for the alignment of the logical table within the table element.
		/// </summary>
		public Table top()
		{
			_align |= AlignInternal.top;
			_align &= ~AlignInternal.bottom;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#left} and clears {@link Align#right} for the alignment of the logical table within the table element.
		/// </summary>
		public Table left()
		{
			_align |= AlignInternal.left;
			_align &= ~AlignInternal.right;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#bottom} and clears {@link Align#top} for the alignment of the logical table within the table element.
		/// </summary>
		public Table bottom()
		{
			_align |= AlignInternal.bottom;
			_align &= ~AlignInternal.top;
			return this;
		}


		/// <summary>
		/// Adds {@link Align#right} and clears {@link Align#left} for the alignment of the logical table within the table element.
		/// </summary>
		public Table right()
		{
			_align |= AlignInternal.right;
			_align &= ~AlignInternal.left;
			return this;
		}


		/// <summary>
		/// enables/disables all debug lines (table, cell, and widget)
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		public override void setDebug( bool enabled )
		{
			tableDebug( enabled ? TableDebug.All : TableDebug.None );
			_debug = enabled;
		}


		/// <summary>
		/// Turn on all debug lines (table, cell, and element)
		/// </summary>
		/// <returns>The all.</returns>
		public new Table debugAll()
		{
			setDebug( true );
			base.debugAll();
			return this;
		}


		/// <summary>
		/// Turns on table debug lines.
		/// </summary>
		/// <returns>The table.</returns>
		public Table debugTable()
		{
			base.setDebug( true );
			if( _tableDebug != TableDebug.Table )
			{
				_tableDebug = TableDebug.Table;
				invalidate();
			}
			return this;
		}


		/// <summary>
		/// Turns on cell debug lines.
		/// </summary>
		/// <returns>The cell.</returns>
		public Table debugCell()
		{
			base.setDebug( true );
			if( _tableDebug != TableDebug.Cell )
			{
				_tableDebug = TableDebug.Cell;
				invalidate();
			}
			return this;
		}


		/// <summary>
		/// Turns on element debug lines.
		/// </summary>
		/// <returns>The element.</returns>
		public Table debugElement()
		{
			base.setDebug( true );
			if( _tableDebug != TableDebug.Element )
			{
				_tableDebug = TableDebug.Element;
				invalidate();
			}
			return this;
		}


		/// <summary>
		/// Turns debug lines on or off.
		/// </summary>
		/// <returns>The debug.</returns>
		/// <param name="tableDebug">Table debug.</param>
		public Table tableDebug( TableDebug tableDebug )
		{
			base.setDebug( tableDebug != TableDebug.None );
			if( _tableDebug != tableDebug )
			{
				_tableDebug = tableDebug;
				if( _tableDebug == TableDebug.None )
				{
					if( _debugRects != null )
						_debugRects.Clear();
				}
				else
				{
					invalidate();
				}
			}
			return this;
		}

		#endregion


		#region Getters

		/// <summary>
		/// Returns the cell for the specified element in this table, or null.
		/// </summary>
		/// <returns>The cell.</returns>
		/// <param name="element">element.</param>
		public Cell getCell( Element element )
		{
			for( int i = 0, n = _cells.Count; i < n; i++ )
			{
				var c = _cells[i];
				if( c.element == element )
					return c;
			}
			return null;
		}


		/// <summary>
		/// returns all the Cells in the table
		/// </summary>
		/// <returns>The cells.</returns>
		public List<Cell> getCells()
		{
			return _cells;
		}


		public Value getPadTopValue()
		{
			return _padTop;
		}


		public float getPadTop()
		{
			return _padTop.get( this );
		}


		public Value getPadLeftValue()
		{
			return _padLeft;
		}


		public float getPadLeft()
		{
			return _padLeft.get( this );
		}


		public Value getPadBottomValue()
		{
			return _padBottom;
		}


		public float getPadBottom()
		{
			return _padBottom.get( this );
		}


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
		/// Returns {@link #getPadTop()} plus {@link #getPadBottom()}.
		/// </summary>
		/// <returns>The pad y.</returns>
		public float getPadY()
		{
			return _padTop.get( this ) + _padBottom.get( this );
		}


		public int getAlign()
		{
			return _align;
		}


		public int getRows()
		{
			return _rows;
		}


		public int getColumns()
		{
			return _columns;
		}


		/// <summary>
		/// Returns the height of the specified row.
		/// </summary>
		/// <returns>The row height.</returns>
		/// <param name="rowIndex">Row index.</param>
		public float getRowHeight( int rowIndex )
		{
			return _rowHeight[rowIndex];
		}


		/// <summary>
		/// Returns the width of the specified column.
		/// </summary>
		/// <returns>The column width.</returns>
		/// <param name="columnIndex">Column index.</param>
		public float getColumnWidth( int columnIndex )
		{
			return _columnWidth[columnIndex];
		}

		#endregion


		float[] ensureSize( float[] array, int size )
		{
			if( array == null || array.Length < size )
				return new float[size];
			
			for( int i = 0, n = array.Length; i < n; i++ )
				array[i] = 0;
			
			return array;
		}


		public override void layout()
		{
			layout( 0, 0, width, height );

			if( _round )
			{
				for( int i = 0, n = _cells.Count; i < n; i++ )
				{
					var c = _cells[i];
					var elementWidth = Mathf.round( c.elementWidth );
					var elementHeight = Mathf.round( c.elementHeight );
					var elementX = Mathf.round( c.elementX );
					var elementY = Mathf.round( c.elementY );
					c.setElementBounds( elementX, elementY, elementWidth, elementHeight );

					if( c.element != null )
						c.element.setBounds( elementX, elementY, elementWidth, elementHeight );
				}
			}
			else
			{
				for( int i = 0, n = _cells.Count; i < n; i++ )
				{
					var c = _cells[i];
					var elementY = c.elementY;
					c.setElementY( elementY );

					if( c.element != null )
						c.element.setBounds( c.elementX, elementY, c.elementWidth, c.elementHeight );
				}
			}

			// Validate children separately from sizing elements to ensure elements without a cell are validated.
			for( int i = 0, n = children.Count; i < n; i++ )
			{
				var child = children[i];
				if( child is ILayout )
					( (ILayout)child ).validate();
			}
		}


		/// <summary>
		/// Positions and sizes children of the table using the cell associated with each child. The values given are the position
		/// within the parent and size of the table.
		/// </summary>
		/// <param name="layoutX">Layout x.</param>
		/// <param name="layoutY">Layout y.</param>
		/// <param name="layoutWidth">Layout width.</param>
		/// <param name="layoutHeight">Layout height.</param>
		void layout( float layoutX, float layoutY, float layoutWidth, float layoutHeight )
		{
			if( _sizeInvalid )
				computeSize();

			var cellCount = _cells.Count;
			var padLeft = _padLeft.get( this );
			var hpadding = padLeft + _padRight.get( this );
			var padTop = _padTop.get( this );
			var vpadding = padTop + _padBottom.get( this );

			int columns = _columns, rows = _rows;
			float[] expandWidth = _expandWidth, expandHeight = _expandHeight;
			float[] columnWidth = _columnWidth, rowHeight = _rowHeight;

			float totalExpandWidth = 0, totalExpandHeight = 0;
			for( var i = 0; i < columns; i++ )
				totalExpandWidth += expandWidth[i];
			for( var i = 0; i < rows; i++ )
				totalExpandHeight += expandHeight[i];

			// Size columns and rows between min and pref size using (preferred - min) size to weight distribution of extra space.
			float[] columnWeightedWidth;
			float totalGrowWidth = _tablePrefWidth - _tableMinWidth;
			if( totalGrowWidth == 0 )
			{
				columnWeightedWidth = _columnMinWidth;
			}
			else
			{
				var extraWidth = Math.Min( totalGrowWidth, Math.Max( 0, layoutWidth - _tableMinWidth ) );
				columnWeightedWidth = Table._columnWeightedWidth = ensureSize( Table._columnWeightedWidth, columns );
				float[] columnMinWidth = _columnMinWidth, columnPrefWidth = _columnPrefWidth;
				for( var i = 0; i < columns; i++ )
				{
					var growWidth = columnPrefWidth[i] - columnMinWidth[i];
					var growRatio = growWidth / totalGrowWidth;
					columnWeightedWidth[i] = columnMinWidth[i] + extraWidth * growRatio;
				}
			}

			float[] rowWeightedHeight;
			var totalGrowHeight = _tablePrefHeight - _tableMinHeight;
			if( totalGrowHeight == 0 )
			{
				rowWeightedHeight = _rowMinHeight;
			}
			else
			{
				rowWeightedHeight = Table._rowWeightedHeight = ensureSize( Table._rowWeightedHeight, rows );
				var extraHeight = Math.Min( totalGrowHeight, Math.Max( 0, layoutHeight - _tableMinHeight ) );
				float[] rowMinHeight = _rowMinHeight, rowPrefHeight = _rowPrefHeight;
				for( int i = 0; i < rows; i++ )
				{
					float growHeight = rowPrefHeight[i] - rowMinHeight[i];
					float growRatio = growHeight / totalGrowHeight;
					rowWeightedHeight[i] = rowMinHeight[i] + extraHeight * growRatio;
				}
			}

			// Determine element and cell sizes (before expand or fill).
			for( var i = 0; i < cellCount; i++ )
			{
				var cell = _cells[i];
				int column = cell.column, row = cell.row;

				var spannedWeightedWidth = 0f;
				var colspan = cell.colspan.Value;
				for( int ii = column, nn = ii + colspan; ii < nn; ii++ )
					spannedWeightedWidth += columnWeightedWidth[ii];
				var weightedHeight = rowWeightedHeight[row];

				var prefWidth = cell.prefWidth.get( cell.element );
				var prefHeight = cell.prefHeight.get( cell.element );
				var minWidth = cell.minWidth.get( cell.element );
				var minHeight = cell.minHeight.get( cell.element );
				var maxWidth = cell.maxWidth.get( cell.element );
				var maxHeight = cell.maxHeight.get( cell.element );

				if( prefWidth < minWidth )
					prefWidth = minWidth;
				if( prefHeight < minHeight )
					prefHeight = minHeight;
				if( maxWidth > 0 && prefWidth > maxWidth )
					prefWidth = maxWidth;
				if( maxHeight > 0 && prefHeight > maxHeight )
					prefHeight = maxHeight;

				cell.elementWidth = Math.Min( spannedWeightedWidth - cell.computedPadLeft - cell.computedPadRight, prefWidth );
				cell.elementHeight = Math.Min( weightedHeight - cell.computedPadTop - cell.computedPadBottom, prefHeight );

				if( colspan == 1 )
					columnWidth[column] = Math.Max( columnWidth[column], spannedWeightedWidth );
				rowHeight[row] = Math.Max( rowHeight[row], weightedHeight );
			}

			// distribute remaining space to any expanding columns/rows.
			if( totalExpandWidth > 0 )
			{
				var extra = layoutWidth - hpadding;
				for( var i = 0; i < columns; i++ )
					extra -= columnWidth[i];
				
				var used = 0f;
				var lastIndex = 0;
				for( var i = 0; i < columns; i++ )
				{
					if( expandWidth[i] == 0 )
						continue;
					var amount = extra * expandWidth[i] / totalExpandWidth;
					columnWidth[i] += amount;
					used += amount;
					lastIndex = i;
				}
				columnWidth[lastIndex] += extra - used;
			}

			if( totalExpandHeight > 0 )
			{
				var extra = layoutHeight - vpadding;
				for( var i = 0; i < rows; i++ )
					extra -= rowHeight[i];
				
				var used = 0f;
				var lastIndex = 0;
				for( var i = 0; i < rows; i++ )
				{
					if( expandHeight[i] == 0 )
						continue;
					
					var amount = extra * expandHeight[i] / totalExpandHeight;
					rowHeight[i] += amount;
					used += amount;
					lastIndex = i;
				}
				rowHeight[lastIndex] += extra - used;
			}

			// distribute any additional width added by colspanned cells to the columns spanned.
			for( var i = 0; i < cellCount; i++ )
			{
				var c = _cells[i];
				var colspan = c.colspan.Value;
				if( colspan == 1 )
					continue;

				var extraWidth = 0f;
				for( int column = c.column, nn = column + colspan; column < nn; column++ )
					extraWidth += columnWeightedWidth[column] - columnWidth[column];
				extraWidth -= Math.Max( 0, c.computedPadLeft + c.computedPadRight );

				extraWidth /= colspan;
				if( extraWidth > 0 )
				{
					for( int column = c.column, nn = column + colspan; column < nn; column++ )
						columnWidth[column] += extraWidth;
				}
			}

			// Determine table size.
			float tableWidth = hpadding, tableHeight = vpadding;
			for( var i = 0; i < columns; i++ )
				tableWidth += columnWidth[i];
			for( var i = 0; i < rows; i++ )
				tableHeight += rowHeight[i];

			// Position table within the container.
			var x = layoutX + padLeft;
			if( ( _align & AlignInternal.right ) != 0 )
				x += layoutWidth - tableWidth;
			else if( ( _align & AlignInternal.left ) == 0 ) // Center
				x += ( layoutWidth - tableWidth ) / 2;

			var y = layoutY + padTop; // bottom
			if( ( _align & AlignInternal.bottom ) != 0 )
				y += layoutHeight - tableHeight;
			else if( ( _align & AlignInternal.top ) == 0 ) // Center
				y += ( layoutHeight - tableHeight ) / 2;

			// position elements within cells.
			float currentX = x, currentY = y;
			for( var i = 0; i < cellCount; i++ )
			{
				var c = _cells[i];

				var spannedCellWidth = 0f;
				for( int column = c.column, nn = column + c.colspan.Value; column < nn; column++ )
					spannedCellWidth += columnWidth[column];
				spannedCellWidth -= c.computedPadLeft + c.computedPadRight;

				currentX += c.computedPadLeft;

				float fillX = c.fillX.Value, fillY = c.fillY.Value;
				if( fillX > 0 )
				{
					c.elementWidth = Math.Max( spannedCellWidth * fillX, c.minWidth.get( c.element ) );
					float maxWidth = c.maxWidth.get( c.element );
					if( maxWidth > 0 )
						c.elementWidth = Math.Min( c.elementWidth, maxWidth );
				}
				if( fillY > 0 )
				{
					c.elementHeight = Math.Max( rowHeight[c.row] * fillY - c.computedPadTop - c.computedPadBottom, c.minHeight.get( c.element ) );
					float maxHeight = c.maxHeight.get( c.element );
					if( maxHeight > 0 )
						c.elementHeight = Math.Min( c.elementHeight, maxHeight );
				}

				var cellAlign = c.align.Value;
				if( ( cellAlign & AlignInternal.left ) != 0 )
					c.elementX = currentX;
				else if( ( cellAlign & AlignInternal.right ) != 0 )
					c.elementX = currentX + spannedCellWidth - c.elementWidth;
				else
					c.elementX = currentX + ( spannedCellWidth - c.elementWidth ) / 2;

				if( ( cellAlign & AlignInternal.top ) != 0 )
					c.elementY = currentY + c.computedPadTop;
				else if( ( cellAlign & AlignInternal.bottom ) != 0 )
					c.elementY = currentY + rowHeight[c.row] - c.elementHeight - c.computedPadBottom;
				else
					c.elementY = currentY + ( rowHeight[c.row] - c.elementHeight + c.computedPadTop - c.computedPadBottom ) / 2;

				if( c.endRow )
				{
					currentX = x;
					currentY += rowHeight[c.row];
				}
				else
				{
					currentX += spannedCellWidth + c.computedPadRight;
				}
			}

			if( _tableDebug != TableDebug.None )
				computeDebugRects( x, y, layoutX, layoutY, layoutWidth, layoutHeight, tableWidth, tableHeight, hpadding, vpadding );
		}


		void computeSize()
		{
			_sizeInvalid = false;

			var cellCount = _cells.Count;

			// Implicitly End the row for layout purposes.
			if( cellCount > 0 && !_cells.Last().endRow )
			{
				endRow();
				_implicitEndRow = true;
			}
			else
			{
				_implicitEndRow = false;
			}

			int columns = _columns, rows = _rows;
			_columnWidth = ensureSize( _columnWidth, columns );
			_rowHeight = ensureSize( _rowHeight, rows );
			_columnMinWidth = ensureSize( _columnMinWidth, columns );
			_rowMinHeight = ensureSize( _rowMinHeight, rows );
			_columnPrefWidth = ensureSize( _columnPrefWidth, columns );
			_rowPrefHeight = ensureSize( _rowPrefHeight, rows );
			_expandWidth = ensureSize( _expandWidth, columns );
			_expandHeight = ensureSize( _expandHeight, rows );

			var spaceRightLast = 0f;
			for( var i = 0; i < cellCount; i++ )
			{
				var cell = _cells[i];
				int column = cell.column, row = cell.row, colspan = cell.colspan.Value;

				// Collect rows that expand and colspan=1 columns that expand.
				if( cell.expandY != 0 && _expandHeight[row] == 0 )
					_expandHeight[row] = cell.expandY.Value;
				if( colspan == 1 && cell.expandX != 0 && _expandWidth[column] == 0 )
					_expandWidth[column] = cell.expandX.Value;

				// Compute combined padding/spacing for cells.
				// Spacing between elements isn't additive, the larger is used. Also, no spacing around edges.
				cell.computedPadLeft = cell.padLeft.get( cell.element ) + ( column == 0 ? 0 : Math.Max( 0, cell.spaceLeft.get( cell.element ) - spaceRightLast ) );
				cell.computedPadTop = cell.padTop.get( cell.element );
				if( cell.cellAboveIndex != -1 )
				{
					var above = _cells[cell.cellAboveIndex];
					cell.computedPadTop += Math.Max( 0, cell.spaceTop.get( cell.element ) - above.spaceBottom.get( cell.element ) );
				}

				var spaceRight = cell.spaceRight.get( cell.element );
				cell.computedPadRight = cell.padRight.get( cell.element ) + ( ( column + colspan ) == columns ? 0 : spaceRight );
				cell.computedPadBottom = cell.padBottom.get( cell.element ) + ( row == rows - 1 ? 0 : cell.spaceBottom.get( cell.element ) );
				spaceRightLast = spaceRight;

				// Determine minimum and preferred cell sizes.
				var prefWidth = cell.prefWidth.get( cell.element );
				var prefHeight = cell.prefHeight.get( cell.element );
				var minWidth = cell.minWidth.get( cell.element );
				var minHeight = cell.minHeight.get( cell.element );
				var maxWidth = cell.maxWidth.get( cell.element );
				var maxHeight = cell.maxHeight.get( cell.element );

				if( prefWidth < minWidth )
					prefWidth = minWidth;
				if( prefHeight < minHeight )
					prefHeight = minHeight;
				if( maxWidth > 0 && prefWidth > maxWidth )
					prefWidth = maxWidth;
				if( maxHeight > 0 && prefHeight > maxHeight )
					prefHeight = maxHeight;

				if( colspan == 1 )
				{
					// Spanned column min and pref width is added later.
					var hpadding = cell.computedPadLeft + cell.computedPadRight;
					_columnPrefWidth[column] = Math.Max( _columnPrefWidth[column], prefWidth + hpadding );
					_columnMinWidth[column] = Math.Max( _columnMinWidth[column], minWidth + hpadding );
				}
				float vpadding = cell.computedPadTop + cell.computedPadBottom;
				_rowPrefHeight[row] = Math.Max( _rowPrefHeight[row], prefHeight + vpadding );
				_rowMinHeight[row] = Math.Max( _rowMinHeight[row], minHeight + vpadding );
			}

			float uniformMinWidth = 0, uniformMinHeight = 0;
			float uniformPrefWidth = 0, uniformPrefHeight = 0;
			for( var i = 0; i < cellCount; i++ )
			{
				var c = _cells[i];

				// Colspan with expand will expand all spanned columns if none of the spanned columns have expand.
				var expandX = c.expandX.Value;
				if( expandX != 0 )
				{
					int nn = c.column + c.colspan.Value;
					for( int ii = c.column; ii < nn; ii++ )
						if( _expandWidth[ii] != 0 )
							goto outer;
					for( int ii = c.column; ii < nn; ii++ )
						_expandWidth[ii] = expandX;
				}
				outer:
				{}

				// Collect uniform sizes.
				if( c.uniformX.HasValue && c.uniformX.Value && c.colspan == 1 )
				{
					float hpadding = c.computedPadLeft + c.computedPadRight;
					uniformMinWidth = Math.Max( uniformMinWidth, _columnMinWidth[c.column] - hpadding );
					uniformPrefWidth = Math.Max( uniformPrefWidth, _columnPrefWidth[c.column] - hpadding );
				}

				if( c.uniformY.HasValue && c.uniformY.Value )
				{
					float vpadding = c.computedPadTop + c.computedPadBottom;
					uniformMinHeight = Math.Max( uniformMinHeight, _rowMinHeight[c.row] - vpadding );
					uniformPrefHeight = Math.Max( uniformPrefHeight, _rowPrefHeight[c.row] - vpadding );
				}
			}

			// Size uniform cells to the same width/height.
			if( uniformPrefWidth > 0 || uniformPrefHeight > 0 )
			{
				for( var i = 0; i < cellCount; i++ )
				{
					var c = _cells[i];
					if( uniformPrefWidth > 0 && c.uniformX.HasValue && c.uniformX.Value && c.colspan == 1 )
					{
						var hpadding = c.computedPadLeft + c.computedPadRight;
						_columnMinWidth[c.column] = uniformMinWidth + hpadding;
						_columnPrefWidth[c.column] = uniformPrefWidth + hpadding;
					}

					if( uniformPrefHeight > 0 && c.uniformY.HasValue && c.uniformY.Value )
					{
						var vpadding = c.computedPadTop + c.computedPadBottom;
						_rowMinHeight[c.row] = uniformMinHeight + vpadding;
						_rowPrefHeight[c.row] = uniformPrefHeight + vpadding;
					}
				}
			}

			// Distribute any additional min and pref width added by colspanned cells to the columns spanned.
			for( var i = 0; i < cellCount; i++ )
			{
				var c = _cells[i];
				var colspan = c.colspan.Value;
				if( colspan == 1 )
					continue;

				var a = c.element;
				var minWidth = c.minWidth.get( a );
				var prefWidth = c.prefWidth.get( a );
				var maxWidth = c.maxWidth.get( a );
				if( prefWidth < minWidth )
					prefWidth = minWidth;
				if( maxWidth > 0 && prefWidth > maxWidth )
					prefWidth = maxWidth;

				float spannedMinWidth = -( c.computedPadLeft + c.computedPadRight ), spannedPrefWidth = spannedMinWidth;
				var totalExpandWidth = 0f;
				for( int ii = c.column, nn = ii + colspan; ii < nn; ii++ )
				{
					spannedMinWidth += _columnMinWidth[ii];
					spannedPrefWidth += _columnPrefWidth[ii];
					totalExpandWidth += _expandWidth[ii]; // Distribute extra space using expand, if any columns have expand.
				}

				var extraMinWidth = Math.Max( 0, minWidth - spannedMinWidth );
				var extraPrefWidth = Math.Max( 0, prefWidth - spannedPrefWidth );
				for( int ii = c.column, nn = ii + colspan; ii < nn; ii++ )
				{
					float ratio = totalExpandWidth == 0 ? 1f / colspan : _expandWidth[ii] / totalExpandWidth;
					_columnMinWidth[ii] += extraMinWidth * ratio;
					_columnPrefWidth[ii] += extraPrefWidth * ratio;
				}
			}

			// Determine table min and pref size.
			_tableMinWidth = 0;
			_tableMinHeight = 0;
			_tablePrefWidth = 0;
			_tablePrefHeight = 0;
			for( var i = 0; i < columns; i++ )
			{
				_tableMinWidth += _columnMinWidth[i];
				_tablePrefWidth += _columnPrefWidth[i];
			}

			for( var i = 0; i < rows; i++ )
			{
				_tableMinHeight += _rowMinHeight[i];
				_tablePrefHeight += Math.Max( _rowMinHeight[i], _rowPrefHeight[i] );
			}

			var hpadding_ = _padLeft.get( this ) + _padRight.get( this );
			var vpadding_ = _padTop.get( this ) + _padBottom.get( this );
			_tableMinWidth = _tableMinWidth + hpadding_;
			_tableMinHeight = _tableMinHeight + vpadding_;
			_tablePrefWidth = Math.Max( _tablePrefWidth + hpadding_, _tableMinWidth );
			_tablePrefHeight = Math.Max( _tablePrefHeight + vpadding_, _tableMinHeight );
		}


		#region Debug

		public override void debugRender( Graphics graphics )
		{
			if( _debugRects != null )
			{
				foreach( var d in _debugRects )
					graphics.batcher.drawHollowRect( x + d.rect.x, y + d.rect.y, d.rect.width, d.rect.height, d.color );
			}

			base.debugRender( graphics );
		}


		void computeDebugRects( float x, float y, float layoutX, float layoutY, float layoutWidth, float layoutHeight, float tableWidth, float tableHeight, float hpadding, float vpadding )
		{
			if( _debugRects != null )
				_debugRects.Clear();

			var currentX = x;
			var currentY = y;
			if( _tableDebug == TableDebug.Table || _tableDebug == TableDebug.All )
			{
				addDebugRect( layoutX, layoutY, layoutWidth, layoutHeight, debugTableColor );
				addDebugRect( x, y, tableWidth - hpadding, tableHeight - vpadding, debugTableColor );
			}

			for( var i = 0; i < _cells.Count; i++ )
			{
				var cell = _cells[i];

				// element bounds.
				if( _tableDebug == TableDebug.Element || _tableDebug == TableDebug.All )
					addDebugRect( cell.elementX, cell.elementY, cell.elementWidth, cell.elementHeight, debugElementColor );

				// Cell bounds.
				float spannedCellWidth = 0;
				for( int column = cell.column, nn = column + cell.colspan.Value; column < nn; column++ )
					spannedCellWidth += _columnWidth[column];
				spannedCellWidth -= cell.computedPadLeft + cell.computedPadRight;
				currentX += cell.computedPadLeft;

				if( _tableDebug == TableDebug.Cell || _tableDebug == TableDebug.All )
				{
					addDebugRect( currentX, currentY + cell.computedPadTop, spannedCellWidth,
						_rowHeight[cell.row] - cell.computedPadTop - cell.computedPadBottom, debugCellColor );
				}

				if( cell.endRow )
				{
					currentX = x;
					currentY += _rowHeight[cell.row];
				}
				else
				{
					currentX += spannedCellWidth + cell.computedPadRight;
				}
			}
		}


		void addDebugRect( float x, float y, float w, float h, Color color )
		{
			if( _debugRects == null )
				_debugRects = new List<DebugRectangleF>();
			
			var rect = new DebugRectangleF( x, y, w, h, color );
			_debugRects.Add( rect );
		}

		#endregion


		#region Value types

		static public Value backgroundTop = new BackgroundTopValue();

		/// <summary>
		/// Value that is the top padding of the table's background
		/// </summary>
		public class BackgroundTopValue : Value
		{
			public override float get( Element context )
			{
				var background = ( (Table)context )._background;
				return background == null ? 0 : background.topHeight;
			}
		}


		static public Value backgroundLeft = new BackgroundLeftValue();

		/// <summary>
		/// Value that is the left padding of the table's background
		/// </summary>
		public class BackgroundLeftValue : Value
		{
			public override float get( Element context )
			{
				var background = ( (Table)context )._background;
				return background == null ? 0 : background.leftWidth;
			}
		}


		static public Value backgroundBottom = new BackgroundBottomValue();

		/// <summary>
		/// Value that is the bottom padding of the table's background
		/// </summary>
		public class BackgroundBottomValue : Value
		{
			public override float get( Element context )
			{
				var background = ( (Table)context )._background;
				return background == null ? 0 : background.bottomHeight;
			}
		}


		static public Value backgroundRight = new BackgroundRightValue();

		/// <summary>
		/// Value that is the right padding of the table's background
		/// </summary>
		public class BackgroundRightValue : Value
		{
			public override float get( Element context )
			{
				var background = ( (Table)context )._background;
				return background == null ? 0 : background.rightWidth;
			}
		}

		#endregion

	}
}

