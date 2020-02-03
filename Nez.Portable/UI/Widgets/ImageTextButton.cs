using Nez.BitmapFonts;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class ImageTextButton : Button
	{
		Image image;
		Label label;
		ImageTextButtonStyle style;


		public ImageTextButton(string text, ImageTextButtonStyle style) : base(style)
		{
			this.style = style;

			Defaults().Space(3);

			image = new Image();
			image.SetScaling(Scaling.Fit);

			label = new Label(text, style.Font, style.FontColor);
			label.SetAlignment(UI.Align.Center);

			Add(image);
			Add(label);

			SetStyle(style);

			SetSize(PreferredWidth, PreferredHeight);
		}


		public ImageTextButton(string text, Skin skin, string styleName = null) : this(text,
			skin.Get<ImageTextButtonStyle>(styleName))
		{
		}


		public void SetStyle(ImageTextButtonStyle style)
		{
			Insist.IsTrue(style is ImageTextButtonStyle, "style must be a ImageTextButtonStyle");

			base.SetStyle(style);

			if (image != null)
				UpdateImage();

			if (label != null)
			{
				var labelStyle = label.GetStyle();
				labelStyle.Font = style.Font;
				labelStyle.FontColor = style.FontColor;
				label.SetStyle(labelStyle);
			}
		}


		public new ImageTextButtonStyle GetStyle()
		{
			return style;
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

			Color? fontColor;
			if (_isDisabled && style.DisabledFontColor.HasValue)
				fontColor = style.DisabledFontColor;
			else if (_mouseDown && style.DownFontColor.HasValue)
				fontColor = style.DownFontColor;
			else if (IsChecked && style.CheckedFontColor.HasValue)
				fontColor = (_mouseOver && style.CheckedOverFontColor.HasValue)
					? style.CheckedOverFontColor
					: style.CheckedFontColor;
			else if (_mouseOver && style.OverFontColor.HasValue)
				fontColor = style.OverFontColor;
			else
				fontColor = style.FontColor;

			if (fontColor.HasValue)
				label.GetStyle().FontColor = fontColor.Value;

			base.Draw(batcher, parentAlpha);
		}


		public Image GetImage()
		{
			return image;
		}


		public Cell GetImageCell()
		{
			return GetCell(image);
		}


		public Label GetLabel()
		{
			return label;
		}


		public Cell GetLabelCell()
		{
			return GetCell(label);
		}


		public void SetText(string text)
		{
			label.SetText(text);
		}


		public string GetText()
		{
			return label.GetText();
		}
	}


	public class ImageTextButtonStyle : TextButtonStyle
	{
		/** Optional. */
		public IDrawable ImageUp, ImageDown, ImageOver, ImageChecked, ImageCheckedOver, ImageDisabled;


		public ImageTextButtonStyle()
		{
			Font = Graphics.Instance.BitmapFont;
		}


		public ImageTextButtonStyle(IDrawable up, IDrawable down, IDrawable over, BitmapFont font) : base(up, down,
			over, font)
		{
		}


		public new ImageTextButtonStyle Clone()
		{
			return new ImageTextButtonStyle
			{
				Up = Up,
				Down = Down,
				Over = Over,
				Checked = Checked,
				CheckedOver = CheckedOver,
				Disabled = Disabled,

				Font = Font,
				FontColor = FontColor,
				DownFontColor = DownFontColor,
				OverFontColor = OverFontColor,
				CheckedFontColor = CheckedFontColor,
				CheckedOverFontColor = CheckedOverFontColor,
				DisabledFontColor = DisabledFontColor,

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