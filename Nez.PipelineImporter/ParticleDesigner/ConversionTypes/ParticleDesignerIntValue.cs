using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerIntValue
	{
		[XmlAttribute]
		public int Value;


		public static implicit operator int( ParticleDesignerIntValue obj )
		{
			return obj.Value;
		}

	}
}

