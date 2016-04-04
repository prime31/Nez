using System;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	[XmlRoot( ElementName = "frame" )]
	public class TmxTilesetTileAnimationFrame
	{
		[XmlAttribute( AttributeName = "tileid" )]
		public int tileId;

		[XmlAttribute( AttributeName = "duration" )]
		public float duration;


		public override string ToString()
		{
			return string.Format( "[TmxTilesetTileAnimationFrame] tileId: {0}, duration: {1}", tileId, duration );
		}
	}
}

