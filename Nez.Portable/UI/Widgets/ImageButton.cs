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


		public ImageButton(ImageButtonStyle style) : base(style)
		{
			image = new Image();
			image.SetScaling(Scaling.Fit);
			Add(image);
			SetStyle(style);
			SetSize(PreferredWidth, PreferredHeight);
		}

		public ImageButton(Skin skin, string styleName = null) : this(skin.Get<ImageButtonStyle>(styleName))
		{ }


		public ImageButton(IDrawable imageUp) : this(new ImageButtonStyle(null, null, null, imageUp, null, null))
		{ }


		public ImageButton(IDrawable imageUp, IDrawable imageDown) : this(new ImageButtonStyle(null, null, null,
			imageUp, imageDown, null))
		{ }


		public ImageButton(IDrawable imageUp, IDrawable imageDown, IDrawable imageOver) : this(
			new ImageButtonStyle(null, null, null, imageUp, imageDown, imageOver))
		{ }


		public override void SetStyle(ButtonStyle style)
		{
			Insist.IsTrue(style is ImageButtonStyle, "style must be a ImageButtonStyle");

			base.SetStyle(style);
			this.style = (ImageButtonStyle) style;
			if (image != null)
				UpdateImage();
		}


		public new ImageButtonStyle GetStyle()
		{
			return style;
		}


		public Image GetImage()
		{
			return image;
		}


		public Cell GetImageCell()
		{
			return GetCell(image);
		}


		private void UpdateImage()
		{
			IDrawable drawable = null;
			if (_isDisabled && style.ImageDisabled != null)
				drawable = style.ImageDisabled;
			else if (_mouseDown && style.ImageDown != null)
				drawable = style.ImageDown;
			else if (IsChecked && style.ImageChecked != null)
				drawable = (style.ImageCheckedOver != null && _mouseOver) ? style.ImageCheckedOver : style.ImageChecked;
			else if (_mouseOver && style.ImageOver != null)
				drawable = style.ImageOver;
			else if (style.ImageUp != null) //
				drawable = style.ImageUp;

			image.SetDrawable(drawable);
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			UpdateImage();
			base.Draw(batcher, parentAlpha);
		}
	}


	public class ImageButtonStyle : ButtonStyle
	{
		/** Optional. */
		public IDrawable ImageUp, ImageDown, ImageOver, ImageChecked, ImageCheckedOver, ImageDisabled;


		public ImageButtonStyle()
		{ }


		public ImageButtonStyle(IDrawable up, IDrawable down, IDrawable checkked, IDrawable imageUp,
		                        IDrawable imageDown, IDrawable imageChecked) : base(up, down, checkked)
		{
			ImageUp = imageUp;
			ImageDown = imageDown;
			ImageChecked = imageChecked;
		}


		public new ImageButtonStyle Clone()
		{
			return new ImageButtonStyle
			{
				Up = Up,
				Down = Down,
				Over = Over,
				Checked = Checked,
				CheckedOver = CheckedOver,
				Disabled = Disabled,

				ImageUp = ImageUp,
				ImageDown = ImageDown,
				ImageOver = ImageOver,
				ImageChecked = ImageChecked,
				ImageCheckedOver = ImageCheckedOver,
				ImageDisabled = ImageDisabled
			};
		}
	}
}