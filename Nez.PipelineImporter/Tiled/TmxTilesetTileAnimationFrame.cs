using System;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	[XmlRoot(ElementName = "frame")]
	public class TmxTilesetTileAnimationFrame
	{
		[XmlAttribute(AttributeName = "tileid")]
		public int TileId;

		[XmlAttribute(AttributeName = "duration")]
		public float Duration;


		public override string ToString()
		{
			return string.Format("[TmxTilesetTileAnimationFrame] tileId: {0}, duration: {1}", TileId, Duration);
		}
	}
}