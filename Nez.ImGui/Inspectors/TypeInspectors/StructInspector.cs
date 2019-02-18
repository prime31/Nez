using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez.IEnumerableExtensions;


namespace Nez.ImGuiTools.TypeInspectors
{
	public class StructInspector : AbstractTypeInspector
	{
		List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

		public override void initialize()
		{
			base.initialize();

			// figure out which fields and properties are useful to add to the inspector
			var fields = ReflectionUtils.getFields( _valueType );
			foreach( var field in fields )
			{
				if( !field.IsPublic && IEnumerableExt.count( field.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				var inspector = TypeInspectorUtils.getInspectorForType( field.FieldType, _target, field );
				if( inspector != null )
				{
					inspector.setStructTarget( _target, this, field );
					inspector.initialize();
					_inspectors.Add( inspector );
				}
			}

			var properties = ReflectionUtils.getProperties( _valueType );
			foreach( var prop in properties )
			{
				if( !prop.CanRead || !prop.CanWrite )
					continue;

				if( ( !prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic ) && IEnumerableExt.count( prop.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				var inspector = TypeInspectorUtils.getInspectorForType( prop.PropertyType, _target, prop );
				if( inspector != null )
				{
					inspector.setStructTarget( _target, this, prop );
					inspector.initialize();
					_inspectors.Add( inspector );
				}
			}
		}

		public override void draw()
		{
			NezImGui.beginBorderedGroup();
			ImGui.Text( _name );
			foreach( var i in _inspectors )
				i.draw();
			NezImGui.endBorderedGroup();
		}

	}
}
