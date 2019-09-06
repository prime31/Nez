using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerColor
	{
		[XmlAttribute("red")] public float Red;

		[XmlAttribute("green")] public float Green;

		[XmlAttribute("blue")] public float Blue;

		[XmlAttribute("alpha")] public float Alpha;


		public static implicit operator Color(ParticleDesignerColor obj)
		{
			return new Color(obj.Red, obj.Green, obj.Blue, obj.Alpha);
		}

        public override string ToString() => $"{Red}, {Green}, {Blue}, {Alpha}";
    }
}