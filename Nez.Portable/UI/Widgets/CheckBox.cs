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


		public CheckBox(string text, CheckBoxStyle style) : base(text, style)
		{
			ClearChildren();
			var label = GetLabel();
			imageCell = Add(image = new Image(style.CheckboxOff));
			Add(label);
			label.SetAlignment(UI.Align.Left);
			GetLabelCell().SetPadLeft(10);
			SetSize(PreferredWidth, PreferredHeight);
		}


		public CheckBox(string text, Skin skin, string styleName = null) : this(text,
			skin.Get<CheckBoxStyle>(styleName))
		{
		}


		public override void SetStyle(ButtonStyle style)
		{
			Insist.IsTrue(style is CheckBoxStyle, "style must be a CheckBoxStyle");
			base.SetStyle(style);
			this.style = (CheckBoxStyle) style;
		}


		/// <summary>
		/// Returns the checkbox's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called
		/// </summary>
		/// <returns>The style.</returns>
		public new CheckBoxStyle GetStyle()
		{
			return style;
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			IDrawable checkbox = null;
			if (_isDisabled)
			{
				if (IsChecked && style.CheckboxOnDisabled != null)
					checkbox = style.CheckboxOnDisabled;
				else
					checkbox = style.CheckboxOffDisabled;
			}

			if (checkbox == null)
			{
				if (IsChecked && style.CheckboxOn != null)
					checkbox = style.CheckboxOn;
				else if (_mouseOver && style.CheckboxOver != null && !_isDisabled)
					checkbox = style.CheckboxOver;
				else
					checkbox = style.CheckboxOff;
			}

			image.SetDrawable(checkbox);
			base.Draw(batcher, parentAlpha);
		}


		public Image GetImage()
		{
			return image;
		}


		public Cell GetImageCell()
		{
			return imageCell;
		}
	}


	/// <summary>
	/// The style for a select box
	/// </summary>
	public class CheckBoxStyle : TextButtonStyle
	{
		public IDrawable CheckboxOn, CheckboxOff;

		/** Optional. */
		public IDrawable CheckboxOver, CheckboxOnDisabled, CheckboxOffDisabled;


		public CheckBoxStyle()
		{
			Font = Graphics.Instance.BitmapFont;
		}


		public CheckBoxStyle(IDrawable checkboxOff, IDrawable checkboxOn, BitmapFont font, Color fontColor)
		{
			CheckboxOff = checkboxOff;
			CheckboxOn = checkboxOn;
			Font = font ?? Graphics.Instance.BitmapFont;
			FontColor = fontColor;
		}
	}
}