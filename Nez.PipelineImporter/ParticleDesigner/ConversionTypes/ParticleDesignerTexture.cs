using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerTexture
	{
		[XmlAttribute("name")] public string Name;

		[XmlAttribute("data")] public string Data;
    }
}