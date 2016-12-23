using System;


namespace Nez.UI
{
	public class HorizontalGroup : Group
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


		public bool _round = true;
		public int _align;
		public bool _reverse;

		public float _spacing;
		public float _padTop, _padLeft, _padBottom, _padRight;
		public float _fill;

		bool _sizeInvalid = true;


		public HorizontalGroup()
		{
			touchable = Touchable.ChildrenOnly;
		}


		public HorizontalGroup( float spacing ) : this()
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
			_prefWidth = _padLeft + _padRight + _spacing * ( children.Count - 1 );
			_prefHeight = 0;
			for( var i = 0; i < children.Count; i++ )
			{
				var child = children[i];
				if( child is ILayout )
				{
					var layout = (ILayout)child;
					_prefWidth += layout.preferredWidth;
					_prefHeight = Math.Max( _prefHeight, layout.preferredHeight );
				}
				else
				{
					_prefWidth += child.width;
					_prefHeight += Math.Max( _prefHeight, child.height );;
				}
			}

			_prefHeight += _padTop + _padBottom;
			if( _round )
			{
				_prefWidth = Mathf.round( _prefWidth );
				_prefHeight = Mathf.round( _prefHeight );
			}
		}


		public override void layout()
		{
			var groupHeight = height - _padTop - _padBottom;
			var x = !_reverse ? _padLeft : width - _padRight + _spacing;

			for( var i = 0; i < children.Count; i++ )
			{
				var child = children[i];
				float width, height;
				ILayout layout = null;

				if( child is ILayout )
				{
					layout = (ILayout)child;
					if( _fill > 0 )
						height = groupHeight * _fill;
					else
						height = Math.Min( layout.preferredHeight, groupHeight );
					height = Math.Max( height, layout.minHeight );

					var maxheight = layout.maxHeight;
					if( maxheight > 0 && height > maxHeight )
						height = maxheight;
					width = layout.preferredWidth;
				}
				else
				{
					width = child.width;
					height = child.height;

					if( _fill > 0 )
						height *= _fill;
				}

				var y = _padTop;
				if( ( _align & AlignInternal.bottom ) != 0 )
					y += groupHeight - height;
				else if( ( _align & AlignInternal.top ) == 0 ) // center
					y += ( groupHeight - height ) / 2;

				if( _reverse )
					x -= ( width + _spacing );

				if( _round )
					child.setBounds( Mathf.round( x ), Mathf.round( y ), Mathf.round( width ), Mathf.round( height ) );
				else
					child.setBounds( x, y, width, height );

				if( !_reverse )
					x += ( width + _spacing );

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
		public HorizontalGroup setAlignment( Align align )
		{
			_align = (int)align;
			return this;
		}


		/// <summary>
		/// If true, the children will be ordered from bottom to top rather than the default top to bottom.
		/// </summary>
		/// <returns>The reverse.</returns>
		/// <param name="reverse">If set to <c>true</c> reverse.</param>
		public HorizontalGroup setReverse( bool reverse )
		{
			_reverse = reverse;
			return this;
		}


		/// <summary>
		/// Sets the space between children
		/// </summary>
		/// <returns>The spacing.</returns>
		/// <param name="spacing">Spacing.</param>
		public HorizontalGroup setSpacing( float spacing )
		{
			_spacing = spacing;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value
		/// </summary>
		/// <returns>The pad.</returns>
		/// <param name="pad">Pad.</param>
		public HorizontalGroup setPad( float pad )
		{
			_padTop = pad;
			_padLeft = pad;
			_padBottom = pad;
			_padRight = pad;
			return this;
		}


		public HorizontalGroup setPad( float top, float left, float bottom, float right )
		{
			_padTop = top;
			_padLeft = left;
			_padBottom = bottom;
			_padRight = right;
			return this;
		}


		public HorizontalGroup setPadTop( float padTop )
		{
			_padTop = padTop;
			return this;
		}


		public HorizontalGroup setPadLeft( float padLeft )
		{
			_padLeft = padLeft;
			return this;
		}


		public HorizontalGroup setPadBottom( float padBottom )
		{
			_padBottom = padBottom;
			return this;
		}


		public HorizontalGroup setPadRight( float padRight )
		{
			_padRight = padRight;
			return this;
		}


		/// <summary>
		/// If true (the default), positions and sizes are rounded to integers.
		/// </summary>
		/// <returns>The round.</returns>
		/// <param name="round">If set to <c>true</c> round.</param>
		public HorizontalGroup setRound( bool round )
		{
			_round = round;
			return this;
		}


		/// <summary>
		/// fill 0 will use pref width
		/// </summary>
		/// <returns>The fill.</returns>
		/// <param name="fill">Fill.</param>
		public HorizontalGroup setFill( float fill )
		{
			_fill = fill;
			return this;
		}

		#endregion

	}
}

