using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerContent
	{
		public ContentImporterContext context;
		public ParticleDesignerEmitterConfig emitterConfig;


		public ParticleDesignerContent( ContentImporterContext context, ParticleDesignerEmitterConfig emitterConfig )
		{
			this.context = context;
			this.emitterConfig = emitterConfig;
		}
	}
}

