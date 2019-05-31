namespace Nez
{
	/// <summary>
	/// very basic wrapper around TextRun. Note that the TextRunComponent.compile method should be used not TextRun.compile so that
	/// the Component data can be passed off to the TextRun.
	/// </summary>
	public class TextRunComponent : RenderableComponent
	{
		public override float width => textRun.width;
		public override float height => textRun.height;

		public TextRun textRun
		{
			get => _textRun;
			set
			{
				_textRun = value;
				compile();
			}
		}

		TextRun _textRun;

		public TextRunComponent()
		{}

		public TextRunComponent( TextRun textRun )
		{
			_textRun = textRun;
			compile();
		}

		/// <summary>
		/// calls through to TextRun.compile and handles marshalling some data between this Component and the underlying TextRun
		/// </summary>
		public void compile()
		{
			_textRun.position = transform.position;
			_textRun.rotation = transform.rotation;
			_textRun.compile();
		}

		public override void render( Graphics graphics, Camera camera )
		{
			if( _textRun != null )
				_textRun.render( graphics );
		}

	}
}

