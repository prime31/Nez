using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nez.Particles;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerProcessorResult
	{
		public ParticleDesignerEmitterConfig ParticleEmitterConfig;
		public Texture2DContent Texture;
		public byte[] TextureTiffData;


		public ParticleDesignerProcessorResult()
		{
		}
	}
}