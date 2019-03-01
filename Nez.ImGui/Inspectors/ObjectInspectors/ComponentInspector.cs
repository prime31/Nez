using System;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ObjectInspectors
{
	public class ComponentInspector : AbstractComponentInspector
	{
		public override Entity entity => _component.entity;
		public override Component component => _component;

		Component _component;
        string _name;
        List<Action> _componentDelegateMethods = new List<Action>();

        public ComponentInspector( Component component )
        {
            _component = component;
            _inspectors = TypeInspectorUtils.getInspectableProperties( component );

            if( _component.GetType().IsGenericType )
            {
                var genericType = _component.GetType().GetGenericArguments()[0].Name;
                _name = $"{_component.GetType().BaseType.Name}<{genericType}>";
            }
            else
            {
                _name = _component.GetType().Name;
            }
            
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
            var isHeaderOpen = ImGui.CollapsingHeader( _name );

            // context menu has to be outside the isHeaderOpen block so it works open or closed
            if( ImGui.BeginPopupContextItem() )
            {
                if( ImGui.Selectable( "Remove Component" ) )
                {
                    _component.removeComponent();
                }
                ImGui.EndPopup();
            }

            if( isHeaderOpen )
            {
                var enabled = _component.enabled;
                if( ImGui.Checkbox( "Enabled", ref enabled ) )
                {
                    _component.setEnabled( enabled );
                }

                for( var i = _inspectors.Count - 1; i >= 0; i-- )
                {
                    if( _inspectors[i].isTargetDestroyed )
                    {
                        _inspectors.RemoveAt( i );
                        continue;
                    }
                    _inspectors[i].draw();
                }
                
                foreach( var action in _componentDelegateMethods )
                    action();
            }
            ImGui.PopID();
		}
	
    }
}
