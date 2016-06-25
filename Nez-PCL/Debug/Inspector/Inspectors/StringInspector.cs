using Nez.UI;


#if DEBUG
namespace Nez
{
	public class StringInspector : Inspector
	{
		TextField _textfield;


		public override void initialize( Table table, Skin skin )
		{
			var label = new Label( _name, skin );
			_textfield = new TextField( getValue<string>(), skin );
			_textfield.setTextFieldFilter( new FloatFilter() );
			_textfield.onTextChanged += ( field, str ) =>
			{
				setValue( str );
			};

			table.add( label );
			table.add( _textfield ).setMaxWidth( 70 );
		}


		public override void update()
		{
			_textfield.setText( getValue<string>() );
		}
	}
}
#endif