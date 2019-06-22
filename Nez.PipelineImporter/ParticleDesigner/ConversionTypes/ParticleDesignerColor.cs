using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerColor
	{
		[XmlAttribute]
		public float Red;

		[XmlAttribute]
		public float Green;

		[XmlAttribute]
		public float Blue;

		[XmlAttribute]
		public float Alpha;


		public static implicit operator Color( ParticleDesignerColor obj )
		{
			return new Color( obj.Red, obj.Green, obj.Blue, obj.Alpha );
		}

	}
}

