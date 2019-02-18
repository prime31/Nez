using System;
using ImGuiNET;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
    static class SceneGraphWindow
    {
        public static void show( ref bool isOpen )
        {
            if( Core.scene == null || !isOpen )
                return;

            ImGui.SetNextWindowPos( new Num.Vector2( 10, 10 ), ImGuiCond.FirstUseEver );
            ImGui.SetNextWindowSize( new Num.Vector2( 300, Screen.height / 2 ), ImGuiCond.FirstUseEver );
            ImGui.Begin( "Scene Graph", ref isOpen );

            ImGui.Text( "Entity List" );
            for( var i = 0; i < Core.scene.entities.count; i++ )
            {
                drawEntity( Core.scene.entities[i] );
            }

            ImGui.End();
        }

		static void drawEntity( Entity entity )
		{
			if( ImGui.TreeNode( entity.name ) )
            {
                ImGui.Text( $"Update Inverval: {entity.updateInterval}" );
                ImGui.Text( $"Tag: {entity.tag}" );

                ImGui.TreePop();
            }
		}
	}
}
