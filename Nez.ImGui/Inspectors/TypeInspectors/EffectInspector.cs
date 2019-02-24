using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.IEnumerableExtensions;


namespace Nez.ImGuiTools.TypeInspectors
{
	public class EffectInspector : AbstractTypeInspector
	{
		List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

		public override void initialize()
		{
			base.initialize();

			// we either have a getter that gets a Material or an Effect
			var effect = _valueType == typeof( Material ) ? getValue<Material>().effect : getValue<Effect>();
			if( effect == null )
				return;

			_name = effect.GetType().Name;

			// figure out which properties are useful to add to the inspector
			var effectProps = ReflectionUtils.getProperties( effect.GetType() );
			foreach( var prop in effectProps )
			{
				if( prop.DeclaringType == typeof( Effect ) )
					continue;

				if( !prop.CanRead || !prop.CanWrite || prop.Name == "Name" )
					continue;

				if( ( !prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic ) && IEnumerableExt.count( prop.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				var inspector = TypeInspectorUtils.getInspectorForType( prop.PropertyType, effect, prop );
				if( inspector != null )
				{
					inspector.setTarget( effect, prop );
					inspector.initialize();
					_inspectors.Add( inspector );
				}
			}
		}

		public override void drawMutable()
		{
			ImGui.Indent();
			if( ImGui.CollapsingHeader( $"{_name}", ImGuiTreeNodeFlags.FramePadding ) )
			{
				foreach( var i in _inspectors )
					i.draw();
			}
			ImGui.Unindent();
		}
	}
}
