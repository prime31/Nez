using System.Collections.Generic;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ComponentInspectors
{
	public abstract class AbstractComponentInspector : IComponentInspector
	{
		protected List<AbstractTypeInspector> _inspectors;
		protected int _scopeId = NezImGui.getScopeId();

		public abstract void draw();
	}
}
