using Nez.UI;
using System.Globalization;


#if DEBUG
namespace Nez
{
	public class FloatInspector : Inspector
	{
		TextField _textField;
		Slider _slider;


		public override void Initialize(Table table, Skin skin, float leftCellWidth)
		{
			// if we have a RangeAttribute we need to make a slider
			var rangeAttr = GetFieldOrPropertyAttribute<RangeAttribute>();
			if (rangeAttr != null)
				SetupSlider(table, skin, leftCellWidth, rangeAttr.MinValue, rangeAttr.MaxValue, rangeAttr.StepSize);
			else
				SetupTextField(table, skin, leftCellWidth);
		}


		void SetupTextField(Table table, Skin skin, float leftCellWidth)
		{
			var label = CreateNameLabel(table, skin, leftCellWidth);
			_textField = new TextField(GetValue<float>().ToString(CultureInfo.InvariantCulture), skin);
			_textField.SetTextFieldFilter(new FloatFilter());
			_textField.OnTextChanged += (field, str) =>
			{
				float newValue;
				if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out newValue))
					SetValue(newValue);
			};

			table.Add(label);
			table.Add(_textField).SetMaxWidth(70);
		}


		void SetupSlider(Table table, Skin skin, float leftCellWidth, float minValue, float maxValue, float stepSize)
		{
			var label = CreateNameLabel(table, skin, leftCellWidth);
			_slider = new Slider(skin, null, minValue, maxValue);
			_slider.SetStepSize(stepSize);
			_slider.SetValue(GetValue<float>());
			_slider.OnChanged += newValue => { _setter.Invoke(newValue); };

			table.Add(label);
			table.Add(_slider);
		}


		public override void Update()
		{
			if (_textField != null)
				_textField.SetText(GetValue<float>().ToString(CultureInfo.InvariantCulture));
			if (_slider != null)
				_slider.SetValue(GetValue<float>());
		}
	}
}
#endif