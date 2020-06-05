using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Nez.ImGuiTools.ObjectInspectors;


namespace Nez.ImGuiTools.SceneGraphPanes
{
	/// <summary>
	/// manages displaying the current Renderers in the Scene
	/// </summary>
	public class RenderersPane
	{
		List<RendererInspector> _renderers = new List<RendererInspector>();
		bool _isRendererListInitialized;

		// Used to hold a reference to the Cast scene, if it derives from `Scene`
		Scene CastScene;

		void UpdateRenderersPaneList()
		{
			// first, we check our list of inspectors and sync it up with the current list of PostProcessors in the Scene.
			// we limit the check to once every 60 fames
			if (!_isRendererListInitialized || Time.FrameCount % 60 == 0)
			{
				_isRendererListInitialized = true;
				for (var i = 0; i < CastScene._renderers.Length; i++)
				{
					var renderer = CastScene._renderers.Buffer[i];
					if (_renderers.Where(inspector => inspector.Renderer == renderer).Count() == 0)
						_renderers.Add(new RendererInspector(renderer));
				}
			}
		}

		public void OnSceneChanged()
		{
			CastScene = Core.Scene as Scene;
			if (CastScene == null)
				throw new InvalidOperationException("You cannot use Post Processors if you are not inheriting the base Scene implementation.");

			_renderers.Clear();
			_isRendererListInitialized = false;
			UpdateRenderersPaneList();
		}

		public void Draw()
		{
			UpdateRenderersPaneList();

			ImGui.Indent();
			for (var i = 0; i < _renderers.Count; i++)
			{
				_renderers[i].Draw();
				NezImGui.SmallVerticalSpace();
			}

			if (_renderers.Count == 0)
				NezImGui.SmallVerticalSpace();

			ImGui.Unindent();
		}
	}
}