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

        void updateRenderersPaneList()
        {
			// first, we check our list of inspectors and sync it up with the current list of PostProcessors in the Scene.
			// we limit the check to once every 60 fames
			if( !_isRendererListInitialized || Time.frameCount % 60 == 0 )
			{
				_isRendererListInitialized = true;
				for( var i = 0; i < Core.scene._renderers.length; i++ )
				{
					var renderer = Core.scene._renderers.buffer[i];
					if( _renderers.Where( inspector => inspector.renderer == renderer ).Count() == 0 )
						_renderers.Add( new RendererInspector( renderer ) );
				}
			}
        }

        public void onSceneChanged()
        {
            _renderers.Clear();
			_isRendererListInitialized = false;
            updateRenderersPaneList();
        }

        public void draw()
        {
            updateRenderersPaneList();

			ImGui.Indent();
			for( var i = 0; i < _renderers.Count; i++ )
			{
				_renderers[i].draw();
				NezImGui.SmallVerticalSpace();
			}

			if( _renderers.Count == 0 )
				NezImGui.SmallVerticalSpace();

			ImGui.Unindent();
        }
 
    }
}