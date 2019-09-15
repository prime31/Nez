using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public class SvgLine : SvgElement
	{
		[XmlAttribute("x1")] public float X1;

		[XmlAttribute("y1")] public float Y1;

		[XmlAttribute("x2")] public float X2;

		[XmlAttribute("y2")] public float Y2;

		public Vector2 Start => new Vector2(X1, Y1);

		public Vector2 End => new Vector2(X2, Y2);


		public Vector2[] GetTransformedPoints()
		{
			var pts = new Vector2[] {Start, End};
			var mat = GetCombinedMatrix();
			Vector2Ext.Transform(pts, ref mat, pts);

			return pts;
		}
	}
}