using Nez.UI;


#if DEBUG
namespace Nez
{
	public class IntInspector : Inspector
	{
		TextField _textfield;
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
			var label = new Label( _name, skin );
			_textfield = new TextField( getValue<int>().ToString(), skin );
			_textfield.setTextFieldFilter( new FloatFilter() );
			_textfield.onTextChanged += ( field, str ) =>
			{
				int newValue;
				if( int.TryParse( str, out newValue ) )
					setValue( newValue );
			};

			table.add( label );
			table.add( _textfield ).setMaxWidth( 70 );
		}


		void setupSlider( Table table, Skin skin, float minValue, float maxValue, float stepSize )
		{
			var label = new Label( _name, skin );
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
			if( _textfield != null )
				_textfield.setText( getValue<int>().ToString() );
			if( _slider != null )
				_slider.setValue( getValue<int>() );
		}
	}
}
#endif