using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	[ContentImporter( ".pex", DefaultProcessor = "ParticleDesignerProcessor", DisplayName = "Particle Designer Importer" )]
	public class ParticleDesignerImporter : ContentImporter<ParticleDesignerContent>
	{
		public override ParticleDesignerContent Import( string filename, ContentImporterContext context )
		{
			context.Logger.LogMessage( "\nImporting XML file: {0}", filename );

			using( var streamReader = new StreamReader( filename ) )
			{
				var deserializer = new XmlSerializer( typeof( ParticleDesignerEmitterConfig ) );
				var emitterConfig = (ParticleDesignerEmitterConfig)deserializer.Deserialize( streamReader );

				return new ParticleDesignerContent( context, emitterConfig );
			}
		}
	}
}

