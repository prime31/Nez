using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxDataTile
	{
		public TmxDataTile()
		{
		}

		public TmxDataTile(uint gid)
		{
			this.Gid = gid;
		}


		[XmlAttribute(AttributeName = "gid")] public uint Gid;
		public bool FlippedHorizontally;
		public bool FlippedVertically;
		public bool FlippedDiagonally;


		public override string ToString()
		{
			return Gid.ToString();
		}
	}
}