using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml.Serialization;


namespace Nez.BitmapFontImporter
{
	[ContentImporter( ".fnt", DefaultProcessor = "BitmapFontProcessor", DisplayName = "BMFont Importer" )]
	public class BitmapFontImporter : ContentImporter<BitmapFontFile>
	{
		public override BitmapFontFile Import( string filename, ContentImporterContext context )
		{
			context.Logger.LogMessage( "Importing XML file: {0}", filename );

			using( var streamReader = new StreamReader( filename ) )
			{
				var deserializer = new XmlSerializer( typeof( BitmapFontFile ) );
				var bmFontFile = (BitmapFontFile)deserializer.Deserialize( streamReader );
				bmFontFile.file = filename;

				return bmFontFile;
			}
		}
	}
}