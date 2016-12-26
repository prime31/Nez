using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.ParticleDesignerImporter
{
	public class ParticleDesignerContent
	{
		public ContentImporterContext context;
		public ParticleDesignerEmitterConfig emitterConfig;
        public string path; //Required so that textures can be loaded from the same directory

		public ParticleDesignerContent( ContentImporterContext context, ParticleDesignerEmitterConfig emitterConfig, string path )
		{
			this.context = context;
			this.emitterConfig = emitterConfig;
            this.path = path;
		}
	}
}

