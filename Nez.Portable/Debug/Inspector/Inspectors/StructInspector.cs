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


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			// add a header
			var label = table.add( createNameLabel( table, skin ) ).setColspan( 2 ).getElement<Label>();
			label.setStyle( label.getStyle().clone() ).setFontColor( new Color( 228, 228, 76 ) );
			table.row().setPadLeft( 15 );

			// figure out which fiedls and properties are useful to add to the inspector
			var fields = ReflectionUtils.getFields( _valueType );
			foreach( var field in fields )
			{
				if( !field.IsPublic && IEnumerableExt.count( field.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				var inspector = getInspectorForType( field.FieldType, _target, field );
				if( inspector != null )
				{
					inspector.setStructTarget( _target, this, field );
					inspector.initialize( table, skin, leftCellWidth );
					_inspectors.Add( inspector );
					table.row().setPadLeft( 15 );
				}
			}

			var properties = ReflectionUtils.getProperties( _valueType );
			foreach( var prop in properties )
			{
				if( !prop.CanRead || !prop.CanWrite )
					continue;

				if( ( !prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic ) && IEnumerableExt.count( prop.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				var inspector = getInspectorForType( prop.PropertyType, _target, prop );
				if( inspector != null )
				{
					inspector.setStructTarget( _target, this, prop );
					inspector.initialize( table, skin, leftCellWidth );
					_inspectors.Add( inspector );
					table.row().setPadLeft( 15 );
				}
			}
		}


		public override void update()
		{
			foreach( var i in _inspectors )
				i.update();
		}

	}
}
#endif