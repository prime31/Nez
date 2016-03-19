using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.UI;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Nez.UISkinImporter
{
	[ContentImporter( ".uiskin", DefaultProcessor = "UISkinProcessor", DisplayName = "UISkin Importer" )]
	public class UISkinImporter : ContentImporter<Dictionary<string,object>>
	{
		public static ContentBuildLogger logger;


		public override Dictionary<string,object> Import( string filename, ContentImporterContext context )
		{
			logger = context.Logger;
			logger.LogMessage( "Importing uiskin file: {0}", filename );

			using( var reader = new StreamReader( filename ) )
			{
				return JsonConvert.DeserializeObject<Dictionary<string,object>>( reader.ReadToEnd() );
			}
		}
	}
}