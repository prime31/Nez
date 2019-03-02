using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ObjectInspectors
{
    public class RendererInspector
    {
        public Renderer renderer => _renderer;

		int _scopeId = NezImGui.GetScopeId();
        string _name;
        Renderer _renderer;
        MaterialInspector _materialInspector;

        public RendererInspector( Renderer renderer )
        {
            _renderer = renderer;
            _name = _renderer.GetType().Name;
            _materialInspector = new MaterialInspector {
                allowsMaterialRemoval = false
            };
            _materialInspector.setTarget( renderer, renderer.GetType().GetField( "material" ) );
        }

        public void draw()
        {
            ImGui.PushID( _scopeId );
            var isOpen = ImGui.CollapsingHeader( _name );
            
            NezImGui.ShowContextMenuTooltip();

			if( ImGui.BeginPopupContextItem() )
			{
				if( ImGui.Selectable( "Remove Renderer" ) )
				{
                    isOpen = false;
                    Core.scene.removeRenderer( _renderer );
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}

            if( isOpen )
            {
                ImGui.Indent();

                _materialInspector.draw();
                
                ImGui.Checkbox( "shouldDebugRender", ref renderer.shouldDebugRender );

			    var value = renderer.renderTargetClearColor.toNumerics();
			    if( ImGui.ColorEdit4( "renderTargetClearColor", ref value ) )
				    renderer.renderTargetClearColor = value.toXNAColor();
                
                if( renderer.camera != null )
                {
                    if( NezImGui.LabelButton( "Camera", renderer.camera.entity.name ) )
				        Core.getGlobalManager<ImGuiManager>().startInspectingEntity( renderer.camera.entity );
                }

                ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
                NezImGui.DisableNextWidget();
                var tempBool = renderer.wantsToRenderToSceneRenderTarget;
                ImGui.Checkbox( "wantsToRenderToSceneRenderTarget", ref tempBool );

                NezImGui.DisableNextWidget();
                tempBool = renderer.wantsToRenderAfterPostProcessors;
                ImGui.Checkbox( "wantsToRenderAfterPostProcessors", ref tempBool );
                ImGui.PopStyleVar();
                
                ImGui.Unindent();
            }

            ImGui.PopID();
        }

    }
}
