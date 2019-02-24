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
		bool _isHeaderOpen;

		public override void initialize()
		{
			base.initialize();

			// figure out which fields and properties are useful to add to the inspector
			var fields = ReflectionUtils.getFields( _valueType );
			foreach( var field in fields )
			{
				if( !field.IsPublic && !field.IsDefined( typeof( InspectableAttribute ) ) )
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

				var isPropertyUndefinedOrPublic = !prop.CanWrite || ( prop.CanWrite && prop.SetMethod.IsPublic );
				if( ( !prop.GetMethod.IsPublic || !isPropertyUndefinedOrPublic ) && !prop.IsDefined( typeof( InspectableAttribute ) ) )
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

		public override void drawMutable()
		{
			ImGui.Indent();
			NezImGui.BeginBorderedGroup();
			
			_isHeaderOpen = ImGui.CollapsingHeader( $"{_name}" );
			if( _isHeaderOpen )
			{
				foreach( var i in _inspectors )
					i.draw();
			}
			NezImGui.EndBorderedGroup( new System.Numerics.Vector2( 4, 1 ), new System.Numerics.Vector2( 4, 2 ) );
			ImGui.Unindent();
		}

		/// <summary>
		/// we need to override here so that we can keep the header enabled so that it can be opened
		/// </summary>
		public override void drawReadOnly()
		{
			drawMutable();
		}

	}
}
