namespace Nez.UI
{
	public class TextTooltip : Tooltip
	{
		public TextTooltip( string text, Element targetElement, Skin skin, string styleName = null ) : this( text, targetElement, skin.get<TextTooltipStyle>( styleName ) )
		{ }


		public TextTooltip( string text, Element targetElement, TextTooltipStyle style ) : base( null, targetElement )
		{
			var label = new Label( text, style.labelStyle );
			_container.setElement( label );
			setStyle( style );
		}


		public TextTooltip setStyle( TextTooltipStyle style )
		{
			_container.getElement<Label>().setStyle( style.labelStyle );
			_container.setBackground( style.background );
			return this;
		}
	}


	public class TextTooltipStyle
	{
		public LabelStyle labelStyle;
		/** Optional. */
		public IDrawable background;


		public TextTooltipStyle()
		{ }


		public TextTooltipStyle( LabelStyle label, IDrawable background )
		{
			this.labelStyle = label;
			this.background = background;
		}
	}
}

