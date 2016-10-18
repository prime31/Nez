using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public class SvgPolygon : SvgElement
	{
		[XmlAttribute( "cx" )]
		public float centerX;

		[XmlAttribute( "cy" )]
		public float centerY;

		[XmlAttribute( "sides" )]
		public int sides;

		[XmlAttribute( "points" )]
		public string pointsAttribute
		{
			get { return null; }
			set { parsePoints( value ); }
		}

		public Vector2[] points;


		void parsePoints( string str )
		{
			var pairs = str.Split( new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries );
			points = new Vector2[pairs.Length];

			for( var i = 0; i < pairs.Length; i++ )
			{
				var parts = pairs[i].Split( new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries );
				points[i] = new Vector2( float.Parse( parts[0] ), float.Parse( parts[1] ) );
			}
		}


		public Vector2[] getTransformedPoints()
		{
			var pts = new Vector2[points.Length];
			var mat = getCombinedMatrix();
			Vector2Ext.transform( points, ref mat, pts );

			return pts;
		}

	}
}
