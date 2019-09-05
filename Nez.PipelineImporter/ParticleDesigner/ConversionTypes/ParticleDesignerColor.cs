using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerColor
	{
		[XmlElement("red")] public float Red;

		[XmlElement("green")] public float Green;

		[XmlElement("blue")] public float Blue;

		[XmlElement("alpha")] public float Alpha;


		public static implicit operator Color(ParticleDesignerColor obj)
		{
			return new Color(obj.Red, obj.Green, obj.Blue, obj.Alpha);
		}
	}
}