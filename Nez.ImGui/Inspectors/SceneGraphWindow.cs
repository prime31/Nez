using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Nez.ImGuiTools.ObjectInspectors;
using Nez.ImGuiTools.SceneGraphPanes;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
	class SceneGraphWindow
	{
		PostProcessorsPane _postProcessorsPane = new PostProcessorsPane();
		RenderersPane _renderersPane = new RenderersPane();
		EntityPane _entityPane = new EntityPane();

		public void onSceneChanged()
		{
			_postProcessorsPane.onSceneChanged();
			_renderersPane.onSceneChanged();
		}

		public void show( ref bool isOpen )
		{
			if( Core.scene == null || !isOpen )
				return;

			ImGui.SetNextWindowPos( new Num.Vector2( 0, 25 ), ImGuiCond.FirstUseEver );
			ImGui.SetNextWindowSize( new Num.Vector2( 300, Screen.height / 2 ), ImGuiCond.FirstUseEver );

			if( ImGui.Begin( "Scene Graph", ref isOpen ) )
			{
				if( ImGui.CollapsingHeader( "Post Processors" ) )
					_postProcessorsPane.draw();

				if( ImGui.CollapsingHeader( "Renderers" ) )
					_renderersPane.draw();

				if( ImGui.CollapsingHeader( "Entities (double-click label to inspect)", ImGuiTreeNodeFlags.DefaultOpen ) )
					_entityPane.draw();

				ImGui.End();
			}
		}

	}
}
