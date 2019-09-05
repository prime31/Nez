using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerIntValue
	{
		[XmlElement("value")] public int Value;


		public static implicit operator int(ParticleDesignerIntValue obj)
		{
			return obj.Value;
		}
	}
}