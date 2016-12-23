using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A button with a child {@link Image} to display an image. This is useful when the button must be larger than the image and the
	/// image centered on the button. If the image is the size of the button, a {@link Button} without any children can be used, where
	/// the {@link Button.ButtonStyle#up}, {@link Button.ButtonStyle#down}, and {@link Button.ButtonStyle#checked} nine patches define
	/// the image.
	/// </summary>
	public class ImageButton : Button
	{
		Image image;
		ImageButtonStyle style;


		public ImageButton( ImageButtonStyle style ) : base( style )
		{
			image = new Image();
			image.setScaling( Scaling.Fit );
			add( image );
			setStyle( style );
			setSize( preferredWidth, preferredHeight );
		}

		public ImageButton( Skin skin, string styleName = null ) : this( skin.get<ImageButtonStyle>( styleName ) )
		{}


		public ImageButton( IDrawable imageUp ) : this( new ImageButtonStyle( null, null, null, imageUp, null, null ) )
		{
		}


		public ImageButton( IDrawable imageUp, IDrawable imageDown ) : this( new ImageButtonStyle( null, null, null, imageUp, imageDown, null ) )
		{
		}


		public ImageButton( IDrawable imageUp, IDrawable imageDown, IDrawable imageOver ) : this( new ImageButtonStyle( null, null, null, imageUp, imageDown, imageOver ) )
		{
		}


		public override void setStyle( ButtonStyle style )
		{
			Assert.isTrue( style is ImageButtonStyle, "style must be a ImageButtonStyle" );

			base.setStyle( style );
			this.style = (ImageButtonStyle)style;
			if( image != null )
				updateImage();
		}


		public new ImageButtonStyle getStyle()
		{
			return style;
		}


		public Image getImage()
		{
			return image;
		}


		public Cell getImageCell()
		{
			return getCell( image );
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
			base.draw( graphics, parentAlpha );
		}

	}


	public class ImageButtonStyle : ButtonStyle
	{
		/** Optional. */
		public IDrawable imageUp, imageDown, imageOver, imageChecked, imageCheckedOver, imageDisabled;


		public ImageButtonStyle()
		{}


		public ImageButtonStyle( IDrawable up, IDrawable down, IDrawable checkked, IDrawable imageUp, IDrawable imageDown, IDrawable imageChecked ) : base( up, down, checkked )
		{
			this.imageUp = imageUp;
			this.imageDown = imageDown;
			this.imageChecked = imageChecked;
		}


		public new ImageButtonStyle clone()
		{
			return new ImageButtonStyle {
				up = up,
				down = down,
				over = over,
				checkked = checkked,
				checkedOver = checkedOver,
				disabled = disabled,

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

