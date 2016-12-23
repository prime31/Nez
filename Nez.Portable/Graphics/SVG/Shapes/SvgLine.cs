using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public class SvgLine : SvgElement
	{
		[XmlAttribute( "x1" )]
		public float x1;

		[XmlAttribute( "y1" )]
		public float y1;

		[XmlAttribute( "x2" )]
		public float x2;

		[XmlAttribute( "y2" )]
		public float y2;

		public Vector2 start { get { return new Vector2( x1, y1 ); } }
		public Vector2 end { get { return new Vector2( x2, y2 ); } }


		public Vector2[] getTransformedPoints()
		{
			var pts = new Vector2[] { start, end };
			var mat = getCombinedMatrix();
			Vector2Ext.transform( pts, ref mat, pts );

			return pts;
		}

	}
}
