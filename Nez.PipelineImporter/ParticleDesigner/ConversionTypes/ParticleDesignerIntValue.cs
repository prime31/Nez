using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerIntValue
	{
		[XmlAttribute("value")] public int Value;


		public static implicit operator int(ParticleDesignerIntValue obj)
		{
			return obj.Value;
		}

        public override string ToString() => Value.ToString();
    }
}