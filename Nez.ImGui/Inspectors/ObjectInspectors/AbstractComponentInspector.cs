using System.Collections.Generic;
using Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.ObjectInspectors
{
	public abstract class AbstractComponentInspector : IComponentInspector
	{
		public abstract Entity entity { get; }
		public abstract Component component { get; }
		
		protected List<AbstractTypeInspector> _inspectors;
		protected int _scopeId = NezImGui.GetScopeId();

		public abstract void draw();
	}
}
