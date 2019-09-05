using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerFloatValue
	{
		[XmlElement("value")] public float Value;


		public static implicit operator float(ParticleDesignerFloatValue obj)
		{
			return obj.Value;
		}
	}
}