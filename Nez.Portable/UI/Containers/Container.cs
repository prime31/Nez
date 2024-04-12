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

		public override float MinWidth => GetMinWidth();

		public override float MinHeight => GetPrefHeight();

		public override float PreferredWidth => GetPrefWidth();

		public override float PreferredHeight => GetPrefHeight();

		public override float MaxWidth => GetMaxWidth();

		public override float MaxHeight => GetMaxHeight();

		#endregion


		Element _element;
		Value _minWidthValue = Value.MinWidth, _minHeightValue = Value.MinHeight;
		Value _prefWidthValue = Value.PrefWidth, _prefHeightValue = Value.PrefHeight;
		Value _maxWidthValue = Value.Zero, _maxHeightValue = Value.Zero;
		Value _padTop = Value.Zero, _padLeft = Value.Zero, _padBottom = Value.Zero, _padRight = Value.Zero;
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
			SetTouchable(Touchable.ChildrenOnly);
			transform = false;
		}


		public Container(Element element) : this()
		{
			SetElement(element);
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			Validate();
			if (transform)
			{
				ApplyTransform(batcher, ComputeTransform());
				DrawBackground(batcher, parentAlpha, 0, 0);
				if (_clip)
				{
					//batcher.flush();
					//float padLeft = this.padLeft.get( this ), padBottom = this.padBottom.get( this );
					//if( clipBegin( padLeft, padBottom,minWidthValueh() - padLeft - padRight.get( tmaxWidth				//	     getHeight() - padBottom - padTop.get( this ) ) )
					{
						DrawChildren(batcher, parentAlpha);

						//batcher.flush();
						//clipEnd();
					}
				}
				else
				{
					DrawChildren(batcher, parentAlpha);
				}

				ResetTransform(batcher);
			}
			else
			{
				DrawBackground(batcher, parentAlpha, GetX(), GetY());
				base.Draw(batcher, parentAlpha);
			}
		}


		/// <summary>
		/// Called to draw the background, before clipping is applied (if enabled). Default implementation draws the background drawable.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="parentAlpha">Parent alpha.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		protected void DrawBackground(Batcher batcher, float parentAlpha, float x, float y)
		{
			if (_background == null)
				return;

			_background.Draw(batcher, x, y, GetWidth(), GetHeight(), ColorExt.Create(color, (int) (color.A * parentAlpha)));
		}


		/// <summary>
		/// Sets the background drawable and adjusts the container's padding to match the background.
		/// </summary>
		/// <param name="background">Background.</param>
		public Container SetBackground(IDrawable background)
		{
			return SetBackground(background, true);
		}


		/// <summary>
		/// Sets the background drawable and, if adjustPadding is true, sets the container's padding to
		/// {@link Drawable#getBottomHeight()} , {@link Drawable#getTopHeight()}, {@link Drawable#getLeftWidth()}, and
		/// {@link Drawable#getRightWidth()}.
		/// If background is null, the background will be cleared and padding removed.
		/// </summary>
		/// <param name="background">Background.</param>
		/// <param name="adjustPadding">If set to <c>true</c> adjust padding.</param>
		public Container SetBackground(IDrawable background, bool adjustPadding)
		{
			if (_background == background)
				return this;

			_background = background;
			if (adjustPadding)
			{
				if (background == null)
					SetPad(Value.Zero);
				else
					SetPad(background.TopHeight, background.LeftWidth, background.BottomHeight, background.RightWidth);
				Invalidate();
			}

			return this;
		}


		public IDrawable GetBackground()
		{
			return _background;
		}


		public override void Layout()
		{
			if (_element == null)
				return;

			float padLeft = _padLeft.Get(this), padBottom = _padBottom.Get(this), padTop = _padTop.Get(this);
			float containerWidth = GetWidth() - padLeft - _padRight.Get(this);
			float containerHeight = GetHeight() - padBottom - padTop;
			float minWidth = _minWidthValue.Get(_element), minHeight = _minHeightValue.Get(_element);
			float prefWidth = _prefWidthValue.Get(_element), prefHeight = _prefHeightValue.Get(_element);
			float maxWidth = _maxWidthValue.Get(_element), maxHeight = _maxHeightValue.Get(_element);

			float width;
			if (_fillX > 0)
				width = containerWidth * _fillX;
			else
				width = Math.Min(prefWidth, containerWidth);
			if (width < minWidth)
				width = minWidth;
			if (maxWidth > 0 && width > maxWidth)
				width = maxWidth;

			float height;
			if (_fillY > 0)
				height = containerHeight * _fillY;
			else
				height = Math.Min(prefHeight, containerHeight);

			if (height < minHeight)
				height = minHeight;
			if (maxHeight > 0 && height > maxHeight)
				height = maxHeight;

			var x = padLeft;
			if ((_align & AlignInternal.Right) != 0)
				x += containerWidth - width;
			else if ((_align & AlignInternal.Left) == 0) // center
				x += (containerWidth - width) / 2;

			var y = padTop;
			if ((_align & AlignInternal.Bottom) != 0)
				y += containerHeight - height;
			else if ((_align & AlignInternal.Top) == 0) // center
				y += (containerHeight - height) / 2;

			if (_round)
			{
				x = Mathf.Round(x);
				y = Mathf.Round(y);
				width = Mathf.Round(width);
				height = Mathf.Round(height);
			}

			_element.SetBounds(x, y, width, height);
			if (_element is ILayout)
				((ILayout) _element).Validate();
		}


		/// <summary>
		/// element may be null
		/// </summary>
		/// <returns>The element.</returns>
		/// <param name="element">element.</param>
		public virtual Element SetElement(Element element)
		{
			if (element == this)
				throw new Exception("element cannot be the Container.");

			if (element == _element)
				return element;

			if (_element != null)
				base.RemoveElement(_element);
			_element = element;
			if (element != null)
				return AddElement(element);

			return null;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The element.</returns>
		public T GetElement<T>() where T : Element
		{
			return _element as T;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The element.</returns>
		public Element GetElement()
		{
			return _element;
		}


		public override bool RemoveElement(Element element)
		{
			if (element != _element)
				return false;

			SetElement(null);
			return true;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value
		/// </summary>
		/// <param name="size">Size.</param>
		public Container SetSize(Value size)
		{
			if (size == null)
				throw new Exception("size cannot be null.");

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
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container SetSize(Value width, Value height)
		{
			if (width == null)
				throw new Exception("width cannot be null.");
			if (height == null)
				throw new Exception("height cannot be null.");

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
		/// <param name="size">Size.</param>
		public Container SetSize(float size)
		{
			SetSize(new Value.Fixed(size));
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public new Container SetSize(float width, float height)
		{
			SetSize(new Value.Fixed(width), new Value.Fixed(height));
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, and maxWidth to the specified value
		/// </summary>
		/// <param name="width">Width.</param>
		public Container SetWidth(Value width)
		{
			if (width == null)
				throw new Exception("width cannot be null.");

			_minWidthValue = width;
			_prefWidthValue = width;
			_maxWidthValue = width;
			return this;
		}


		/// <summary>
		/// Sets the minWidth, prefWidth, and maxWidth to the specified value
		/// </summary>
		/// <param name="width">Width.</param>
		public new Container SetWidth(float width)
		{
			SetWidth(new Value.Fixed(width));
			return this;
		}


		/// <summary>
		/// Sets the minHeight, prefHeight, and maxHeight to the specified value.
		/// </summary>
		/// <param name="height">Height.</param>
		public Container SetHeight(Value height)
		{
			if (height == null)
				throw new Exception("height cannot be null.");

			_minHeightValue = height;
			_prefHeightValue = height;
			_maxHeightValue = height;
			return this;
		}


		/// <summary>
		/// Sets the minHeight, prefHeight, and maxHeight to the specified value
		/// </summary>
		/// <param name="height">Height.</param>
		public new Container SetHeight(float height)
		{
			SetHeight(new Value.Fixed(height));
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified value
		/// </summary>
		/// <param name="size">Size.</param>
		public Container SetMinSize(Value size)
		{
			if (size == null)
				throw new Exception("size cannot be null.");

			_minWidthValue = size;
			_minHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the minimum size.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container SetMinSize(Value width, Value height)
		{
			if (width == null)
				throw new Exception("width cannot be null.");
			if (height == null)
				throw new Exception("height cannot be null.");

			_minWidthValue = width;
			_minHeightValue = height;
			return this;
		}


		public Container SetMinWidth(Value minWidth)
		{
			if (minWidth == null)
				throw new Exception("minWidth cannot be null.");

			_minWidthValue = minWidth;
			return this;
		}


		public Container SetMinHeight(Value minHeight)
		{
			if (minHeight == null)
				throw new Exception("minHeight cannot be null.");

			_minHeightValue = minHeight;
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified value
		/// </summary>
		/// <param name="size">Size.</param>
		public Container SetMinSize(float size)
		{
			SetMinSize(new Value.Fixed(size));
			return this;
		}


		/// <summary>
		/// Sets the minWidth and minHeight to the specified values
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container SetMinSize(float width, float height)
		{
			SetMinSize(new Value.Fixed(width), new Value.Fixed(height));
			return this;
		}


		public Container SetMinWidth(float minWidth)
		{
			_minWidthValue = new Value.Fixed(minWidth);
			return this;
		}


		public Container SetMinHeight(float minHeight)
		{
			_minHeightValue = new Value.Fixed(minHeight);
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified value.
		/// </summary>
		/// <param name="size">Size.</param>
		public Container PrefSize(Value size)
		{
			if (size == null)
				throw new Exception("size cannot be null.");

			_prefWidthValue = size;
			_prefHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified values.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container PrefSize(Value width, Value height)
		{
			if (width == null)
				throw new Exception("width cannot be null.");
			if (height == null)
				throw new Exception("height cannot be null.");

			_prefWidthValue = width;
			_prefHeightValue = height;
			return this;
		}


		public Container SetPrefWidth(Value prefWidth)
		{
			if (prefWidth == null)
				throw new Exception("prefWidth cannot be null.");

			_prefWidthValue = prefWidth;
			return this;
		}


		public Container SetPrefHeight(Value prefHeight)
		{
			if (prefHeight == null)
				throw new Exception("prefHeight cannot be null.");

			_prefHeightValue = prefHeight;
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified value.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container SetPrefSize(float width, float height)
		{
			PrefSize(new Value.Fixed(width), new Value.Fixed(height));
			return this;
		}


		/// <summary>
		/// Sets the prefWidth and prefHeight to the specified values
		/// </summary>
		/// <param name="size">Size.</param>
		public Container SetPrefSize(float size)
		{
			PrefSize(new Value.Fixed(size));
			return this;
		}


		public Container SetPrefWidth(float prefWidth)
		{
			_prefWidthValue = new Value.Fixed(prefWidth);
			return this;
		}


		public Container SetPrefHeight(float prefHeight)
		{
			_prefHeightValue = new Value.Fixed(prefHeight);
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified value.
		/// </summary>
		/// <param name="size">Size.</param>
		public Container SetMaxSize(Value size)
		{
			if (size == null)
				throw new Exception("size cannot be null.");

			_maxWidthValue = size;
			_maxHeightValue = size;
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified values
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container SetMaxSize(Value width, Value height)
		{
			if (width == null)
				throw new Exception("width cannot be null.");
			if (height == null)
				throw new Exception("height cannot be null.");

			_maxWidthValue = width;
			_maxHeightValue = height;
			return this;
		}


		public Container SetMaxWidth(Value maxWidth)
		{
			if (maxWidth == null)
				throw new Exception("maxWidth cannot be null.");

			_maxWidthValue = maxWidth;
			return this;
		}


		public Container SetMaxHeight(Value maxHeight)
		{
			if (maxHeight == null)
				throw new Exception("maxHeight cannot be null.");

			_maxHeightValue = maxHeight;
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified value
		/// </summary>
		/// <param name="size">Size.</param>
		public Container SetMaxSize(float size)
		{
			SetMaxSize(new Value.Fixed(size));
			return this;
		}


		/// <summary>
		/// Sets the maxWidth and maxHeight to the specified values
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Container SetMaxSize(float width, float height)
		{
			SetMaxSize(new Value.Fixed(width), new Value.Fixed(height));
			return this;
		}


		public Container SetMaxWidth(float maxWidth)
		{
			_maxWidthValue = new Value.Fixed(maxWidth);
			return this;
		}


		public Container SetMaxHeight(float maxHeight)
		{
			_maxHeightValue = new Value.Fixed(maxHeight);
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
		/// </summary>
		/// <param name="pad">Pad.</param>
		public Container SetPad(Value pad)
		{
			if (pad == null)
				throw new Exception("pad cannot be null.");

			_padTop = pad;
			_padLeft = pad;
			_padBottom = pad;
			_padRight = pad;
			return this;
		}


		public Container SetPad(Value top, Value left, Value bottom, Value right)
		{
			if (top == null)
				throw new Exception("top cannot be null.");
			if (left == null)
				throw new Exception("left cannot be null.");
			if (bottom == null)
				throw new Exception("bottom cannot be null.");
			if (right == null)
				throw new Exception("right cannot be null.");

			_padTop = top;
			_padLeft = left;
			_padBottom = bottom;
			_padRight = right;
			return this;
		}


		public Container SetPadTop(Value padTop)
		{
			if (padTop == null)
				throw new Exception("padTop cannot be null.");

			_padTop = padTop;
			return this;
		}


		public Container SetPadLeft(Value padLeft)
		{
			if (padLeft == null)
				throw new Exception("padLeft cannot be null.");

			_padLeft = padLeft;
			return this;
		}


		public Container SetPadBottom(Value padBottom)
		{
			if (padBottom == null)
				throw new Exception("padBottom cannot be null.");

			_padBottom = padBottom;
			return this;
		}


		public Container SetPadRight(Value padRight)
		{
			if (padRight == null)
				throw new Exception("padRight cannot be null.");

			_padRight = padRight;
			return this;
		}


		/// <summary>
		/// Sets the padTop, padLeft, padBottom, and padRight to the specified value
		/// </summary>
		/// <param name="pad">Pad.</param>
		public Container SetPad(float pad)
		{
			Value value = new Value.Fixed(pad);
			_padTop = value;
			_padLeft = value;
			_padBottom = value;
			_padRight = value;
			return this;
		}


		public Container SetPad(float top, float left, float bottom, float right)
		{
			_padTop = new Value.Fixed(top);
			_padLeft = new Value.Fixed(left);
			_padBottom = new Value.Fixed(bottom);
			_padRight = new Value.Fixed(right);
			return this;
		}


		public Container SetPadTop(float padTop)
		{
			_padTop = new Value.Fixed(padTop);
			return this;
		}


		public Container SetPadLeft(float padLeft)
		{
			_padLeft = new Value.Fixed(padLeft);
			return this;
		}


		public Container SetPadBottom(float padBottom)
		{
			_padBottom = new Value.Fixed(padBottom);
			return this;
		}


		public Container SetPadRight(float padRight)
		{
			_padRight = new Value.Fixed(padRight);
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1
		/// </summary>
		public Container SetFill()
		{
			_fillX = 1f;
			_fillY = 1f;
			return this;
		}


		/// <summary>
		/// Sets fillX to 1
		/// </summary>
		public Container SetFillX()
		{
			_fillX = 1f;
			return this;
		}


		/// <summary>
		/// Sets fillY to 1
		/// </summary>
		public Container SetFillY()
		{
			_fillY = 1f;
			return this;
		}


		public Container SetFill(float x, float y)
		{
			_fillX = x;
			_fillY = y;
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1 if true, 0 if false
		/// </summary>
		/// <param name="x">If set to <c>true</c> x.</param>
		/// <param name="y">If set to <c>true</c> y.</param>
		public Container SetFill(bool x, bool y)
		{
			_fillX = x ? 1f : 0;
			_fillY = y ? 1f : 0;
			return this;
		}


		/// <summary>
		/// Sets fillX and fillY to 1 if true, 0 if false
		/// </summary>
		/// <param name="fill">If set to <c>true</c> fill.</param>
		public Container SetFill(bool fill)
		{
			_fillX = fill ? 1f : 0;
			_fillY = fill ? 1f : 0;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the element within the container. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom},
		/// {@link Align#left}, {@link Align#right}, or any combination of those.
		/// </summary>
		/// <param name="align">Align.</param>
		public Container SetAlign(Align align)
		{
			_align = (int) align;
			return this;
		}


		/// <summary>
		/// Sets the alignment of the element within the container to {@link Align#center}. This clears any other alignment.
		/// </summary>
		public Container SetAlignCenter()
		{
			_align = AlignInternal.Center;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#top} and clears {@link Align#bottom} for the alignment of the element within the container.
		/// </summary>
		public Container SetTop()
		{
			_align |= AlignInternal.Top;
			_align &= ~AlignInternal.Bottom;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#left} and clears {@link Align#right} for the alignment of the element within the container.
		/// </summary>
		public Container SetLeft()
		{
			_align |= AlignInternal.Left;
			_align &= ~AlignInternal.Right;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#bottom} and clears {@link Align#top} for the alignment of the element within the container.
		/// </summary>
		public Container SetBottom()
		{
			_align |= AlignInternal.Bottom;
			_align &= ~AlignInternal.Top;
			return this;
		}


		/// <summary>
		/// Sets {@link Align#right} and clears {@link Align#left} for the alignment of the element within the container.
		/// </summary>
		public Container SetRight()
		{
			_align |= AlignInternal.Right;
			_align &= ~AlignInternal.Left;
			return this;
		}


		public float GetMinWidth()
		{
			return _minWidthValue.Get(_element) + _padLeft.Get(this) + _padRight.Get(this);
		}


		public Value GetMinHeightValue()
		{
			return _minHeightValue;
		}


		public float GetMinHeight()
		{
			return _minHeightValue.Get(_element) + _padTop.Get(this) + _padBottom.Get(this);
		}


		public Value GetPrefWidthValue()
		{
			return _prefWidthValue;
		}


		public float GetPrefWidth()
		{
			float v = _prefWidthValue.Get(_element);
			if (_background != null)
				v = Math.Max(v, _background.MinWidth);
			return Math.Max(GetMinWidth(), v + _padLeft.Get(this) + _padRight.Get(this));
		}


		public Value GetPrefHeightValue()
		{
			return _prefHeightValue;
		}


		public float GetPrefHeight()
		{
			float v = _prefHeightValue.Get(_element);
			if (_background != null)
				v = Math.Max(v, _background.MinHeight);
			return Math.Max(GetMinHeight(), v + _padTop.Get(this) + _padBottom.Get(this));
		}


		public Value GetMaxWidthValue()
		{
			return _maxWidthValue;
		}


		public float GetMaxWidth()
		{
			float v = _maxWidthValue.Get(_element);
			if (v > 0)
				v += _padLeft.Get(this) + _padRight.Get(this);
			return v;
		}


		public Value GetMaxHeightValue()
		{
			return _maxHeightValue;
		}


		public float GetMaxHeight()
		{
			float v = _maxHeightValue.Get(_element);
			if (v > 0)
				v += _padTop.Get(this) + _padBottom.Get(this);
			return v;
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad top value.</returns>
		public Value GetPadTopValue()
		{
			return _padTop;
		}


		public float GetPadTop()
		{
			return _padTop.Get(this);
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad left value.</returns>
		public Value GetPadLeftValue()
		{
			return _padLeft;
		}


		public float GetPadLeft()
		{
			return _padLeft.Get(this);
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad bottom value.</returns>
		public Value GetPadBottomValue()
		{
			return _padBottom;
		}


		public float GetPadBottom()
		{
			return _padBottom.Get(this);
		}


		/// <summary>
		/// May be null if this value is not set
		/// </summary>
		/// <returns>The pad right value.</returns>
		public Value GetPadRightValue()
		{
			return _padRight;
		}


		public float GetPadRight()
		{
			return _padRight.Get(this);
		}


		/// <summary>
		/// Returns {@link #getPadLeft()} plus {@link #getPadRight()}.
		/// </summary>
		/// <returns>The pad x.</returns>
		public float GetPadX()
		{
			return _padLeft.Get(this) + _padRight.Get(this);
		}


		/// <summary>
		/// Returns {@link #getPadTop()} plus {@link #getPadBottom()}
		/// </summary>
		/// <returns>The pad y.</returns>
		public float GetPadY()
		{
			return _padTop.Get(this) + _padBottom.Get(this);
		}


		public float GetFillX()
		{
			return _fillX;
		}


		public float GetFillY()
		{
			return _fillY;
		}


		public int GetAlign()
		{
			return _align;
		}


		/// <summary>
		/// If true (the default), positions and sizes are rounded to integers
		/// </summary>
		/// <param name="round">If set to <c>true</c> round.</param>
		public void SetRound(bool round)
		{
			_round = round;
		}


		/// <summary>
		/// Causes the contents to be clipped if they exceed the container bounds. Enabling clipping will set
		/// {@link #setTransform(bool)} to true
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		public void SetClip(bool enabled)
		{
			_clip = enabled;
			transform = enabled;
			Invalidate();
		}


		public bool GetClip()
		{
			return _clip;
		}


		public override Element Hit(Vector2 point)
		{
			if (_clip)
			{
				if (GetTouchable() == Touchable.Disabled)
					return null;
				if (point.X < 0 || point.X >= GetWidth() || point.Y < 0 || point.Y >= GetHeight())
					return null;
			}

			return base.Hit(point);
		}


		public override void DebugRender(Batcher batcher)
		{
			Validate();
			if (transform)
			{
				ApplyTransform(batcher, ComputeTransform());
				if (_clip)
				{
					//shapes.flush();
					//float padLeft = this.padLeft.get( this ), padBottom = this.padBottom.get( this );
					//bool draw = background == null ? clipBegin( 0, 0, getWidth(), getHeight() ) : clipBegin( padLeft, padBottom,
					//	            getWidth() - padLeft - padRight.get( this ), getHeight() - padBottom - padTop.get( this ) );
					var draw = true;
					if (draw)
					{
						DebugRenderChildren(batcher, 1f);

						//clipEnd();
					}
				}
				else
				{
					DebugRenderChildren(batcher, 1f);
				}

				ResetTransform(batcher);
			}
			else
			{
				base.DebugRender(batcher);
			}
		}
	}
}