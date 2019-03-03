using ImGuiNET;
using Num = System.Numerics;

namespace Nez.ImGuiTools.ObjectInspectors
{
	public class TransformInspector
	{
		Transform _transform;

		public TransformInspector( Transform transform )
		{
			_transform = transform;
		}

		public void draw()
		{
			if( ImGui.CollapsingHeader( "Transform", ImGuiTreeNodeFlags.DefaultOpen ) )
			{
                ImGui.LabelText( "Children", _transform.childCount.ToString() );
                
                if( _transform.parent == null )
                {
                    ImGui.LabelText( "Parent", "none" );
                }
                else
                {
                    if( NezImGui.LabelButton( "Parent", _transform.parent.entity.name ) )
                        Core.getGlobalManager<ImGuiManager>().startInspectingEntity( _transform.parent.entity );

                    if( ImGui.Button( "Detach From Parent" ) )
                        _transform.parent = null;
                }

                NezImGui.SmallVerticalSpace();

                var pos = _transform.localPosition.toNumerics();
                if( ImGui.DragFloat2( "Local Position", ref pos ) )
                    _transform.setLocalPosition( pos.toXNA() );
                
                var rot = _transform.localRotationDegrees;
                if( ImGui.DragFloat( "Local Rotation", ref rot, 1, -360, 360 ) )
                    _transform.setLocalRotationDegrees( rot );
                
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