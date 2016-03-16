using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;
using Nez.UI;


namespace Nez.UISkinImporter
{
	[ContentProcessor( DisplayName = "UISkin Processor" )]
	public class UISkinProcessor : ContentProcessor<Dictionary<string,object>, UISkinConfig>
	{
		public override UISkinConfig Process( Dictionary<string,object> input, ContentProcessorContext context )
		{
			var skinConfig = new UISkinConfig();
			var styleConfig = new UISkinStyleConfig();

			foreach( var key in input.Keys )
			{
				// special cases first
				if( key == "colors" )
				{
					skinConfig.colors = parseColors( input[key] as JObject );
				}
				else if( key == "libGdxAtlases" )
				{
					var jArr = input[key] as JArray;
					skinConfig.libGdxAtlases = jArr.ToObject<string[]>();
					UISkinImporter.logger.LogMessage( "added {0} LibGdxAtlases\n", jArr.Count );
				}
				else if( key == "textureAtlases" )
				{
					var jArr = input[key] as JArray;
					skinConfig.textureAtlases = jArr.ToObject<string[]>();
					UISkinImporter.logger.LogMessage( "added {0} TextureAtlases\n", jArr.Count );
				}
				else
				{
					UISkinImporter.logger.LogMessage( "adding style type: {0}", key );
					styleConfig.Add( key, parseStyles( input[key] as JObject ) );
				}
			}

			if( styleConfig.Keys.Count > 0 )
				skinConfig.styles = styleConfig;

			return skinConfig;
		}


		Dictionary<string,Color> parseColors( JObject colors )
		{
			if( colors.Count == 0 )
				return null;

			var result = new Dictionary<string,Color>( colors.Count );
			foreach( JProperty key in colors.Children() )
			{
				var obj = colors[key.Name];
				UISkinImporter.logger.LogMessage( "adding color: {0}", key.Name );

				if( obj is JValue  )
				{
					var jVal = obj as JValue;
					var val = jVal.Value as string;

					// we could have hex or hex
					if( val.StartsWith( "#" ) )
						result.Add( key.Name, ColorExt.hexToColor( val.Substring( 1 ) ) );
					else if( val.StartsWith( "0x" ) )
						result.Add( key.Name, ColorExt.hexToColor( val.Substring( 2 ) ) );
					else
						UISkinImporter.logger.LogMessage( "unsupported color definition {0}: {1}", key.Name, val );
				}
				else if( obj is JArray )
				{
					var jArr = obj as JArray;
					var arr = jArr.ToObject<int[]>();
					result.Add( key.Name, new Color( arr[0], arr[1], arr[2], arr[3] ) );
				}
			}

			UISkinImporter.logger.LogMessage( "" );
			return result;
		}
	

		Dictionary<string,Dictionary<string,string>> parseStyles( JObject styles )
		{
			var res = new Dictionary<string,Dictionary<string,string>>();
			foreach( JProperty key in styles.Children() )
			{
				UISkinImporter.logger.LogMessage( "\tadding style: {0}", key.Name );
				res.Add( key.Name, parseStyle( styles[key.Name] as JObject ) );
			}

			UISkinImporter.logger.LogMessage( "" );
			return res;
		}


		Dictionary<string,string> parseStyle( JObject styles )
		{
			var res = new Dictionary<string,string>();
			foreach( JProperty key in styles.Children() )
				res.Add( key.Name, ( styles[key.Name] as JValue ).Value as string );

			return res;
		}

	}
}

