using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerVector2
	{
		[XmlAttribute]
		public float X;

		[XmlAttribute]
		public float Y;


		public static implicit operator Vector2( ParticleDesignerVector2 pdvec )
		{
			return new Vector2( pdvec.X, pdvec.Y );
		}

	}
}

