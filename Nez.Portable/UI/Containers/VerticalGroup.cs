using System;


namespace Nez.UI
{
	/// <summary>
	/// A group that lays out its children on top of each other in a single column. This can be easier than using {@link Table} when
	/// elements need to be inserted in the middle of the group.
	/// 
	/// The preferred width is the largest preferred width of any child. The preferred height is the sum of the children's preferred
	/// heights, plus spacing between them if set. The min size is the preferred size and the max size is 0.
	/// </summary>
	public class VerticalGroup : Group
	{
		public override float preferredWidth
		{
			get
			{
				if( _sizeInvalid )
					computeSize();
				return _prefWidth;
			}
		}

		public override float preferredHeight
		{
			get
			{
				if( _sizeInvalid )
					computeSize();
				return _prefHeight;
			}
		}

		float _prefWidth, _prefHeight;

		bool _round = true;
		int _align;
		bool _reverse;
		float _spacing;
		float _padTop, _padLeft, _padBottom, _padRight;
		float _fill;
		bool _sizeInvalid = true;


		public VerticalGroup()
		{
			touchable = Touchable.ChildrenOnly;
		}


		public VerticalGroup( float spacing ) : this()
		{
			setSpacing( spacing );
		}


		public override void invalidate()
		{
			_sizeInvalid = true;
			base.invalidate();
		}


		void computeSize()
		{
			_sizeInvalid = false;
			_prefWidth = 0;
			_prefHeight = _padTop + _padBottom + _spacing * ( children.Count - 1 );
			for( var i = 0; i < children.Count; i++ )
			{
				var child = children[i];
				if( child is ILayout )
				{
					var layout = (ILayout)child;
					_prefWidth = Math.Max( _prefWidth, layout.preferredWidth );
					_prefHeight += layout.preferredHeight;
				}
				else
				{
					_prefWidth = Math.Max( _prefWidth, child.width );
					_prefHeight += child.height;
				}
			}

			_prefWidth += _padLeft + _padRight;
			if( _round )
			{
				_prefWidth = Mathf.round( _prefWidth );
				_prefHeight = Mathf.round( _prefHeight );
			}
		}


		public override void layout()
		{
			var groupWidth = width - _padLeft - _padRight;
			var y = _reverse ? height - _padBottom + _spacing : _padTop;

			for( var i = 0; i < children.Count; i++ )
			{
				var child = children[i];
				float width, height;

				ILayout layout = null;
				if( child is ILayout )
				{
					layout = (ILayout)child;
					if( _fill > 0 )
						width = groupWidth * _fill;
					else
						width = Math.Min( layout.preferredWidth, groupWidth );
					width = Math.Max( width, layout.minWidth );

					if( layout.maxWidth > 0 && width > layout.maxWidth )
						width = layout.maxWidth;
					height = layout.preferredHeight;
				}
				else
				{
					width = child.width;
					height = child.height;

					if( _fill > 0 )
						width *= _fill;
				}

				var x = _padLeft;
				if( ( _align & AlignInternal.right ) != 0 )
					x += groupWidth - width;
				else if( ( _align & AlignInternal.left ) == 0 ) // center
					x += ( groupWidth - width ) / 2;
				
				if( _reverse )
					y -= ( height + _spacing );
				
				if( _round )
					child.setBounds( Mathf.round( x ), Mathf.round( y ), Mathf.round( width ), Mathf.round( height ) );
				else
					child.setBounds( x, y, width, height );
				
				if( !_reverse )
					y += ( height + _spacing );

				if( layout != null )
					layout.validate();
			}
		}

	
		#region Configuration

		/// <summary>
		/// Sets the alignment of widgets within the vertical group. Set to {@link Align#center}, {@link Align#top},
		/// {@link Align#bottom}, {@link Align#left}, {@link Align#right}, or any combination of those
		/// </summary>
		/// <returns>The alignment.</returns>
		/// <param name="align">Align.</param>
		public VerticalGroup setAlignment( Align align )
		{
			_align = (int)align;
			return this;
		}


		/// <summary>
		/// If true, the children will be ordered from bottom to top rather than the default top to bottom.
		/// </summary>
		/// <returns>The reverse.</returns>
		/// <param name="reverse">If set to <c>true</c> reverse.</param>
		public VerticalGroup setReverse( bool reverse )
		{
			_reverse = reverse;
			return this;
		}


		/// <summary>
		/// Sets the space between children
		/// </summary>
		/// <returns>The spacing.</returns>
		/// <param name="spacing">Spacing.</param>
		public VerticalGroup setSpacing( float spacing )
		{
			_spacing = spacing;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value
		/// </summary>
		/// <returns>The pad.</returns>
		/// <param name="pad">Pad.</param>
		public VerticalGroup setPad( float pad )
		{
			_padTop = pad;
			_padLeft = pad;
			_padBottom = pad;
			_padRight = pad;
			return this;
		}


		public VerticalGroup setPad( float top, float left, float bottom, float right )
		{
			_padTop = top;
			_padLeft = left;
			_padBottom = bottom;
			_padRight = right;
			return this;
		}


		public VerticalGroup setPadTop( float padTop )
		{
			_padTop = padTop;
			return this;
		}


		public VerticalGroup setPadLeft( float padLeft )
		{
			_padLeft = padLeft;
			return this;
		}


		public VerticalGroup setPadBottom( float padBottom )
		{
			_padBottom = padBottom;
			return this;
		}


		public VerticalGroup setPadRight( float padRight )
		{
			_padRight = padRight;
			return this;
		}


		/// <summary>
		/// If true (the default), positions and sizes are rounded to integers.
		/// </summary>
		/// <returns>The round.</returns>
		/// <param name="round">If set to <c>true</c> round.</param>
		public VerticalGroup setRound( bool round )
		{
			_round = round;
			return this;
		}


		/// <summary>
		/// fill 0 will use pref width
		/// </summary>
		/// <returns>The fill.</returns>
		/// <param name="fill">Fill.</param>
		public VerticalGroup setFill( float fill )
		{
			_fill = fill;
			return this;
		}

		#endregion

	}
}

