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
			var format = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
			var pairs = str.Split( new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries );
			points = new Vector2[pairs.Length];

			for( var i = 0; i < pairs.Length; i++ )
			{
				var parts = pairs[i].Split( new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries );
				points[i] = new Vector2( float.Parse( parts[0], format ), float.Parse( parts[1], format ) );
			}
		}


		public Vector2[] getTransformedPoints()
		{
			var pts = new Vector2[points.Length];
			var mat = getCombinedMatrix();
			Vector2Ext.transform( points, ref mat, pts );

			return pts;
		}


		/// <summary>
		/// gets the points relative to the center. SVG by default uses absolute positions for points.
		/// </summary>
		/// <returns>The relative points.</returns>
		public Vector2[] getRelativePoints()
		{
			var pts = new Vector2[points.Length];

			var center = new Vector2( centerX, centerY );
			for( var i = 0; i < points.Length; i++ )
				pts[i] = points[i] - center;

			return pts;
		}

	}
}
