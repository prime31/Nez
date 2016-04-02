using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


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


		public Dialog( string title, WindowStyle windowStyle ) : base( title, windowStyle )
		{
			initialize();
		}


		public Dialog( string title, Skin skin, string styleName = null ) : this( title, skin.get<WindowStyle>( styleName ) )
		{}


		private void initialize()
		{
			defaults().space( 16 );
			add( contentTable = new Table() ).expand().fill();
			row();
			add( buttonTable = new Table() );

			contentTable.defaults().space( 16 );
			buttonTable.defaults().space( 16 );
		}


		public Table getContentTable()
		{
			return contentTable;
		}


		public Table getButtonTable()
		{
			return buttonTable;
		}


		/// <summary>
		/// Adds a label to the content table
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="text">Text.</param>
		public Dialog addText( string text )
		{
			return addText( new Label( text ) );
		}


		/// <summary>
		/// Adds the given Label to the content table
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="label">Label.</param>
		public Dialog addText( Label label )
		{
			contentTable.add( label );
			return this;
		}


		/** Adds a text button to the button table.
	 * @param object The object that will be passed to {@link #result(Object)} if this button is clicked. May be null. */
		public Button addButton( string text, TextButtonStyle buttonStyle )
		{
			return addButton( new TextButton( text, buttonStyle ) );
		}


		/** Adds the given button to the button table.
	 * @param object The object that will be passed to {@link #result(Object)} if this button is clicked. May be null. */
		public Button addButton( Button button )
		{
			buttonTable.add( button );
			return button;
		}


		/// <summary>
		/// {@link #pack() Packs} the dialog and adds it to the stage
		/// </summary>
		/// <param name="stage">Stage.</param>
		public Dialog show( Stage stage )
		{
			setPosition( Mathf.round( ( stage.getWidth() - getWidth() ) / 2 ), Mathf.round( ( stage.getHeight() - getHeight() ) / 2 ) );

			pack();
			stage.addElement( this );

			return this;
		}


		/// <summary>
		/// Hides the dialog
		/// </summary>
		public void hide()
		{
			remove();
		}

	}
}

