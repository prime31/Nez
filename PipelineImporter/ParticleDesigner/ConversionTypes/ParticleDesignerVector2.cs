using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerVector2
	{
		[XmlAttribute]
		public float x;

		[XmlAttribute]
		public float y;


		public static implicit operator Vector2( ParticleDesignerVector2 pdvec )
		{
			return new Vector2( pdvec.x, pdvec.y );
		}

	}
}

