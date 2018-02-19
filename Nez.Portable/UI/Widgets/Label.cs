using System;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	public class Label : Element
	{
		public override float preferredWidth
		{
			get
			{
				if( _wrapText )
					return 0;

				if( _prefSizeInvalid )
					computePrefSize();

				var w = _prefSize.X;
				if( _style.background != null )
					w += _style.background.leftWidth + _style.background.rightWidth;
				return w;
			}
		}

		public override float preferredHeight
		{
			get
			{
				if( _prefSizeInvalid )
					computePrefSize();

				var h = _prefSize.Y;
				if( _style.background != null )
					h += _style.background.topHeight + _style.background.bottomHeight;
				return h;
			}
		}


		// configuration
		LabelStyle _style;
		string _text;
		float _fontScaleX = 1;
		float _fontScaleY = 1;
		int labelAlign = AlignInternal.left;
		//int lineAlign = AlignInternal.left;
		string _ellipsis;
		bool _wrapText;

		// internal state
		string _wrappedString;
		bool _prefSizeInvalid;
		float _lastPrefHeight;
		Vector2 _prefSize;
		Vector2 _textPosition;


		public Label( string text, LabelStyle style )
		{
			setStyle( style );
			setText( text );
			touchable = Touchable.Disabled;
		}


		public Label( string text, Skin skin, string styleName = null ) : this( text, skin.get<LabelStyle>( styleName ) )
		{}


		public Label( string text, BitmapFont font, Color fontColor ) : this( text, new LabelStyle( font, fontColor ) )
		{}


		public Label( string text, BitmapFont font ) : this( text, font, Color.White )
		{}


		public Label( string text ) : this( text, Graphics.instance.bitmapFont )
		{}


		public virtual Label setStyle( LabelStyle style )
		{
			_style = style;
			invalidateHierarchy();
			return this;
		}


		/// <summary>
		/// Returns the button's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public virtual LabelStyle getStyle()
		{
			return _style;
		}


		public override void invalidate()
		{
			base.invalidate();
			_prefSizeInvalid = true;
		}


		void computePrefSize()
		{
			_prefSizeInvalid = false;

			if( _wrapText && _ellipsis == null && width > 0 )
			{
				var widthCalc = width;
				if( _style.background != null )
					widthCalc -= _style.background.leftWidth + _style.background.rightWidth;

				_wrappedString = _style.font.wrapText( _text, widthCalc / _fontScaleX );
			}
			else if( _ellipsis != null && width > 0 )
			{
				// we have a max width and an ellipsis so we will truncate the text
				var widthCalc = width;
				if( _style.background != null )
					widthCalc -= _style.background.leftWidth + _style.background.rightWidth;
				
				_wrappedString = _style.font.truncateText( _text, _ellipsis, widthCalc / _fontScaleX );
			}
			else
			{
				_wrappedString = _text;
			}

			_prefSize = _style.font.measureString( _wrappedString ) * new Vector2( _fontScaleX, _fontScaleY );
		}


		#region Configuration

		public Label setText( string text )
		{
			if( _text != text )
			{
				_wrappedString = null;
				_text = text;
				_prefSizeInvalid = true;
				invalidateHierarchy();
			}
			return this;
		}


		public string getText()
		{
			return _text;
		}


		/// <summary>
		/// background may be null to clear the background.
		/// </summary>
		/// <returns>this</returns>
		/// <param name="background">Background.</param>
		public Label setBackground( IDrawable background )
		{
			_style.background = background;
			invalidate();
			return this;
		}


		/// <summary>
		/// alignment Aligns all the text within the label (default left center) and each line of text horizontally (default left)
		/// </summary>
		/// <param name="alignment">Alignment.</param>
		public Label setAlignment( Align alignment )
		{
			return setAlignment( alignment, alignment );
		}


		/// <summary>
		/// labelAlign Aligns all the text within the label (default left center).
		/// lineAlign Aligns each line of text horizontally (default left).
		/// </summary>
		/// <param name="labelAlign">Label align.</param>
		/// <param name="lineAlign">Line align.</param>
		public Label setAlignment( Align labelAlign, Align lineAlign )
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

			invalidate();
			return this;
		}


		public Label setFontColor( Color color )
		{
			_style.fontColor = color;
			return this;
		}


		public Label setFontScale( float fontScale )
		{
			_fontScaleX = fontScale;
			_fontScaleY = fontScale;
			invalidateHierarchy();
			return this;
		}


		public Label setFontScale( float fontScaleX, float fontScaleY )
		{
			_fontScaleX = fontScaleX;
			_fontScaleY = fontScaleY;
			invalidateHierarchy();
			return this;
		}


		/// <summary>
		/// When non-null the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not occur
		/// when ellipsis is enabled. Default is null.
		/// </summary>
		/// <param name="ellipsis">Ellipsis.</param>
		public Label setEllipsis( string ellipsis )
		{
			_ellipsis = ellipsis;
			return this;
		}


		/// <summary>
		/// When true the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not occur when
		/// ellipsis is true. Default is false.
		/// </summary>
		/// <param name="ellipsis">Ellipsis.</param>
		public Label setEllipsis( bool ellipsis )
		{
			if( ellipsis )
				_ellipsis = "...";
			else
				_ellipsis = null;
			return this;
		}


		/// <summary>
		/// should the text be wrapped?
		/// </summary>
		/// <param name="shouldWrap">If set to <c>true</c> should wrap.</param>
		public Label setWrap( bool shouldWrap )
		{
			_wrapText = shouldWrap;
			invalidateHierarchy();
			return this;
		}

		#endregion


		public override void layout()
		{
			if( _prefSizeInvalid )
				computePrefSize();

			var isWrapped = _wrapText && _ellipsis == null;
			if( isWrapped )
			{
				if( _lastPrefHeight != preferredHeight )
				{
					_lastPrefHeight = preferredHeight;
					invalidateHierarchy();
				}
			}

			var width = this.width;
			var height = this.height;
			_textPosition.X = 0;
			_textPosition.Y = 0;
			// TODO: explore why descent causes mis-alignment
			//_textPosition.Y =_style.font.descent;
			if( _style.background != null )
			{
				_textPosition.X = _style.background.leftWidth;
				_textPosition.Y = _style.background.topHeight;
				width -= _style.background.leftWidth + _style.background.rightWidth;
				height -= _style.background.topHeight + _style.background.bottomHeight;
			}

			float textWidth, textHeight;
			if( isWrapped || _wrappedString.IndexOf( '\n' ) != -1 )
			{
				// If the text can span multiple lines, determine the text's actual size so it can be aligned within the label.
				textWidth = _prefSize.X;
				textHeight = _prefSize.Y;

				if( ( labelAlign & AlignInternal.left ) == 0 )
				{
					if( ( labelAlign & AlignInternal.right ) != 0 )
						_textPosition.X += width - textWidth;
					else
						_textPosition.X += ( width - textWidth ) / 2;
				}
			}
			else
			{
				textWidth = width;
				textHeight = _style.font.lineHeight * _fontScaleY;
			}
				
			if( ( labelAlign & AlignInternal.bottom ) != 0 )
			{
				_textPosition.Y += height - textHeight;
				y += _style.font.descent;
			}
			else if( ( labelAlign & AlignInternal.top ) != 0 )
			{
				_textPosition.Y += 0;
				y -= _style.font.descent;
			}
			else
			{
				_textPosition.Y += ( height - textHeight ) / 2;
			}

			//_textPosition.Y += textHeight;

			// if we have GlyphLayout this code is redundant
			if( ( labelAlign & AlignInternal.left ) != 0 )
				_textPosition.X = 0;
			else if( labelAlign == AlignInternal.center )
				_textPosition.X = width / 2 - ( _prefSize.X / 2 ); // center of width - center of text size
			else
				_textPosition.X = width - _prefSize.X; // full width - our text size
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			validate();

			var color = new Color( this.color, (int)(this.color.A * parentAlpha) );
			if( _style.background != null )
				_style.background.draw( graphics, x, y, width == 0 ? _prefSize.X : width, height, color );

			graphics.batcher.drawString( _style.font, _wrappedString, new Vector2( x, y ) + _textPosition, _style.fontColor, 0, Vector2.Zero, new Vector2( _fontScaleX, _fontScaleY ), SpriteEffects.None, 0 );
		}

	}


	/// <summary>
	/// the style for a label
	/// </summary>
	public class LabelStyle
	{
		public Color fontColor = Color.White;
		public BitmapFont font;
		public IDrawable background;


		public LabelStyle()
		{
			font = Graphics.instance.bitmapFont;
		}


		public LabelStyle( BitmapFont font, Color fontColor )
		{
			this.font = font ?? Graphics.instance.bitmapFont;
			this.fontColor = fontColor;
		}


		public LabelStyle( Color fontColor ) : this( null, fontColor )
		{}


		public LabelStyle clone()
		{
			return new LabelStyle {
				fontColor = fontColor,
				font = font,
				background = background
			};
		}
	}
}

