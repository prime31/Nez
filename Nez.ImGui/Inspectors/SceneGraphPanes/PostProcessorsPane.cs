using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Nez.ImGuiTools.ObjectInspectors;

namespace Nez.ImGuiTools.SceneGraphPanes
{
	/// <summary>
	/// manages displaying the current PostProcessors in the Scene and provides a means to add PostProcessors
	/// </summary>
    public class PostProcessorsPane
    {
        List<PostProcessorInspector> _postProcessorInspectors = new List<PostProcessorInspector>();
		bool _isPostProcessorListInitialized;

        void updatePostProcessorInspectorList()
        {
			// first, we check our list of inspectors and sync it up with the current list of PostProcessors in the Scene.
			// we limit the check to once every 60 fames
			if( !_isPostProcessorListInitialized || Time.frameCount % 60 == 0 )
			{
				_isPostProcessorListInitialized = true;
				for( var i = 0; i < Core.scene._postProcessors.length; i++ )
				{
					var postProcessor = Core.scene._postProcessors.buffer[i];
					if( _postProcessorInspectors.Where( inspector => inspector.postProcessor == postProcessor ).Count() == 0 )
						_postProcessorInspectors.Add( new PostProcessorInspector( postProcessor ) );
				}
			}
        }

        public void onSceneChanged()
        {
            _postProcessorInspectors.Clear();
			_isPostProcessorListInitialized = false;
            updatePostProcessorInspectorList();
        }

        public void draw()
        {
            updatePostProcessorInspectorList();

			ImGui.Indent();
			for( var i = 0; i < _postProcessorInspectors.Count; i++ )
			{
				if( _postProcessorInspectors[i].postProcessor._scene != null )
				{
					_postProcessorInspectors[i].draw();
					NezImGui.SmallVerticalSpace();
				}
			}

			if( _postProcessorInspectors.Count == 0 )
				NezImGui.SmallVerticalSpace();

			if( NezImGui.CenteredButton( "Add PostProcessor", 0.6f ) )
			{
				ImGui.OpenPopup( "postprocessor-selector" );
			}

			ImGui.Unindent();

			NezImGui.MediumVerticalSpace();
            drawPostProcessorSelectorPopup();
        }

        void drawPostProcessorSelectorPopup()
        {
			if( ImGui.BeginPopup( "postprocessor-selector" ) )
			{
				foreach( var subclassType in InspectorCache.getAllPostProcessorSubclassTypes() )
				{
					if( ImGui.Selectable( subclassType.Name ) )
					{
						var postprocessor = (PostProcessor)Activator.CreateInstance( subclassType, new object[] { _postProcessorInspectors.Count } );
						Core.scene.addPostProcessor( postprocessor );
						_isPostProcessorListInitialized = false;
					}
				}
				ImGui.EndPopup();
			}
        }
 
    }
}