using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	/// <summary>
	/// dummy interface used by SvgPath.getTransformedDrawingPoints to workaround PCL not having System.Drawing
	/// </summary>
	public interface ISvgPathBuilder
	{
		Vector2[] GetDrawingPoints(List<SvgPathSegment> segments, float flatness = 3);
	}
}