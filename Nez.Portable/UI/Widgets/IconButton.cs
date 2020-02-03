using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
    /// <summary>
    /// Button with a background and an icon
    /// </summary>
    public class IconButton : Button
    {
        Image image;
        Image iconImage;
        IconButtonStyle style;


        public IconButton(IconButtonStyle style) : base(style)
        {
            image = new Image();
            iconImage = new Image();
            var stck = new Stack();
            image.SetScaling(Scaling.Fit);
            stck.Add(image);
            stck.Add(iconImage);
            Add(stck);
            SetStyle(style);
            PadLeft(style.PadLeft).PadRight(style.PadRight).PadTop(style.PadTop).PadBottom(style.PadBottom);
            SetSize(PreferredWidth, PreferredHeight);
        }

        public IconButton(Skin skin, string styleName = null) : this(skin.Get<IconButtonStyle>(styleName))
        { }


        public IconButton(IDrawable icon, IDrawable imageUp) : this(new IconButtonStyle(icon, null, null, null, imageUp, null, null))
        {
        }


        public IconButton(IDrawable icon, IDrawable imageUp, IDrawable imageDown) : this(new IconButtonStyle(icon, null, null, null, imageUp, imageDown, null))
        {
        }


        public IconButton(IDrawable icon, IDrawable imageUp, IDrawable imageDown, IDrawable imageOver) : this(new IconButtonStyle(icon, null, null, null, imageUp, imageDown, imageOver))
        {
        }


        public override void SetStyle(ButtonStyle style)
        {
            Insist.IsTrue(style is IconButtonStyle, "style must be a ImageButtonStyle");

            base.SetStyle(style);
            this.style = (IconButtonStyle)style;
            if (image != null)
                UpdateImage();
        }


        public new IconButtonStyle GetStyle()
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
            iconImage.SetDrawable(style.Icon);
        }


        public override void Draw(Batcher batcher, float parentAlpha)
        {
            UpdateImage();
            base.Draw(batcher, parentAlpha);
        }

    }


    public class IconButtonStyle : ImageButtonStyle
    {
        public IDrawable Icon;
        public int PadLeft, PadRight, PadTop, PadBottom;

        public IconButtonStyle()
        { }


        public IconButtonStyle(IDrawable icon, IDrawable up, IDrawable down, IDrawable checkked, IDrawable imageUp, IDrawable imageDown, IDrawable imageChecked) : base(up, down, checkked, imageUp, imageDown, imageChecked)
        {
            this.Icon = icon;
        }


        public new IconButtonStyle Clone()
        {
            return new IconButtonStyle
            {
                Icon = Icon,
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

