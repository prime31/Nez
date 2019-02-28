using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.IEnumerableExtensions;


namespace Nez.ImGuiTools.TypeInspectors
{
	public class MaterialInspector : AbstractTypeInspector
	{
		List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

		public override void initialize()
		{
			base.initialize();

			var material = getValue<Material>();
			if( material == null )
				return;

			_name = material.GetType().Name;

			// fetch our inspectors and let them know who their parent is
			_inspectors = TypeInspectorUtils.getInspectableProperties( material );
		}

		public override void drawMutable()
		{
			ImGui.Indent();
			var isOpen = ImGui.CollapsingHeader( $"{_name}", ImGuiTreeNodeFlags.FramePadding );
			if( ImGui.BeginPopupContextItem() )
			{
				if( ImGui.Selectable( "Remove Material" ) )
				{
					setValue( null );
					_isTargetDestroyed = true;
				}

				ImGui.EndPopup();
			}

			if( isOpen && !_isTargetDestroyed )
			{
				ImGui.Indent();
				for( var i = _inspectors.Count - 1; i >= 0; i-- )
				{
					if( _inspectors[i].isTargetDestroyed )
					{
						_inspectors.RemoveAt( i );
						continue;
					}
					_inspectors[i].draw();
				}
				ImGui.Unindent();
			}
			ImGui.Unindent();
		}
	}
}
