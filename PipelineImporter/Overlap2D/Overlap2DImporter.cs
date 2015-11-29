using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Nez.Overlap2D.Runtime;


namespace Nez.Overlap2D
{
	[ContentImporter( ".dt", DefaultProcessor = "Overlap2DProcessor", DisplayName = "Overlap2D Importer" )]
	public class Overlap2DImporter : ContentImporter<SceneVO>
	{
		public override SceneVO Import( string filename, ContentImporterContext context )
		{
			if( filename == null )
				throw new ArgumentNullException( "filename" );

			using( var reader = new StreamReader( filename ) )
			{
				context.Logger.LogMessage( "Deserializing filename: {0}", filename );

				var scene = JsonConvert.DeserializeObject<SceneVO>( reader.ReadToEnd() );

/*				var serializer = new JsonSerializer();
				JObject all = JObject.Parse(reader.ReadToEnd());
				var scene = new SceneVO();
				scene.sceneName = all["sceneName"].ToString();
				context.Logger.LogMessage( "Deserializing sceneName: {0}", scene.sceneName );
				scene.composite = new CompositeVO();
				context.Logger.LogMessage( "Deserializing sImages" );
				var sImages = all["composite"]["sImages"].Children().ToList();
				foreach( JToken result in sImages )
				{
					context.Logger.LogMessage( "Deserializing sImage: {0}", result.ToString() );
					SimpleImageVO si = JsonConvert.DeserializeObject<SimpleImageVO>( result.ToString() );
					scene.composite.sImages.Add( si );
				}*/

				//var scene = (SceneVO)serializer.Deserialize( reader, typeof( SceneVO ) );

				return scene;
			}
		}
	}
}

