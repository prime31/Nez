using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nez.Particles;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerProcessorResult
	{
		public ParticleDesignerEmitterConfig particleEmitterConfig;
		public Texture2DContent texture;
		public byte[] textureTiffData;


		public ParticleDesignerProcessorResult()
		{}
	}
}

