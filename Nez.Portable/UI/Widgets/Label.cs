using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	public class Label : Element
	{
		public override float PreferredWidth
		{
			get
			{
				if (_wrapText)
					return 0;

				if (_prefSizeInvalid)
					ComputePrefSize();

				var w = _prefSize.X;
				if (_style.Background != null)
					w += _style.Background.LeftWidth + _style.Background.RightWidth;
				return w;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				if (_prefSizeInvalid)
					ComputePrefSize();

				var h = _prefSize.Y;
				if (_style.Background != null)
					h += _style.Background.TopHeight + _style.Background.BottomHeight;
				return h;
			}
		}


		// configuration
		LabelStyle _style;
		string _text;

		int labelAlign = AlignInternal.Left;

		//int lineAlign = AlignInternal.left;
		string _ellipsis;
		bool _wrapText;

		// internal state
		string _wrappedString;
		bool _prefSizeInvalid;
		float _lastPrefHeight;
		Vector2 _prefSize;
		Vector2 _textPosition;


		public Label(string text, LabelStyle style)
		{
			SetStyle(style);
			SetText(text);
			touchable = Touchable.Disabled;
		}


		public Label(string text, Skin skin, string styleName = null) : this(text, skin.Get<LabelStyle>(styleName))
		{ }


		public Label(string text, BitmapFont font, Color fontColor) : this(text, new LabelStyle(font, fontColor))
		{ }


		public Label(string text, BitmapFont font, Color fontColor, float fontScale) : this(text, font, fontColor, fontScale, fontScale)
        { }


        public Label(string text, BitmapFont font, Color fontColor, float fontScaleX, float fontScaleY) : this(text, new LabelStyle(font, fontColor, fontScaleX, fontScaleY))
        { }



		public Label(string text, BitmapFont font) : this(text, font, Color.White)
		{ }


		public Label(string text) : this(text, Graphics.Instance.BitmapFont)
		{ }


		public virtual Label SetStyle(LabelStyle style)
		{
			_style = style;
			InvalidateHierarchy();
			return this;
		}


		/// <summary>
		/// Returns the button's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public virtual LabelStyle GetStyle()
		{
			return _style;
		}


		public override void Invalidate()
		{
			base.Invalidate();
			_prefSizeInvalid = true;
		}


		void ComputePrefSize()
		{
			_prefSizeInvalid = false;

			if (_wrapText && _ellipsis == null && width > 0)
			{
				var widthCalc = width;
				if (_style.Background != null)
					widthCalc -= _style.Background.LeftWidth + _style.Background.RightWidth;

				_wrappedString = _style.Font.WrapText(_text, widthCalc / _style.FontScaleX);
			}
			else if (_ellipsis != null && width > 0)
			{
				// we have a max width and an ellipsis so we will truncate the text
				var widthCalc = width;
				if (_style.Background != null)
					widthCalc -= _style.Background.LeftWidth + _style.Background.RightWidth;

				_wrappedString = _style.Font.TruncateText(_text, _ellipsis, widthCalc / _style.FontScaleX);
			}
			else
			{
				_wrappedString = _text;
			}

			_prefSize = _style.Font.MeasureString(_wrappedString) * new Vector2(_style.FontScaleX, _style.FontScaleY);
		}


		#region Configuration

		public Label SetText(string text)
		{
			if (_text != text)
			{
				_wrappedString = null;
				_text = text;
				_prefSizeInvalid = true;
				InvalidateHierarchy();
			}

			return this;
		}


		public string GetText()
		{
			return _text;
		}


		/// <summary>
		/// background may be null to clear the background.
		/// </summary>
		/// <returns>this</returns>
		/// <param name="background">Background.</param>
		public Label SetBackground(IDrawable background)
		{
			_style.Background = background;
			Invalidate();
			return this;
		}


		/// <summary>
		/// alignment Aligns all the text within the label (default left center) and each line of text horizontally (default left)
		/// </summary>
		/// <param name="alignment">Alignment.</param>
		public Label SetAlignment(Align alignment)
		{
			return SetAlignment(alignment, alignment);
		}


		/// <summary>
		/// labelAlign Aligns all the text within the label (default left center).
		/// lineAlign Aligns each line of text horizontally (default left).
		/// </summary>
		/// <param name="labelAlign">Label align.</param>
		/// <param name="lineAlign">Line align.</param>
		public Label SetAlignment(Align labelAlign, Align lineAlign)
		{
			this.labelAlign = (int)labelAlign;

			// TODO
			//			var tempLineAlign = (int)lineAlign;
			//			if( ( tempLineAlign & AlignInternal.left ) != 0 )
			//				this.lineAlign = AlignInternal.left;
			//			else if( ( tempLineAlign & AlignInternal.right ) != 0 )
			//				this.lineAlign = AlignInternal.right;
			//			else
			//				this.lineAlign = AlignInternal.center;

			Invalidate();
			return this;
		}


		public Label SetFontColor(Color color)
		{
			_style.FontColor = color;
			return this;
		}


		public Label SetFontScale(float fontScale)
		{
			_style.FontScaleX = fontScale;
			_style.FontScaleY = fontScale;
			InvalidateHierarchy();
			return this;
		}


		public Label SetFontScale(float fontScaleX, float fontScaleY)
		{
			_style.FontScaleX = fontScaleX;
			_style.FontScaleY = fontScaleY;
			InvalidateHierarchy();
			return this;
		}


		/// <summary>
		/// When non-null the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not occur
		/// when ellipsis is enabled. Default is null.
		/// </summary>
		/// <param name="ellipsis">Ellipsis.</param>
		public Label SetEllipsis(string ellipsis)
		{
			_ellipsis = ellipsis;
			return this;
		}


		/// <summary>
		/// When true the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not occur when
		/// ellipsis is true. Default is false.
		/// </summary>
		/// <param name="ellipsis">Ellipsis.</param>
		public Label SetEllipsis(bool ellipsis)
		{
			if (ellipsis)
				_ellipsis = "...";
			else
				_ellipsis = null;
			return this;
		}


		/// <summary>
		/// should the text be wrapped?
		/// </summary>
		/// <param name="shouldWrap">If set to <c>true</c> should wrap.</param>
		public Label SetWrap(bool shouldWrap)
		{
			_wrapText = shouldWrap;
			InvalidateHierarchy();
			return this;
		}

		#endregion


		public override void Layout()
		{
			if (_prefSizeInvalid)
				ComputePrefSize();

			var isWrapped = _wrapText && _ellipsis == null;
			if (isWrapped)
			{
				if (_lastPrefHeight != PreferredHeight)
				{
					_lastPrefHeight = PreferredHeight;
					InvalidateHierarchy();
				}
			}

			float paddingLeft = 0;
			float paddingRight = 0;
			float paddingTop = 0;
			float paddingBottom = 0;

			if (_style.Background != null)
			{
				paddingLeft = _style.Background.LeftWidth;
				paddingRight = _style.Background.RightWidth;
				paddingTop = _style.Background.TopHeight;
				paddingBottom = _style.Background.BottomHeight;
			}
			
			// TODO: explore why descent causes mis-alignment
			//_textPosition.Y =_style.font.descent;

			float textWidth, textHeight;
			if (isWrapped || _wrappedString.IndexOf('\n') != -1)
			{
				// If the text can span multiple lines, determine the text's actual size so it can be aligned within the label.
				textWidth = _prefSize.X;
				textHeight = _prefSize.Y;
			}
			else
			{
				textWidth = width - (paddingLeft + paddingRight);
				textHeight = _style.Font.LineHeight * _style.FontScaleY;
			}
			
			_textPosition.Y = 0;

			// Bottom | BottomLeft | BottomRight
			if ((labelAlign & AlignInternal.Bottom) != 0)
			{
				_textPosition.Y = height - paddingBottom - textHeight;
				y += _style.Font.Padding.Bottom;
			}
			// Top | TopLeft | TopRight
			else if ((labelAlign & AlignInternal.Top) != 0)
			{
				_textPosition.Y = paddingTop;
				y -= _style.Font.Padding.Bottom;
			}
			// Center | Left | Right
			else
			{
				_textPosition.Y = paddingTop + (height - (paddingTop + paddingBottom) - textHeight) / 2;
			}

			_textPosition.X = 0;

			// Left | TopLeft | BottomLeft
			if ((labelAlign & AlignInternal.Left) != 0)
				_textPosition.X = paddingLeft;
			// Right | TopRight | BottomRight
			else if ((labelAlign & AlignInternal.Right) != 0)
				_textPosition.X = width - paddingRight - textWidth;
			// Center | Top | Bottom
			else
				_textPosition.X = paddingLeft + (width - (paddingLeft + paddingRight) - textWidth) / 2;
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			Validate();

			var color = ColorExt.Create(this.color, (int)(this.color.A * parentAlpha));
			_style.Background?.Draw(batcher, x, y, width == 0 ? _prefSize.X : width, height, color);

			batcher.DrawString(_style.Font, _wrappedString, new Vector2(x, y) + _textPosition,
				_style.FontColor, 0, Vector2.Zero, new Vector2(_style.FontScaleX, _style.FontScaleY), SpriteEffects.None, 0);
		}
	}


	/// <summary>
	/// the style for a label
	/// </summary>
	public class LabelStyle
	{
		public Color FontColor = Color.White;
		public BitmapFont Font;
		public IDrawable Background;
		public float FontScaleX = 1f;
		public float FontScaleY = 1f;
		public float FontScale { set { FontScaleX = value; FontScaleY = value; } }


		public LabelStyle()
		{
			Font = Graphics.Instance.BitmapFont;
		}


		public LabelStyle(BitmapFont font, Color fontColor) : this(font, fontColor, 1f)
        { }


        public LabelStyle(BitmapFont font, Color fontColor, float fontScaleX, float fontScaleY)
		{
			Font = font ?? Graphics.Instance.BitmapFont;
			FontColor = fontColor;
			FontScaleX = fontScaleX;
			FontScaleY = fontScaleY;
		}


		public LabelStyle(BitmapFont font, Color fontColor, float fontScale) : this(font, fontColor, fontScale, fontScale)
        {

        }


		public LabelStyle(Color fontColor) : this(null, fontColor)
		{ }


		public LabelStyle Clone()
		{
			return new LabelStyle
			{
				FontColor = FontColor,
				Font = Font,
				Background = Background,
				FontScaleX = FontScaleX,
				FontScaleY = FontScaleY
			};
		}
	}
}