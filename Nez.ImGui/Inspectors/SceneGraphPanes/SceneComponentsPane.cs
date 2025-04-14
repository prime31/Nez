using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Nez.ImGuiTools.ObjectInspectors;


namespace Nez.ImGuiTools.SceneGraphPanes
{
	/// <summary>
	/// manages displaying the current SceneComponents in the Scene and provides a means to add SceneComponents
	/// </summary>
	public class SceneComponentsPane
	{
		List<SceneComponentInspector> _sceneComponentInspectors = new List<SceneComponentInspector>();
		bool _isSceneComponentListInitialized;

		void UpdateSceneComponentInspectorList()
		{
			// first, we check our list of inspectors and sync it up with the current list of SceneComponents in the Scene.
			// we limit the check to once every 60 fames
			if (!_isSceneComponentListInitialized || Time.FrameCount % 60 == 0)
			{
				_isSceneComponentListInitialized = true;

				for (var i = 0; i < Core.Scene._sceneComponents.Length; i++)
				{
					var sceneComponent = Core.Scene._sceneComponents.Buffer[i];
					if (_sceneComponentInspectors.Where(inspector => inspector.SceneComponent == sceneComponent).Count() == 0)
						_sceneComponentInspectors.Add(new SceneComponentInspector(sceneComponent));
				}
			}
		}

		public void OnSceneChanged()
		{
			_sceneComponentInspectors.Clear();
			_isSceneComponentListInitialized = false;
			UpdateSceneComponentInspectorList();
		}

		public void Draw()
		{
			UpdateSceneComponentInspectorList();

			ImGui.Indent();

			for (var i = 0; i < _sceneComponentInspectors.Count; i++)
			{
				// watch out for removed SceneComponents
				if (_sceneComponentInspectors[i].SceneComponent.Scene == null)
				{
					_sceneComponentInspectors.RemoveAt(i);
					continue;
				}
				
				_sceneComponentInspectors[i].Draw();
				NezImGui.SmallVerticalSpace();
			}

			if (_sceneComponentInspectors.Count == 0)
				NezImGui.SmallVerticalSpace();

			ImGui.Unindent();
		}
	}
}