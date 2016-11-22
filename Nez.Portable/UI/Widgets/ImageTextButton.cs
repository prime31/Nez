using System;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class ImageTextButton : Button
	{
		Image image;
		Label label;
		ImageTextButtonStyle style;


		public ImageTextButton( string text, ImageTextButtonStyle style ) : base( style )
		{
			this.style = style;

			defaults().space( 3 );

			image = new Image();
			image.setScaling( Scaling.Fit );

			label = new Label( text, style.font, style.fontColor );
			label.setAlignment( Align.center );

			add( image );
			add( label );

			setStyle( style );

			setSize( preferredWidth, preferredHeight );
		}


		public ImageTextButton( string text, Skin skin, string styleName = null ) : this( text, skin.get<ImageTextButtonStyle>( styleName ) )
		{}


		public void setStyle( ImageTextButtonStyle style )
		{
			Assert.isTrue( style is ImageTextButtonStyle, "style must be a ImageTextButtonStyle" );

			base.setStyle( style );

			if( image != null )
				updateImage();
			
			if( label != null )
			{
				var labelStyle = label.getStyle();
				labelStyle.font = style.font;
				labelStyle.fontColor = style.fontColor;
				label.setStyle( labelStyle );
			}
		}


		public new ImageTextButtonStyle getStyle()
		{
			return style;
		}


		private void updateImage()
		{
			IDrawable drawable = null;
			if( _isDisabled && style.imageDisabled != null )
				drawable = style.imageDisabled;
			else if( _mouseDown && style.imageDown != null )
				drawable = style.imageDown;
			else if( isChecked && style.imageChecked != null )
				drawable = ( style.imageCheckedOver != null && _mouseOver ) ? style.imageCheckedOver : style.imageChecked;
			else if( _mouseOver && style.imageOver != null )
				drawable = style.imageOver;
			else if( style.imageUp != null ) //
				drawable = style.imageUp;
			image.setDrawable( drawable );
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			updateImage();

			Color? fontColor;
			if( _isDisabled && style.disabledFontColor.HasValue )
				fontColor = style.disabledFontColor;
			else if( _mouseDown && style.downFontColor.HasValue )
				fontColor = style.downFontColor;
			else if( isChecked && style.checkedFontColor.HasValue )
				fontColor = ( _mouseOver && style.checkedOverFontColor.HasValue ) ? style.checkedOverFontColor : style.checkedFontColor;
			else if( _mouseOver && style.overFontColor.HasValue )
				fontColor = style.overFontColor;
			else
				fontColor = style.fontColor;
			
			if( fontColor.HasValue )
				label.getStyle().fontColor = fontColor.Value;
			
			base.draw( graphics, parentAlpha );
		}


		public Image getImage()
		{
			return image;
		}


		public Cell getImageCell()
		{
			return getCell( image );
		}


		public Label getLabel()
		{
			return label;
		}


		public Cell getLabelCell()
		{
			return getCell( label );
		}


		public void setText( string text )
		{
			label.setText( text );
		}


		public string getText()
		{
			return label.getText();
		}
	}


	public class ImageTextButtonStyle : TextButtonStyle
	{
		/** Optional. */
		public IDrawable imageUp, imageDown, imageOver, imageChecked, imageCheckedOver, imageDisabled;


		public ImageTextButtonStyle()
		{
			font = Graphics.instance.bitmapFont;
		}


		public ImageTextButtonStyle( IDrawable up, IDrawable down, IDrawable over, BitmapFont font ) : base( up, down, over, font )
		{
		}


		public new ImageTextButtonStyle clone()
		{
			return new ImageTextButtonStyle {
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
				disabledFontColor = disabledFontColor,

				imageUp = imageUp,
				imageDown = imageDown,
				imageOver = imageOver,
				imageChecked = imageChecked,
				imageCheckedOver = imageCheckedOver,
				imageDisabled = imageDisabled
			};
		}
	}
}

