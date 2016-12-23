namespace Nez
{
	/// <summary>
	/// very basic wrapper around TextRun. Note that the TextRunComponent.compile method should be used not TextRun.compile so that
	/// the Component data can be passed off to the TextRun.
	/// </summary>
	public class TextRunComponent : RenderableComponent
	{
		public override float width { get { return textRun.width; } }
		public override float height { get { return textRun.height; } }
		public TextRun textRun;


		public TextRunComponent( TextRun textRun )
		{
			this.textRun = textRun;
		}


		/// <summary>
		/// calls through to TextRun.compile and handles marshalling some data between this Component and the underlying TextRun
		/// </summary>
		public void compile()
		{
			textRun.position = transform.position;
			textRun.rotation = transform.rotation;
			textRun.compile();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			textRun.render( graphics );
		}
	}
}

