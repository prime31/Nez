using System;


namespace Nez.UI
{
	public class HorizontalGroup : Group
	{
		public override float PreferredWidth
		{
			get
			{
				if (_sizeInvalid)
					ComputeSize();
				return _prefWidth;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				if (_sizeInvalid)
					ComputeSize();
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


		public HorizontalGroup(float spacing) : this()
		{
			SetSpacing(spacing);
		}


		public override void Invalidate()
		{
			_sizeInvalid = true;
			base.Invalidate();
		}


		void ComputeSize()
		{
			_sizeInvalid = false;
			_prefWidth = _padLeft + _padRight + _spacing * (children.Count - 1);
			_prefHeight = 0;
			for (var i = 0; i < children.Count; i++)
			{
				var child = children[i];
				if (child is ILayout)
				{
					var layout = (ILayout) child;
					_prefWidth += layout.PreferredWidth;
					_prefHeight = Math.Max(_prefHeight, layout.PreferredHeight);
				}
				else
				{
					_prefWidth += child.width;
					_prefHeight += Math.Max(_prefHeight, child.height);
					;
				}
			}

			_prefHeight += _padTop + _padBottom;
			if (_round)
			{
				_prefWidth = Mathf.Round(_prefWidth);
				_prefHeight = Mathf.Round(_prefHeight);
			}
		}


		public override void Layout()
		{
			var groupHeight = height - _padTop - _padBottom;
			var x = !_reverse ? _padLeft : width - _padRight + _spacing;

			for (var i = 0; i < children.Count; i++)
			{
				var child = children[i];
				float width, height;
				ILayout layout = null;

				if (child is ILayout)
				{
					layout = (ILayout) child;
					if (_fill > 0)
						height = groupHeight * _fill;
					else
						height = Math.Min(layout.PreferredHeight, groupHeight);
					height = Math.Max(height, layout.MinHeight);

					var maxheight = layout.MaxHeight;
					if (maxheight > 0 && height > MaxHeight)
						height = maxheight;
					width = layout.PreferredWidth;
				}
				else
				{
					width = child.width;
					height = child.height;

					if (_fill > 0)
						height *= _fill;
				}

				var y = _padTop;
				if ((_align & AlignInternal.Bottom) != 0)
					y += groupHeight - height;
				else if ((_align & AlignInternal.Top) == 0) // center
					y += (groupHeight - height) / 2;

				if (_reverse)
					x -= (width + _spacing);

				if (_round)
					child.SetBounds(Mathf.Round(x), Mathf.Round(y), Mathf.Round(width), Mathf.Round(height));
				else
					child.SetBounds(x, y, width, height);

				if (!_reverse)
					x += (width + _spacing);

				if (layout != null)
					layout.Validate();
			}
		}


		#region Configuration

		/// <summary>
		/// Sets the alignment of widgets within the vertical group. Set to {@link Align#center}, {@link Align#top},
		/// {@link Align#bottom}, {@link Align#left}, {@link Align#right}, or any combination of those
		/// </summary>
		/// <param name="align">Align.</param>
		public HorizontalGroup SetAlignment(Align align)
		{
			_align = (int) align;
			return this;
		}


		/// <summary>
		/// If true, the children will be ordered from bottom to top rather than the default top to bottom.
		/// </summary>
		/// <param name="reverse">If set to <c>true</c> reverse.</param>
		public HorizontalGroup SetReverse(bool reverse)
		{
			_reverse = reverse;
			return this;
		}


		/// <summary>
		/// Sets the space between children
		/// </summary>
		/// <param name="spacing">Spacing.</param>
		public HorizontalGroup SetSpacing(float spacing)
		{
			_spacing = spacing;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value
		/// </summary>
		/// <param name="pad">Pad.</param>
		public HorizontalGroup SetPad(float pad)
		{
			_padTop = pad;
			_padLeft = pad;
			_padBottom = pad;
			_padRight = pad;
			return this;
		}


		public HorizontalGroup SetPad(float top, float left, float bottom, float right)
		{
			_padTop = top;
			_padLeft = left;
			_padBottom = bottom;
			_padRight = right;
			return this;
		}


		public HorizontalGroup SetPadTop(float padTop)
		{
			_padTop = padTop;
			return this;
		}


		public HorizontalGroup SetPadLeft(float padLeft)
		{
			_padLeft = padLeft;
			return this;
		}


		public HorizontalGroup SetPadBottom(float padBottom)
		{
			_padBottom = padBottom;
			return this;
		}


		public HorizontalGroup SetPadRight(float padRight)
		{
			_padRight = padRight;
			return this;
		}


		/// <summary>
		/// If true (the default), positions and sizes are rounded to integers.
		/// </summary>
		/// <param name="round">If set to <c>true</c> round.</param>
		public HorizontalGroup SetRound(bool round)
		{
			_round = round;
			return this;
		}


		/// <summary>
		/// fill 0 will use pref width
		/// </summary>
		/// <param name="fill">Fill.</param>
		public HorizontalGroup SetFill(float fill)
		{
			_fill = fill;
			return this;
		}

		#endregion
	}
}