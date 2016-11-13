using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public class SvgPolyline : SvgElement
	{
		[XmlAttribute( "points" )]
		public string pointsAttribute
		{
			get { return null; }
			set { parsePoints( value ); }
		}

		public Vector2[] points;


		void parsePoints( string str )
		{
			// normalize commas and spaces since some programs use comma separate points and others use spaces
			str = str.Replace( ',', ' ' );
			var pairs = str.Split( new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries );
			points = new Vector2[pairs.Length / 2];

			var pointIndex = 0;
			for( var i = 0; i < pairs.Length; i += 2 )
				points[pointIndex++] = new Vector2( float.Parse( pairs[i] ), float.Parse( pairs[i + 1] ) );
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
