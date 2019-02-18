using System;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ComponentInspectors
{
	public class ComponentInspector : AbstractComponentInspector
	{
        Component _component;

        public ComponentInspector( Component component )
        {
            _component = component;
            _inspectors = TypeInspectorUtils.getInspectableProperties( component );
        }

		public override void draw()
		{
            ImGui.PushID( _scopeId );
            if( ImGui.CollapsingHeader( _component.GetType().Name ) )
            {
                var enabled = _component.enabled;
                if( ImGui.Checkbox( "Enabled", ref enabled ) )
                    _component.setEnabled( enabled );

                foreach( var inspector in _inspectors )
                    inspector.draw();
            }
            ImGui.PopID();
		}
	}
}
