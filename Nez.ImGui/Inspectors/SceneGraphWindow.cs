using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Nez.ImGuiTools.ComponentInspectors;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
	static class SceneGraphWindow
	{
        static List<PostProcessorInspector> _postProcessorInspectors = new List<PostProcessorInspector>();
        
        public static void onSceneChanged()
        {
            _postProcessorInspectors.Clear();
        }

		public static void show( ref bool isOpen )
		{
			if( Core.scene == null || !isOpen )
				return;

			ImGui.SetNextWindowPos( new Num.Vector2( 0, 25 ), ImGuiCond.FirstUseEver );
			ImGui.SetNextWindowSize( new Num.Vector2( 300, Screen.height / 2 ), ImGuiCond.FirstUseEver );

			if( ImGui.Begin( "Scene Graph", ref isOpen ) )
			{
				if( Core.scene.rawPostProcessorList.length > 0 && ImGui.CollapsingHeader( "Post Processors" ) )
				{
                    drawPostProcessors();
				}

				if( ImGui.CollapsingHeader( "Entities (double-click label to inspect)", ImGuiTreeNodeFlags.DefaultOpen ) )
				{
					for( var i = Core.scene.entities.count - 1; i >= 0; i-- )
					{
						drawEntity( Core.scene.entities[i] );
					}
				}

				ImGui.End();
			}
		}

		static void drawEntity( Entity entity, bool onlyDrawRoots = true )
		{
			if( onlyDrawRoots && entity.transform.parent != null )
				return;

			var treeNodeOpened = false;
			if( entity.transform.childCount > 0 )
			{
				treeNodeOpened = ImGui.TreeNodeEx( $"{entity.name} ({entity.transform.childCount})", ImGuiTreeNodeFlags.OpenOnArrow );
			}
			else
			{
				treeNodeOpened = ImGui.TreeNodeEx( $"{entity.name} ({entity.transform.childCount})", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.OpenOnArrow );
			}

            // we are looking for a double-click that is not on the arrow
			if( ImGui.IsMouseDoubleClicked( 0 ) && ImGui.IsItemClicked() && ( ImGui.GetMousePos().X - ImGui.GetItemRectMin().X ) > ImGui.GetTreeNodeToLabelSpacing() )
				Core.getGlobalManager<ImGuiManager>().startInspectingEntity( entity );

			if( treeNodeOpened )
			{
				for( var i = 0; i < entity.transform.childCount; i++ )
					drawEntity( entity.transform.getChild( i ).entity, false );

				ImGui.TreePop();
			}
		}
	
        static void drawPostProcessors()
        {
            // first, we check our list of inspectors and sync it up with the current list of PostProcessors in the Scene
            for( var i = 0; i < Core.scene.rawPostProcessorList.length; i++ )
            {
                var postProcessor = Core.scene.rawPostProcessorList.buffer[i];
                if( _postProcessorInspectors.Where( inspector => inspector.postProcessor == postProcessor ).Count() == 0 )
                    _postProcessorInspectors.Add( new PostProcessorInspector( postProcessor ) );
            }

            for( var i = _postProcessorInspectors.Count - 1; i >= 0; i-- )
            {
                if( !_postProcessorInspectors[i].postProcessor.isAttachedToScene )
                {
                    _postProcessorInspectors.RemoveAt( i );
                }
                else
                {
                    _postProcessorInspectors[i].draw();
                    NezImGui.smallVerticalSpace();
                }
            }
        }

    }
}
