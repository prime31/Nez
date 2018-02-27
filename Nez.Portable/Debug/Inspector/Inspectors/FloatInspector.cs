using Nez.UI;
using System.Globalization;


#if DEBUG
namespace Nez
{
	public class FloatInspector : Inspector
	{
		TextField _textField;
		Slider _slider;


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			// if we have a RangeAttribute we need to make a slider
			var rangeAttr = getFieldOrPropertyAttribute<RangeAttribute>();
			if( rangeAttr != null )
				setupSlider( table, skin, leftCellWidth, rangeAttr.minValue, rangeAttr.maxValue, rangeAttr.stepSize );
			else
				setupTextField( table, skin, leftCellWidth );
		}


		void setupTextField( Table table, Skin skin, float leftCellWidth )
		{
			var label = createNameLabel( table, skin, leftCellWidth );
			_textField = new TextField( getValue<float>().ToString( CultureInfo.InvariantCulture ), skin );
			_textField.setTextFieldFilter( new FloatFilter() );
			_textField.onTextChanged += ( field, str ) =>
			{
				float newValue;
				if( float.TryParse( str, NumberStyles.Float, CultureInfo.InvariantCulture, out newValue ) )
					setValue( newValue );
			};

			table.add( label );
			table.add( _textField ).setMaxWidth( 70 );
		}


		void setupSlider( Table table, Skin skin, float leftCellWidth, float minValue, float maxValue, float stepSize )
		{
			var label = createNameLabel( table, skin, leftCellWidth );
			_slider = new Slider( skin, null, minValue, maxValue );
			_slider.setStepSize( stepSize );
			_slider.setValue( getValue<float>() );
			_slider.onChanged += newValue =>
			{
				_setter.Invoke( newValue );
			};

			table.add( label );
			table.add( _slider );
		}


		public override void update()
		{
			if( _textField != null )
				_textField.setText( getValue<float>().ToString( CultureInfo.InvariantCulture ));
			if( _slider != null )
				_slider.setValue( getValue<float>() );
		}
	}
}
#endif