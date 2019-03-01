using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ObjectInspectors
{
    public class ObjectInspector : AbstractTypeInspector
    {
        List<AbstractTypeInspector> _inspectors;
        
        public override void initialize()
        {
            var obj = getValue();
            if( obj != null )
                _inspectors = TypeInspectorUtils.getInspectableProperties( obj );
            else
                _inspectors = new List<AbstractTypeInspector>();
        }

		public override void drawMutable()
		{
			if( ImGui.CollapsingHeader( _name ) )
            {
                foreach( var inspector in _inspectors )
                    inspector.draw();
            }
		}

	}
}