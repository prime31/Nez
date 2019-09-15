using System.Collections.Generic;

namespace Nez.Tiled
{
	public interface ITmxLayer : ITmxElement
	{
		float OffsetX { get; }
		float OffsetY { get; }
		float Opacity { get; }
		bool Visible { get; }
		Dictionary<string, string> Properties { get; }
	}
}