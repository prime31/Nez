using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;


namespace Nez.ImGuiTools.ObjectInspectors
{
	public class PostProcessorInspector
	{
		public PostProcessor PostProcessor => _postProcessor;

		protected List<AbstractTypeInspector> _inspectors;
		protected int _scopeId = NezImGui.GetScopeId();

		PostProcessor _postProcessor;

		public PostProcessorInspector(PostProcessor postProcessor)
		{
			_postProcessor = postProcessor;
			_inspectors = TypeInspectorUtils.GetInspectableProperties(postProcessor);

			// if we are a Material<T>, we need to fix the duplicate Effect due to the "new T effect"
			if (ReflectionUtils.IsGenericTypeOrSubclassOfGenericType(_postProcessor.GetType()))
			{
				var didFindEffectInspector = false;
				for (var i = 0; i < _inspectors.Count; i++)
				{
					var isEffectInspector = _inspectors[i] is Nez.ImGuiTools.TypeInspectors.EffectInspector;
					if (isEffectInspector)
					{
						if (didFindEffectInspector)
						{
							_inspectors.RemoveAt(i);
							break;
						}

						didFindEffectInspector = true;
					}
				}
			}

			for (var i = 0; i < _inspectors.Count; i++)
			{
				var effectInspector = _inspectors[i] as Nez.ImGuiTools.TypeInspectors.EffectInspector;
				if (effectInspector != null)
					effectInspector.AllowsEffectRemoval = false;
			}
		}

		public void Draw()
		{
			ImGui.PushID(_scopeId);
			var isOpen = ImGui.CollapsingHeader(_postProcessor.GetType().Name.Replace("PostProcessor", string.Empty));

			NezImGui.ShowContextMenuTooltip();

			if (ImGui.BeginPopupContextItem())
			{
				if (ImGui.Selectable("Remove PostProcessor"))
				{
					isOpen = false;
					Core.Scene.RemovePostProcessor(_postProcessor);
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