using Nez.UI;


#if DEBUG
namespace Nez
{
	public class StringInspector : Inspector
	{
		TextField _textField;


		public override void Initialize(Table table, Skin skin, float leftCellWidth)
		{
			var label = CreateNameLabel(table, skin, leftCellWidth);
			_textField = new TextField(GetValue<string>(), skin);
			_textField.SetTextFieldFilter(new FloatFilter());
			_textField.OnTextChanged += (field, str) => { SetValue(str); };

			table.Add(label);
			table.Add(_textField).SetMaxWidth(70);
		}


		public override void Update()
		{
			_textField.SetText(GetValue<string>());
		}
	}
}
#endif