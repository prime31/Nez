using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerTexture
	{
		[XmlAttribute]
		public string Name;

		[XmlAttribute]
		public string Data;
	}
}

