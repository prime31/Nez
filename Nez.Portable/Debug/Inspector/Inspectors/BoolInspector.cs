using Nez.UI;


#if DEBUG
namespace Nez
{
	public class BoolInspector : Inspector
	{
		CheckBox _checkbox;


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			var label = createNameLabel( table, skin, leftCellWidth );

			_checkbox = new CheckBox( string.Empty, skin );
			_checkbox.programmaticChangeEvents = false;
			_checkbox.isChecked = getValue<bool>();
			_checkbox.onChanged += newValue =>
			{
				setValue( newValue );
			};

			table.add( label ).width( 135 );
			table.add( _checkbox );
		}


		public override void update()
		{
			_checkbox.isChecked = getValue<bool>();
		}
	}
}
#endif