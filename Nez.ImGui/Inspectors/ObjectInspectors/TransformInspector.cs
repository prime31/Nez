using ImGuiNET;
using Num = System.Numerics;


namespace Nez.ImGuiTools.ObjectInspectors
{
	public class TransformInspector
	{
		Transform _transform;

		public TransformInspector(Transform transform)
		{
			_transform = transform;
		}

		public void Draw()
		{
			if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
			{
				ImGui.LabelText("Children", _transform.ChildCount.ToString());

				if (_transform.Parent == null)
				{
					ImGui.LabelText("Parent", "none");
				}
				else
				{
					if (NezImGui.LabelButton("Parent", _transform.Parent.Entity.Name))
						Core.GetGlobalManager<ImGuiManager>().StartInspectingEntity(_transform.Parent.Entity);

					if (ImGui.Button("Detach From Parent"))
						_transform.Parent = null;
				}

				NezImGui.SmallVerticalSpace();

				var pos = _transform.LocalPosition.ToNumerics();
				if (ImGui.DragFloat2("Local Position", ref pos))
					_transform.SetLocalPosition(pos.ToXNA());

				var rot = _transform.LocalRotationDegrees;
				if (ImGui.DragFloat("Local Rotation", ref rot, 1, -360, 360))
					_transform.SetLocalRotationDegrees(rot);

				var scale = _transform.LocalScale.ToNumerics();
				if (ImGui.DragFloat2("Local Scale", ref scale, 0.05f))
					_transform.SetLocalScale(scale.ToXNA());

				scale = _transform.Scale.ToNumerics();
				if (ImGui.DragFloat2("Scale", ref scale, 0.05f))
					_transform.SetScale(scale.ToXNA());
			}
		}
	}
}