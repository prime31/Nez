using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerFloatValue
	{
		[XmlAttribute("value")] public float Value;


		public static implicit operator float(ParticleDesignerFloatValue obj)
		{
			return obj.Value;
		}

        public override string ToString() => Value.ToString();
    }
}