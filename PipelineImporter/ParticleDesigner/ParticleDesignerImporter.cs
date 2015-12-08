using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	[ContentImporter( ".pex", DefaultProcessor = "ParticleDesignerImpoter", DisplayName = "Particle Designer Importer" )]
	public class ParticleDesignerImporter : ContentImporter<ParticleEmitterConfig>
	{
		public override ParticleEmitterConfig Import( string filename, ContentImporterContext context )
		{
			context.Logger.LogMessage( "Importing XML file: {0}", filename );

			using( var streamReader = new StreamReader( filename ) )
			{
				var deserializer = new XmlSerializer( typeof( ParticleEmitterConfig ) );
				return (ParticleEmitterConfig)deserializer.Deserialize( streamReader );
			}
		}
	}
}

