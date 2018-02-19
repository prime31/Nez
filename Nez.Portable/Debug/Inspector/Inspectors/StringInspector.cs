using Nez.UI;


#if DEBUG
namespace Nez
{
	public class StringInspector : Inspector
	{
		TextField _textField;


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			var label = createNameLabel( table, skin, leftCellWidth );
			_textField = new TextField( getValue<string>(), skin );
			_textField.setTextFieldFilter( new FloatFilter() );
			_textField.onTextChanged += ( field, str ) =>
			{
				setValue( str );
			};

			table.add( label );
			table.add( _textField ).setMaxWidth( 70 );
		}


		public override void update()
		{
			_textField.setText( getValue<string>() );
		}
	}
}
#endif