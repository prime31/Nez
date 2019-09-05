using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxImageLayer : TmxLayer
	{
		[XmlElement(ElementName = "image")] public TmxImage Image;


		public override string ToString()
		{
			return string.Format("[TmxImageLayer] name: {0}, image: {1}, visible: {2}", Name, Image, Visible);
		}
	}
}