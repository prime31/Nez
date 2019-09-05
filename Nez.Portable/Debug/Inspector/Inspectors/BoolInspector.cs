using Nez.UI;


#if DEBUG
namespace Nez
{
	public class BoolInspector : Inspector
	{
		CheckBox _checkbox;


		public override void Initialize(Table table, Skin skin, float leftCellWidth)
		{
			var label = CreateNameLabel(table, skin, leftCellWidth);

			_checkbox = new CheckBox(string.Empty, skin);
			_checkbox.ProgrammaticChangeEvents = false;
			_checkbox.IsChecked = GetValue<bool>();
			_checkbox.OnChanged += newValue => { SetValue(newValue); };

			table.Add(label).Width(135);
			table.Add(_checkbox);
		}


		public override void Update()
		{
			_checkbox.IsChecked = GetValue<bool>();
		}
	}
}
#endif