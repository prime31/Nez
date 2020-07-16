using Microsoft.Xna.Framework;
using Nez.UI;
using System.Globalization;


#if DEBUG
namespace Nez
{
	public class Vector2Inspector : Inspector
	{
		TextField _textFieldX, _textFieldY;


		public override void Initialize(Table table, Skin skin, float leftCellWidth)
		{
			var value = GetValue<Vector2>();
			var label = CreateNameLabel(table, skin, leftCellWidth);

			var labelX = new Label("x", skin);
			_textFieldX = new TextField(value.X.ToString(CultureInfo.InvariantCulture), skin);
			_textFieldX.SetTextFieldFilter(new FloatFilter()).SetPreferredWidth(50);
			_textFieldX.OnTextChanged += (field, str) =>
			{
				float newX;
				if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out newX))
				{
					var newValue = GetValue<Vector2>();
					newValue.X = newX;
					SetValue(newValue);
				}
			};

			var labelY = new Label("y", skin);
			_textFieldY = new TextField(value.Y.ToString(CultureInfo.InvariantCulture), skin);
			_textFieldY.SetTextFieldFilter(new FloatFilter()).SetPreferredWidth(50);
			_textFieldY.OnTextChanged += (field, str) =>
			{
				float newY;
				if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out newY))
				{
					var newValue = GetValue<Vector2>();
					newValue.Y = newY;
					SetValue(newValue);
				}
			};

			var hBox = new HorizontalGroup(5);
			hBox.AddElement(labelX);
			hBox.AddElement(_textFieldX);
			hBox.AddElement(labelY);
			hBox.AddElement(_textFieldY);

			table.Add(label);
			table.Add(hBox);
		}


		public override void Update()
		{
			var value = GetValue<Vector2>();
			_textFieldX.SetText(value.X.ToString(CultureInfo.InvariantCulture));
			_textFieldY.SetText(value.Y.ToString(CultureInfo.InvariantCulture));
		}
	}
}
#endif