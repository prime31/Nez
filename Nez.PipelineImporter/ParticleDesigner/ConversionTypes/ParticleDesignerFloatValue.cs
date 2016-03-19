using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerFloatValue
	{
		[XmlAttribute]
		public float value;


		public static implicit operator float( ParticleDesignerFloatValue obj )
		{
			return obj.value;
		}

	}
}

