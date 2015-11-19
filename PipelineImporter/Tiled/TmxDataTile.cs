using System.Xml.Serialization;


namespace Nez
{
	public class TmxDataTile
	{
		public TmxDataTile()
		{
		}

		public TmxDataTile( uint gid )
		{
			Gid = gid;
		}

		public override string ToString()
		{
			return Gid.ToString();
		}

		[XmlAttribute( AttributeName = "gid" )]
		public uint Gid;
		public bool flippedHorizontally;
		public bool flippedVertically;
		public bool flippedDiagonally;
	}
}