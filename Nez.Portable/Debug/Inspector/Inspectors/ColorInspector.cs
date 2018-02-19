using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	public class ColorInspector : Inspector
	{
		TextField _textFieldR, _textFieldG, _textFieldB, _textFieldA;


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			var value = getValue<Color>();
			var label = createNameLabel( table, skin, leftCellWidth );

			var labelR = new Label( "r", skin );
			_textFieldR = new TextField( value.R.ToString(), skin );
			_textFieldR.setMaxLength( 3 );
			_textFieldR.setTextFieldFilter( new DigitsOnlyFilter() ).setPreferredWidth( 28 );
			_textFieldR.onTextChanged += ( field, str ) =>
			{
				int newR;
				if( int.TryParse( str, out newR ) )
				{
					var newValue = getValue<Color>();
					newValue.R = (byte)newR;
					setValue( newValue );
				}
			};

			var labelG = new Label( "g", skin );
			_textFieldG = new TextField( value.G.ToString(), skin );
			_textFieldG.setMaxLength( 3 );
			_textFieldG.setTextFieldFilter( new DigitsOnlyFilter() ).setPreferredWidth( 28 );
			_textFieldG.onTextChanged += ( field, str ) =>
			{
				int newG;
				if( int.TryParse( str, out newG ) )
				{
					var newValue = getValue<Color>();
					newValue.G = (byte)newG;
					setValue( newValue );
				}
			};

			var labelB = new Label( "b", skin );
			_textFieldB = new TextField( value.B.ToString(), skin );
			_textFieldB.setMaxLength( 3 );
			_textFieldB.setTextFieldFilter( new DigitsOnlyFilter() ).setPreferredWidth( 28 );
			_textFieldB.onTextChanged += ( field, str ) =>
			{
				int newB;
				if( int.TryParse( str, out newB ) )
				{
					var newValue = getValue<Color>();
					newValue.B = (byte)newB;
					setValue( newValue );
				}
			};

			var labelA = new Label( "a", skin );
			_textFieldA = new TextField( value.A.ToString(), skin );
			_textFieldA.setMaxLength( 3 );
			_textFieldA.setTextFieldFilter( new DigitsOnlyFilter() ).setPreferredWidth( 28 );
			_textFieldA.onTextChanged += ( field, str ) =>
			{
				int newA;
				if( int.TryParse( str, out newA ) )
				{
					var newValue = getValue<Color>();
					newValue.A = (byte)newA;
					setValue( newValue );
				}
			};

			var hBox = new HorizontalGroup( 2 );
			hBox.addElement( labelR );
			hBox.addElement( _textFieldR );
			hBox.addElement( labelG );
			hBox.addElement( _textFieldG );
			hBox.addElement( labelB );
			hBox.addElement( _textFieldB );
			hBox.addElement( labelA );
			hBox.addElement( _textFieldA );

			table.add( label );
			table.add( hBox );
		}


		public override void update()
		{
			var value = getValue<Color>();
			_textFieldR.setText( value.R.ToString() );
			_textFieldG.setText( value.G.ToString() );
		}

	}
}
#endif