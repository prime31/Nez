using System;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ComponentInspectors
{
	public class ComponentInspector : AbstractComponentInspector
	{
        Component _component;
        List<Action> _componentDelegateMethods = new List<Action>();

        public ComponentInspector( Component component )
        {
            _component = component;
            _inspectors = TypeInspectorUtils.getInspectableProperties( component );
            
            var methods = TypeInspectorUtils.GetAllMethodsWithAttribute<InspectorDelegateAttribute>( _component.GetType() );
            foreach( var method in methods )
            {
                // only allow zero param methods
                if( method.GetParameters().Length == 0 )
                {
                    _componentDelegateMethods.Add( (Action)Delegate.CreateDelegate( typeof( Action ), _component, method ) );
                }
            }
        }

		public override void draw()
		{
            ImGui.PushID( _scopeId );
            if( ImGui.CollapsingHeader( _component.GetType().Name ) )
            {
                var enabled = _component.enabled;
                if( ImGui.Checkbox( "Enabled", ref enabled ) )
                {
                    _component.setEnabled( enabled );
                }

                foreach( var inspector in _inspectors )
                {
                    inspector.draw();
                }
                
                foreach( var action in _componentDelegateMethods )
                {
                    action();
                }
            }
            ImGui.PopID();
		}
	}
}
