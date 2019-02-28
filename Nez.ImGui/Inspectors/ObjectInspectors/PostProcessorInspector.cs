using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ComponentInspectors
{
    public class PostProcessorInspector
    {
        public PostProcessor postProcessor => _postProcessor;

        protected List<AbstractTypeInspector> _inspectors;
		protected int _scopeId = NezImGui.SetScopeId();

        PostProcessor _postProcessor;

        public PostProcessorInspector( PostProcessor postProcessor )
        {
            _postProcessor = postProcessor;
            _inspectors = TypeInspectorUtils.getInspectableProperties( postProcessor );
        }

        public void draw()
        {
            ImGui.PushID( _scopeId );
            ImGui.Indent();
            if( ImGui.CollapsingHeader( _postProcessor.GetType().Name.Replace( "PostProcessor", string.Empty ) ) )
            {
                ImGui.Checkbox( "Enabled", ref _postProcessor.enabled );
                foreach( var inspector in _inspectors )
                    inspector.draw();
            }
            ImGui.Unindent();
            ImGui.PopID();
        }
    }
}
