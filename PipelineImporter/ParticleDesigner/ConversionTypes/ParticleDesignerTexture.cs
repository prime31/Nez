using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerTexture
	{
		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public string data;
	}
}

