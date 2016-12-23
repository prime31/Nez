using Nez.UI;


#if DEBUG
namespace Nez
{
	public class IntInspector : Inspector
	{
		TextField _textField;
		Slider _slider;


		public override void initialize( Table table, Skin skin )
		{
			// if we have a RangeAttribute we need to make a slider
			var rangeAttr = getFieldOrPropertyAttribute<RangeAttribute>();
			if( rangeAttr != null )
				setupSlider( table, skin, rangeAttr.minValue, rangeAttr.maxValue, rangeAttr.stepSize );
			else
				setupTextField( table, skin );
		}


		void setupTextField( Table table, Skin skin )
		{
			var label = createNameLabel( table, skin );
			_textField = new TextField( getValue<int>().ToString(), skin );
			_textField.setTextFieldFilter( new FloatFilter() );
			_textField.onTextChanged += ( field, str ) =>
			{
				int newValue;
				if( int.TryParse( str, out newValue ) )
					setValue( newValue );
			};

			table.add( label );
			table.add( _textField ).setMaxWidth( 70 );
		}


		void setupSlider( Table table, Skin skin, float minValue, float maxValue, float stepSize )
		{
			var label = createNameLabel( table, skin );
			_slider = new Slider( skin, null, minValue, maxValue );
			_slider.setStepSize( stepSize );
			_slider.setValue( getValue<int>() );
			_slider.onChanged += newValue =>
			{
				_setter.Invoke( (int)newValue );
			};

			table.add( label );
			table.add( _slider );
		}


		public override void update()
		{
			if( _textField != null )
				_textField.setText( getValue<int>().ToString() );
			if( _slider != null )
				_slider.setValue( getValue<int>() );
		}
	}
}
#endif