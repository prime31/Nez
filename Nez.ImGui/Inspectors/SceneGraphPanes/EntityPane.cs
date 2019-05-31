using System;
using ImGuiNET;

namespace Nez.ImGuiTools.SceneGraphPanes
{
    public class EntityPane
    {
        string _newEntityName = "";

        public void draw()
        {
            for( var i = 0; i < Core.scene.entities.count; i++ )
                drawEntity( Core.scene.entities[i] );

			NezImGui.MediumVerticalSpace();
			if( NezImGui.CenteredButton( "Create Entity", 0.6f ) )
			{
				ImGui.OpenPopup( "create-entity" );
			}

			drawCreateEntityPopup();
        }

		void drawEntity( Entity entity, bool onlyDrawRoots = true )
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

			NezImGui.ShowContextMenuTooltip();

			// context menu for entity commands
			ImGui.OpenPopupOnItemClick( "entityContextMenu", 1 );
			drawEntityContextMenuPopup( entity );

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

		void drawEntityContextMenuPopup( Entity entity )
		{
			if( ImGui.BeginPopup( "entityContextMenu" ) )
			{
				if( ImGui.Selectable( "Clone Entity " + entity.name ) )
				{
					var clone = entity.clone( Core.scene.camera.position );
					entity.scene.addEntity( clone );
				}

				if( ImGui.Selectable( "Destroy Entity" ) )
					entity.destroy();

				if( ImGui.Selectable( "Create Child Entity", false, ImGuiSelectableFlags.DontClosePopups ) )
					ImGui.OpenPopup( "create-new-entity" );

				if( ImGui.BeginPopup( "create-new-entity" ) )
				{
					ImGui.Text( "New Entity Name:" );
					ImGui.InputText( "##newChildEntityName", ref _newEntityName, 25 );

					if( ImGui.Button( "Cancel") )
					{
						_newEntityName = "";
						ImGui.CloseCurrentPopup();
					}
					
					ImGui.SameLine( ImGui.GetContentRegionAvailWidth() - ImGui.GetItemRectSize().X );

					ImGui.PushStyleColor( ImGuiCol.Button, Microsoft.Xna.Framework.Color.Green.PackedValue );
					if( ImGui.Button( "Create" ) )
					{
						_newEntityName = _newEntityName.Length > 0 ? _newEntityName : Utils.randomString( 8 );
						var newEntity = new Entity( _newEntityName );
						newEntity.transform.setParent( entity.transform );
						entity.scene.addEntity( newEntity );

						_newEntityName = "";
						ImGui.CloseCurrentPopup();
					}
					ImGui.PopStyleColor();

					ImGui.EndPopup();
				}

				ImGui.EndPopup();
			}
		}

		void drawCreateEntityPopup()
		{
			if( ImGui.BeginPopup( "create-entity" ) )
			{
					ImGui.Text( "New Entity Name:" );
					ImGui.InputText( "##newEntityName", ref _newEntityName, 25 );

					if( ImGui.Button( "Cancel") )
					{
						_newEntityName = "";
						ImGui.CloseCurrentPopup();
					}
					
					ImGui.SameLine( ImGui.GetContentRegionAvailWidth() - ImGui.GetItemRectSize().X );

					ImGui.PushStyleColor( ImGuiCol.Button, Microsoft.Xna.Framework.Color.Green.PackedValue );
					if( ImGui.Button( "Create" ) )
					{
						_newEntityName = _newEntityName.Length > 0 ? _newEntityName : Utils.randomString( 8 );
						var newEntity = new Entity( _newEntityName );
						newEntity.transform.position = Core.scene.camera.transform.position;
						Core.scene.addEntity( newEntity );

						_newEntityName = "";
						ImGui.CloseCurrentPopup();
					}
					ImGui.PopStyleColor();
			}
		}

    }
}