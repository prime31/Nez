using ImGuiNET;
using Num = System.Numerics;

namespace Nez.ImGuiTools.ComponentInspectors
{
	public class TransformInspector : IComponentInspector
	{
		Transform _transform;

		public TransformInspector( Transform transform )
		{
			_transform = transform;
		}

		void IComponentInspector.draw()
		{
			if( ImGui.CollapsingHeader( "Transform", ImGuiTreeNodeFlags.DefaultOpen ) )
			{
                ImGui.Indent();
                ImGui.Text( $"Children: {_transform.childCount}" );
                ImGui.Text( "Parent: " );
                ImGui.SameLine();
                if( _transform.parent == null )
                {
                    ImGui.Text( "none" );
                }
                else
                {
                    if( ImGui.SmallButton( _transform.parent.entity.name ) )
					    Core.getGlobalManager<ImGuiManager>().startInspectingEntity( _transform.parent.entity );
                }
                ImGui.Unindent();

                NezImGui.smallVerticalSpace();

                var pos = _transform.localPosition.toNumerics();
                if( ImGui.DragFloat2( "Local Position", ref pos ) )
                    _transform.setLocalPosition( pos.toXNA() );
                
                var rot = _transform.localRotation;
                if( ImGui.SliderAngle( "Local Rotation", ref rot ) )
                    _transform.setLocalRotation( rot );
                
                var scale = _transform.localScale.toNumerics();
                if( ImGui.DragFloat2( "Local Scale", ref scale, 0.05f ) )
                    _transform.setLocalScale( scale.toXNA() );

                scale = _transform.scale.toNumerics();
                if( ImGui.DragFloat2( "Scale", ref scale, 0.05f ) )
                    _transform.setScale( scale.toXNA() );
            }
		}
	}
}