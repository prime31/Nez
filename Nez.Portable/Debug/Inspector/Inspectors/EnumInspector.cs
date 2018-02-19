using System;
using Nez.UI;
using System.Collections.Generic;


#if DEBUG
namespace Nez
{
	public class EnumInspector : Inspector
	{
		SelectBox<string> _selectBox;


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			var label = createNameLabel( table, skin, leftCellWidth );

			// gotta get ugly here
			_selectBox = new SelectBox<string>( skin );

			var enumValues = Enum.GetValues( _valueType );
			var enumStringValues = new List<string>();
			foreach( var e in enumValues )
				enumStringValues.Add( e.ToString() );
			_selectBox.setItems( enumStringValues );

			_selectBox.onChanged += selectedItem =>
			{
				setValue( Enum.Parse( _valueType, selectedItem ) );
			};

			table.add( label );
			table.add( _selectBox ).setFillX();
		}


		public override void update()
		{
			_selectBox.setSelected( getValue<object>().ToString() );
		}
	}
}
#endif