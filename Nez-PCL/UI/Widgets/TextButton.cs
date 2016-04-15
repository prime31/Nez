using System;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class TextButton : Button
	{
		Label label;
		TextButtonStyle style;


		public TextButton( string text, TextButtonStyle style ) : base( style )
		{
			setStyle( style );
			label = new Label( text, style.font, style.fontColor );
			label.setAlignment( Align.center );

			add( label ).expand().fill();
			setSize( preferredWidth, preferredHeight );
		}


		public TextButton( string text, Skin skin, string styleName = null ) : this( text, skin.get<TextButtonStyle>( styleName ) )
		{}


		public override void setStyle( ButtonStyle style )
		{
			Assert.isTrue( style is TextButtonStyle, "style must be a TextButtonStyle" );

			base.setStyle( style );
			this.style = (TextButtonStyle)style;

			if( label != null )
			{
				var textButtonStyle = (TextButtonStyle)style;
				var labelStyle = label.getStyle();
				labelStyle.font = textButtonStyle.font;
				labelStyle.fontColor = textButtonStyle.fontColor;
				label.setStyle( labelStyle );
			}
		}


		public new TextButtonStyle getStyle()
		{
			return style;
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			Color? fontColor = null;
			if( _isDisabled && style.disabledFontColor.HasValue )
				fontColor = style.disabledFontColor;
			else if( _mouseDown && style.downFontColor.HasValue )
				fontColor = style.downFontColor;
			else if( isChecked &&
				( !_mouseOver && style.checkedFontColor.HasValue || _mouseOver && style.checkedOverFontColor.HasValue ) )
				fontColor = ( _mouseOver && style.checkedOverFontColor.HasValue ) ? style.checkedOverFontColor : style.checkedFontColor;
			else if( _mouseOver && style.overFontColor.HasValue )
				fontColor = style.overFontColor;
			else
				fontColor = style.fontColor;
			
			if( fontColor != null )
				label.getStyle().fontColor = fontColor.Value;
			
			base.draw( graphics, parentAlpha );
		}


		public Label getLabel()
		{
			return label;
		}


		public Cell getLabelCell()
		{
			return getCell( label );
		}


		public void setText( String text )
		{
			label.setText( text );
		}


		public string getText()
		{
			return label.getText();
		}


		public override string ToString()
		{
			return string.Format( "[TextButton] text: {0}", getText() );
		}
	}


	/// <summary>
	/// The style for a text button
	/// </summary>
	public class TextButtonStyle : ButtonStyle
	{
		public BitmapFont font;

		/** Optional. */
		public Color fontColor = Color.White;
		public Color? downFontColor, overFontColor, checkedFontColor, checkedOverFontColor, disabledFontColor;


		public TextButtonStyle()
		{
			font = Graphics.instance.bitmapFont;
		}


		public TextButtonStyle( IDrawable up, IDrawable down, IDrawable over, BitmapFont font ) : base( up, down, over )
		{
			this.font = font ?? Graphics.instance.bitmapFont;
		}


		public TextButtonStyle( IDrawable up, IDrawable down, IDrawable over ) : this( up, down, over, Graphics.instance.bitmapFont )
		{
		}


		public new static TextButtonStyle create( Color upColor, Color downColor, Color overColor )
		{
			return new TextButtonStyle {
				up = new PrimitiveDrawable( upColor ),
				down = new PrimitiveDrawable( downColor ),
				over = new PrimitiveDrawable( overColor )
			};
		}


		public new TextButtonStyle clone()
		{
			return new TextButtonStyle {
				up = up,
				down = down,
				over = over,
				checkked = checkked,
				checkedOver = checkedOver,
				disabled = disabled,
					
				font = font,
				fontColor = fontColor,
				downFontColor = downFontColor,
				overFontColor = overFontColor,
				checkedFontColor = checkedFontColor,
				checkedOverFontColor = checkedOverFontColor,
				disabledFontColor = disabledFontColor
			};
		}

	}
}

