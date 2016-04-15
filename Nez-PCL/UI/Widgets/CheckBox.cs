using System;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A checkbox is a button that contains an image indicating the checked or unchecked state and a label
	/// </summary>
	public class CheckBox : TextButton
	{
		private Image image;
		private Cell imageCell;
		private CheckBoxStyle style;


		public CheckBox( string text, CheckBoxStyle style ) : base( text, style )
		{
			clearChildren();
			var label = getLabel();
			imageCell = add( image = new Image( style.checkboxOff ) );
			add( label );
			label.setAlignment( Align.left );
			getLabelCell().setPadLeft( 10 );
			setSize( preferredWidth, preferredHeight );
		}


		public CheckBox( string text, Skin skin, string styleName = null ) : this( text, skin.get<CheckBoxStyle>( styleName ) )
		{}


		public override void setStyle( ButtonStyle style )
		{
			Assert.isTrue( style is CheckBoxStyle, "style must be a CheckBoxStyle" );
			base.setStyle( style );
			this.style = (CheckBoxStyle)style;
		}


		/// <summary>
		/// Returns the checkbox's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called
		/// </summary>
		/// <returns>The style.</returns>
		public new CheckBoxStyle getStyle()
		{
			return style;
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			IDrawable checkbox = null;
			if( _isDisabled )
			{
				if( isChecked && style.checkboxOnDisabled != null )
					checkbox = style.checkboxOnDisabled;
				else
					checkbox = style.checkboxOffDisabled;
			}

			if( checkbox == null )
			{
				if( isChecked && style.checkboxOn != null )
					checkbox = style.checkboxOn;
				else if( _mouseOver && style.checkboxOver != null && !_isDisabled )
					checkbox = style.checkboxOver;
				else
					checkbox = style.checkboxOff;
			}

			image.setDrawable( checkbox );
			base.draw( graphics, parentAlpha );
		}


		public Image getImage()
		{
			return image;
		}


		public Cell getImageCell()
		{
			return imageCell;
		}
	
	}


	/// <summary>
	/// The style for a select box
	/// </summary>
	public class CheckBoxStyle : TextButtonStyle
	{
		public IDrawable checkboxOn, checkboxOff;
		/** Optional. */
		public IDrawable checkboxOver, checkboxOnDisabled, checkboxOffDisabled;


		public CheckBoxStyle()
		{
			font = Graphics.instance.bitmapFont;
		}


		public CheckBoxStyle( IDrawable checkboxOff, IDrawable checkboxOn, BitmapFont font, Color fontColor )
		{
			this.checkboxOff = checkboxOff;
			this.checkboxOn = checkboxOn;
			this.font = font ?? Graphics.instance.bitmapFont;
			this.fontColor = fontColor;
		}
	}
}

