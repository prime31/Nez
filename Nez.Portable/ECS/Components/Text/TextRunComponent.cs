namespace Nez
{
	/// <summary>
	/// very basic wrapper around TextRun. Note that the TextRunComponent.compile method should be used not TextRun.compile so that
	/// the Component data can be passed off to the TextRun.
	/// </summary>
	public class TextRunComponent : RenderableComponent
	{
		public override float Width => TextRun.Width;
		public override float Height => TextRun.Height;

		public TextRun TextRun
		{
			get => _textRun;
			set
			{
				_textRun = value;
				Compile();
			}
		}

		TextRun _textRun;

		public TextRunComponent()
		{
		}

		public TextRunComponent(TextRun textRun)
		{
			_textRun = textRun;
			Compile();
		}

		/// <summary>
		/// calls through to TextRun.compile and handles marshalling some data between this Component and the underlying TextRun
		/// </summary>
		public void Compile()
		{
			_textRun.Position = Transform.Position;
			_textRun.Rotation = Transform.Rotation;
			_textRun.Compile();
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			if (_textRun != null)
				_textRun.Render(batcher);
		}
	}
}