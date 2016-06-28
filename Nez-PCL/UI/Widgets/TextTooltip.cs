namespace Nez.UI
{
	public class TextTooltip : Tooltip
	{
		public TextTooltip( string text, Element targetElement, Skin skin, string styleName = null ) : this( text, targetElement, skin.get<TextTooltipStyle>( styleName ) )
		{ }


		public TextTooltip( string text, Element targetElement, TextTooltipStyle style ) : base( null, targetElement )
		{
			var label = new Label( text, style.label );
			_container.setElement( label );
			setStyle( style );
		}


		public void setStyle( TextTooltipStyle style )
		{
			_container.getElement<Label>().setStyle( style.label );
			_container.setBackground( style.background );
		}
	}


	public class TextTooltipStyle
	{
		public LabelStyle label;
		/** Optional. */
		public IDrawable background;


		public TextTooltipStyle()
		{ }


		public TextTooltipStyle( LabelStyle label, IDrawable background )
		{
			this.label = label;
			this.background = background;
		}
	}
}

