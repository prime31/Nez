using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;


namespace Nez.ImGuiTools.ObjectInspectors
{
	public class RendererInspector
	{
		public Renderer Renderer => _renderer;

		int _scopeId = NezImGui.GetScopeId();
		string _name;
		Renderer _renderer;
		MaterialInspector _materialInspector;

		public RendererInspector(Renderer renderer)
		{
			_renderer = renderer;
			_name = _renderer.GetType().Name;
			_materialInspector = new MaterialInspector
			{
				AllowsMaterialRemoval = false
			};
			_materialInspector.SetTarget(renderer, renderer.GetType().GetField("Material"));
		}

		public void Draw()
		{
			ImGui.PushID(_scopeId);
			var isOpen = ImGui.CollapsingHeader(_name);

			NezImGui.ShowContextMenuTooltip();

			if (ImGui.BeginPopupContextItem())
			{
				if (ImGui.Selectable("Remove Renderer"))
				{
					isOpen = false;
					Core.Scene.RemoveRenderer(_renderer);
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}

			if (isOpen)
			{
				ImGui.Indent();

				_materialInspector.Draw();

				ImGui.Checkbox("shouldDebugRender", ref Renderer.ShouldDebugRender);

				var value = Renderer.RenderTargetClearColor.ToNumerics();
				if (ImGui.ColorEdit4("renderTargetClearColor", ref value))
					Renderer.RenderTargetClearColor = value.ToXNAColor();

				if (Renderer.Camera != null)
				{
					if (NezImGui.LabelButton("Camera", Renderer.Camera.Entity.Name))
						Core.GetGlobalManager<ImGuiManager>().StartInspectingEntity(Renderer.Camera.Entity);
				}

				ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
				NezImGui.DisableNextWidget();
				var tempBool = Renderer.WantsToRenderToSceneRenderTarget;
				ImGui.Checkbox("wantsToRenderToSceneRenderTarget", ref tempBool);

				NezImGui.DisableNextWidget();
				tempBool = Renderer.WantsToRenderAfterPostProcessors;
				ImGui.Checkbox("wantsToRenderAfterPostProcessors", ref tempBool);
				ImGui.PopStyleVar();

				ImGui.Unindent();
			}

			ImGui.PopID();
		}
	}
}