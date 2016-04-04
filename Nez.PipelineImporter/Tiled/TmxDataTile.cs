using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxDataTile
	{
		public TmxDataTile()
		{
			
		}
		
		public TmxDataTile( uint gid )
		{
			this.gid = gid;
		}


		[XmlAttribute( AttributeName = "gid" )]
		public uint gid;
		public bool flippedHorizontally;
		public bool flippedVertically;
		public bool flippedDiagonally;


		public override string ToString()
		{
			return gid.ToString();
		}
	}
}
