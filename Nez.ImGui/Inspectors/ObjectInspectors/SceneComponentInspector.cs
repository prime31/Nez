using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;


namespace Nez.ImGuiTools.ObjectInspectors
{
	public class SceneComponentInspector
	{
		public SceneComponent SceneComponent => _sceneComponent;
		protected List<AbstractTypeInspector> _inspectors;

		protected int _scopeId = NezImGui.GetScopeId();
		string _name;
		SceneComponent _sceneComponent;

		public SceneComponentInspector(SceneComponent sceneComponent)
		{
			_sceneComponent = sceneComponent;
			_inspectors = TypeInspectorUtils.GetInspectableProperties(sceneComponent);
			_name = _sceneComponent.GetType().Name;
		}

		public void Draw()
		{
			ImGui.PushID(_scopeId);
			var isOpen = ImGui.CollapsingHeader(_name);

			NezImGui.ShowContextMenuTooltip();

			if (ImGui.BeginPopupContextItem())
			{
				if (ImGui.Selectable("Remove SceneComponent"))
				{
					isOpen = false;
					Core.Scene.RemoveSceneComponent(_sceneComponent);
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}

			if (isOpen)
			{
				ImGui.Indent();
				foreach (var inspector in _inspectors)
					inspector.Draw();
				ImGui.Unindent();
			}

			ImGui.PopID();
		}
	}
}