using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.UI;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Nez.UISkinImporter
{
	[ContentImporter( ".uiskin", DefaultProcessor = "UISkinProcessor", DisplayName = "UISkin Importer" )]
	public class UISkinImporter : ContentImporter<IDictionary<string,object>>
	{
		public static ContentBuildLogger logger;


		public override IDictionary<string,object> Import( string filename, ContentImporterContext context )
		{
			logger = context.Logger;
			logger.LogMessage( "Importing uiskin file: {0}", filename );

			using( var reader = new StreamReader( filename ) )
			{
				var ret = JsonConvert.DeserializeObject<IDictionary<string,object>>( reader.ReadToEnd(), new JsonConverter[] {new JsonDictionaryConverter()} );
                return ret;
			}
		}
	}
}