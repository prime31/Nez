using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerContent
	{
		public ContentImporterContext Context;
		public ParticleDesignerEmitterConfig EmitterConfig;
		public string Path; //Required so that textures can be loaded from the same directory

		public ParticleDesignerContent(ContentImporterContext context, ParticleDesignerEmitterConfig emitterConfig,
		                               string path)
		{
			this.Context = context;
			this.EmitterConfig = emitterConfig;
			this.Path = path;
		}
	}
}