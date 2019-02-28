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
		static string _entityName = "";

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
						drawEntity( Core.scene.entities[i] );
				}

				ImGui.End();
			}
		}

		static void drawEntity( Entity entity, bool onlyDrawRoots = true )
		{
			if( onlyDrawRoots && entity.transform.parent != null )
				return;

			ImGui.PushID( (int)entity.id );
			var treeNodeOpened = false;
			if( entity.transform.childCount > 0 )
			{
				treeNodeOpened = ImGui.TreeNodeEx( $"{entity.name} ({entity.transform.childCount})###{entity.id}", ImGuiTreeNodeFlags.OpenOnArrow );
			}
			else
			{
				treeNodeOpened = ImGui.TreeNodeEx( $"{entity.name} ({entity.transform.childCount})###{entity.id}", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.OpenOnArrow );
			}

			// context menu for entity commands
			ImGui.OpenPopupOnItemClick( "entityContextMenu", 1 );
			if( ImGui.BeginPopup( "entityContextMenu" ) )
			{
				if( ImGui.Selectable( "Clone Entity " + entity.name ) )
				{
					var clone = entity.clone();
					entity.scene.addEntity( clone );
				}

				if( ImGui.Selectable( "Destroy Entity" ) )
					entity.destroy();

				if( ImGui.Button( "Create Child Entity" ) )
					ImGui.OpenPopup( "create-entity" );

				if( ImGui.BeginPopup( "create-entity" ) )
				{
					ImGui.Text( "New Entity Name:" );
					ImGui.InputText( "##cloneEntityName", ref _entityName, 25 );

					if( ImGui.Button( "Cancel") )
					{
						_entityName = "";
						ImGui.CloseCurrentPopup();
					}
					
					ImGui.SameLine( ImGui.GetContentRegionAvailWidth() - ImGui.GetItemRectSize().X );

					ImGui.PushStyleColor( ImGuiCol.Button, Microsoft.Xna.Framework.Color.Green.PackedValue );
					if( ImGui.Button( "Create" ) )
					{
						var newEntity = new Entity( _entityName );
						newEntity.transform.setParent( entity.transform );
						entity.scene.addEntity( newEntity );

						_entityName = "";
						ImGui.CloseCurrentPopup();
					}
					ImGui.PopStyleColor();

					ImGui.EndPopup();
				}

				ImGui.EndPopup();
			}

			// we are looking for a double-click that is not on the arrow
			if( ImGui.IsMouseDoubleClicked( 0 ) && ImGui.IsItemClicked() && ( ImGui.GetMousePos().X - ImGui.GetItemRectMin().X ) > ImGui.GetTreeNodeToLabelSpacing() )
			{
				Core.getGlobalManager<ImGuiManager>().startInspectingEntity( entity );
			}

			if( treeNodeOpened )
			{
				for( var i = 0; i < entity.transform.childCount; i++ )
					drawEntity( entity.transform.getChild( i ).entity, false );

				ImGui.TreePop();
			}
			ImGui.PopID();
		}

		static void drawPostProcessors()
		{
			// first, we check our list of inspectors and sync it up with the current list of PostProcessors in the Scene.
			// we limit the check to once every 60 fames
			if( Time.frameCount % 60 == 0 )
			{
				for( var i = 0; i < Core.scene.rawPostProcessorList.length; i++ )
				{
					var postProcessor = Core.scene.rawPostProcessorList.buffer[i];
					if( _postProcessorInspectors.Where( inspector => inspector.postProcessor == postProcessor ).Count() == 0 )
						_postProcessorInspectors.Add( new PostProcessorInspector( postProcessor ) );
				}
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
					NezImGui.SmallVerticalSpace();
				}
			}
		}

	}
}
