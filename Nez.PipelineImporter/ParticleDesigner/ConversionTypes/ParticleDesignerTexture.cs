using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerTexture
	{
		[XmlElement("name")] public string Name;

		[XmlElement("data")] public string Data;
	}
}