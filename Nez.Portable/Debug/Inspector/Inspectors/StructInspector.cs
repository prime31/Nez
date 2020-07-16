using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Nez.IEnumerableExtensions;
using Nez.UI;


#if DEBUG
namespace Nez
{
	public class StructInspector : Inspector
	{
		List<Inspector> _inspectors = new List<Inspector>();


		public override void Initialize(Table table, Skin skin, float leftCellWidth)
		{
			// add a header
			var label = table.Add(CreateNameLabel(table, skin)).SetColspan(2).GetElement<Label>();
			label.SetStyle(label.GetStyle().Clone()).SetFontColor(new Color(228, 228, 76));
			table.Row().SetPadLeft(15);

			// figure out which fiedls and properties are useful to add to the inspector
			var fields = ReflectionUtils.GetFields(_valueType);
			foreach (var field in fields)
			{
				if (!field.IsPublic && IEnumerableExt.Count(field.GetCustomAttributes<InspectableAttribute>()) == 0)
					continue;

				var inspector = GetInspectorForType(field.FieldType, _target, field);
				if (inspector != null)
				{
					inspector.SetStructTarget(_target, this, field);
					inspector.Initialize(table, skin, leftCellWidth);
					_inspectors.Add(inspector);
					table.Row().SetPadLeft(15);
				}
			}

			var properties = ReflectionUtils.GetProperties(_valueType);
			foreach (var prop in properties)
			{
				if (!prop.CanRead || !prop.CanWrite)
					continue;

				if ((!prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic) &&
				    IEnumerableExt.Count(prop.GetCustomAttributes<InspectableAttribute>()) == 0)
					continue;

				var inspector = GetInspectorForType(prop.PropertyType, _target, prop);
				if (inspector != null)
				{
					inspector.SetStructTarget(_target, this, prop);
					inspector.Initialize(table, skin, leftCellWidth);
					_inspectors.Add(inspector);
					table.Row().SetPadLeft(15);
				}
			}
		}


		public override void Update()
		{
			foreach (var i in _inspectors)
				i.Update();
		}
	}
}
#endif