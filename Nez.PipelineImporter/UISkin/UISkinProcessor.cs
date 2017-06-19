using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;
using Nez.UI;
using System;

namespace Nez.UISkinImporter
{
	[ContentProcessor( DisplayName = "UISkin Processor" )]
	public class UISkinProcessor : ContentProcessor<Dictionary<string, object>, UISkinConfig>
	{
		public override UISkinConfig Process( Dictionary<string, object> input, ContentProcessorContext context )
		{
			var skinConfig = new UISkinConfig();
			var styleConfig = new UISkinStyleConfig();

			foreach( var key in input.Keys )
			{
				// special cases first
				if( key == "colors" )
				{
					skinConfig.colors = parseColors( input[key] as Dictionary<string, object> );
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
					styleConfig.Add( key, input[key] );
				}
			}

			if( styleConfig.Keys.Count > 0 )
				skinConfig.styles = styleConfig;

			return skinConfig;
		}


		Dictionary<string, Color> parseColors( Dictionary<string, object> colors )
		{
			if( colors.Count == 0 )
				return null;

			var result = new Dictionary<string, Color>( colors.Count );
			foreach( var key in colors.Keys )
			{
				var obj = colors[key];
				UISkinImporter.logger.LogMessage( "adding color: {0}", key );
				Color color;

				// Allow the passing of strings (hex), arrays and objects (r:, g:, b:) to
				//  represent colors. Also detect the usage of integers or normalized floats
				if( obj is string )
				{
					var val = obj as string;

					// we could have hex or hex
					if( val.StartsWith( "#" ) )
						color = ColorExt.hexToColor( val.Substring( 1 ) );
					else if( val.StartsWith( "0x" ) )
						color = ColorExt.hexToColor( val.Substring( 2 ) );
					else
					{
						UISkinImporter.logger.LogMessage( "unsupported color definition {0}: {1}", key, val );
						continue;
					}
				}
				else if( obj is JArray )
				{
					var jArr = obj as JArray;
					if( jArr.Count < 3 )
					{
						UISkinImporter.logger.LogMessage( "unsupported color definition {0}: color array requires at least 3 members", key );
						continue;
					}

					if( jArr[0].Type == JTokenType.Integer )
					{
						var arr = jArr.ToObject<int[]>();
						color = new Color( arr[0], arr[1], arr[2] );
						if( arr.Length == 4 )
							color = new Color( color, arr[3] );
					}
					else if( jArr[0].Type == JTokenType.Float )
					{
						var arr = jArr.ToObject<float[]>();
						color = new Color( arr[0], arr[1], arr[2] );
						if( arr.Length == 4 )
							color = new Color( color, arr[3] );
					}
					else
					{
						UISkinImporter.logger.LogMessage( "unsupported color definition {0}: unknown type", key );
						continue;
					}
				}
				else if( obj is Dictionary<string, object> )
				{
					var dict = obj as Dictionary<string, object>;
					if( dict.Count < 3 )
					{
						UISkinImporter.logger.LogMessage( "unsupported color definition {0}: color object requires at least 3 members", key );
						continue;
					}

					if( dict["r"] is long )
					{
						color = new Color( Convert.ToInt32( dict["r"] ), Convert.ToInt32( dict["g"] ), Convert.ToInt32( dict["b"] ) );
						if( dict.Count == 4 )
							color = new Color( color, Convert.ToInt32( dict["a"] ) );
					}
					else if( dict["r"] is double )
					{
						color = new Color( Convert.ToSingle( dict["r"] ), Convert.ToSingle( dict["g"] ), Convert.ToSingle( dict["b"] ) );
						if( dict.Count == 4 )
							color = new Color( color, Convert.ToSingle( dict["a"] ) );
					}
				}

				result.Add( key, color );
			}

			UISkinImporter.logger.LogMessage( "" );
			return result;
		}
	}
}

