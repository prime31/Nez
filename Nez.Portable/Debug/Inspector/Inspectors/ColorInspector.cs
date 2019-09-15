using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	public class ColorInspector : Inspector
	{
		TextField _textFieldR, _textFieldG, _textFieldB, _textFieldA;


		public override void Initialize(Table table, Skin skin, float leftCellWidth)
		{
			var value = GetValue<Color>();
			var label = CreateNameLabel(table, skin, leftCellWidth);

			var labelR = new Label("r", skin);
			_textFieldR = new TextField(value.R.ToString(), skin);
			_textFieldR.SetMaxLength(3);
			_textFieldR.SetTextFieldFilter(new DigitsOnlyFilter()).SetPreferredWidth(28);
			_textFieldR.OnTextChanged += (field, str) =>
			{
				int newR;
				if (int.TryParse(str, out newR))
				{
					var newValue = GetValue<Color>();
					newValue.R = (byte) newR;
					SetValue(newValue);
				}
			};

			var labelG = new Label("g", skin);
			_textFieldG = new TextField(value.G.ToString(), skin);
			_textFieldG.SetMaxLength(3);
			_textFieldG.SetTextFieldFilter(new DigitsOnlyFilter()).SetPreferredWidth(28);
			_textFieldG.OnTextChanged += (field, str) =>
			{
				int newG;
				if (int.TryParse(str, out newG))
				{
					var newValue = GetValue<Color>();
					newValue.G = (byte) newG;
					SetValue(newValue);
				}
			};

			var labelB = new Label("b", skin);
			_textFieldB = new TextField(value.B.ToString(), skin);
			_textFieldB.SetMaxLength(3);
			_textFieldB.SetTextFieldFilter(new DigitsOnlyFilter()).SetPreferredWidth(28);
			_textFieldB.OnTextChanged += (field, str) =>
			{
				int newB;
				if (int.TryParse(str, out newB))
				{
					var newValue = GetValue<Color>();
					newValue.B = (byte) newB;
					SetValue(newValue);
				}
			};

			var labelA = new Label("a", skin);
			_textFieldA = new TextField(value.A.ToString(), skin);
			_textFieldA.SetMaxLength(3);
			_textFieldA.SetTextFieldFilter(new DigitsOnlyFilter()).SetPreferredWidth(28);
			_textFieldA.OnTextChanged += (field, str) =>
			{
				int newA;
				if (int.TryParse(str, out newA))
				{
					var newValue = GetValue<Color>();
					newValue.A = (byte) newA;
					SetValue(newValue);
				}
			};

			var hBox = new HorizontalGroup(2);
			hBox.AddElement(labelR);
			hBox.AddElement(_textFieldR);
			hBox.AddElement(labelG);
			hBox.AddElement(_textFieldG);
			hBox.AddElement(labelB);
			hBox.AddElement(_textFieldB);
			hBox.AddElement(labelA);
			hBox.AddElement(_textFieldA);

			table.Add(label);
			table.Add(hBox);
		}


		public override void Update()
		{
			var value = GetValue<Color>();
			_textFieldR.SetText(value.R.ToString());
			_textFieldG.SetText(value.G.ToString());
		}
	}
}
#endif