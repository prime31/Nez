namespace Nez.UI
{
	/// <summary>
	/// Displays a dialog, which is a modal window containing a content table with a button table underneath it. Methods are provided
	/// to add a label to the content table and buttons to the button table, but any widgets can be added. When a button is clicked,
	/// {@link #result(Object)} is called and the dialog is removed from the stage.
	/// </summary>
	public class Dialog : Window
	{
		Table contentTable, buttonTable;


		public Dialog(string title, WindowStyle windowStyle) : base(title, windowStyle)
		{
			Initialize();
		}


		public Dialog(string title, Skin skin, string styleName = null) : this(title, skin.Get<WindowStyle>(styleName))
		{
		}


		private void Initialize()
		{
			Defaults().Space(16);
			Add(contentTable = new Table()).Expand().Fill();
			Row();
			Add(buttonTable = new Table());

			contentTable.Defaults().Space(16);
			buttonTable.Defaults().Space(16);
		}


		public Table GetContentTable()
		{
			return contentTable;
		}


		public Table GetButtonTable()
		{
			return buttonTable;
		}


		/// <summary>
		/// Adds a label to the content table
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="text">Text.</param>
		public Dialog AddText(string text)
		{
			return AddText(new Label(text));
		}


		/// <summary>
		/// Adds the given Label to the content table
		/// </summary>
		/// <param name="label">Label.</param>
		public Dialog AddText(Label label)
		{
			contentTable.Add(label);
			return this;
		}


		/** Adds a text button to the button table.
	 * @param object The object that will be passed to {@link #result(Object)} if this button is clicked. May be null. */
		public Button AddButton(string text, TextButtonStyle buttonStyle)
		{
			return AddButton(new TextButton(text, buttonStyle));
		}


		/** Adds the given button to the button table.
	 * @param object The object that will be passed to {@link #result(Object)} if this button is clicked. May be null. */
		public Button AddButton(Button button)
		{
			buttonTable.Add(button);
			return button;
		}


		/// <summary>
		/// {@link #pack() Packs} the dialog and adds it to the stage
		/// </summary>
		/// <param name="stage">Stage.</param>
		public Dialog Show(Stage stage)
		{
			stage.AddElement(this);
			SetPosition(Mathf.Round((stage.GetWidth() - GetWidth()) / 2),
				Mathf.Round((stage.GetHeight() - GetHeight()) / 2));

			Pack();

			return this;
		}


		/// <summary>
		/// Hides the dialog
		/// </summary>
		public void Hide()
		{
			Remove();
		}
	}
}