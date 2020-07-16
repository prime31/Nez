using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public class SvgRectangle : SvgElement
	{
		[XmlAttribute("x")] public float X;

		[XmlAttribute("y")] public float Y;

		[XmlAttribute("width")] public float Width;

		[XmlAttribute("height")] public float Height;

		public Vector2 Center => new Vector2(X + Width / 2, Y + Height / 2);


		/// <summary>
		/// gets the points for the rectangle with all transforms applied
		/// </summary>
		/// <returns>The transformed points.</returns>
		public Vector2[] GetTransformedPoints()
		{
			var pts = new Vector2[]
			{
				new Vector2(X, Y), new Vector2(X + Width, Y), new Vector2(X + Width, Y + Height),
				new Vector2(X, Y + Height)
			};
			var mat = GetCombinedMatrix();
			Vector2Ext.Transform(pts, ref mat, pts);

			return pts;
		}
	}
}