using Nez.UI;


#if DEBUG
namespace Nez
{
	public class FloatInspector : Inspector
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
			_textField = new TextField( getValue<float>().ToString(), skin );
			_textField.setTextFieldFilter( new FloatFilter() );
			_textField.onTextChanged += ( field, str ) =>
			{
				float newValue;
				if( float.TryParse( str, out newValue ) )
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
				_textField.setText( getValue<float>().ToString() );
			if( _slider != null )
				_slider.setValue( getValue<float>() );
		}
	}
}
#endif