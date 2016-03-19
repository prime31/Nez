using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zlib;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;


namespace Nez.TiledMaps
{
	[ContentProcessor( DisplayName = "Tiled Map Processor" )]
	public class TiledMapProcessor : ContentProcessor<TmxMap,TiledMapProcessorResult>
	{
		public static ContentBuildLogger logger;


		public override TiledMapProcessorResult Process( TmxMap map, ContentProcessorContext context )
		{
			logger = context.Logger;
			foreach( var layer in map.layers.OfType<TmxTileLayer>() )
			{
				var data = layer.data;

				if( data.encoding == "csv" )
				{
					data.tiles = data.value
                        .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                        .Select( uint.Parse )
                        .Select( gid => new TmxDataTile( (uint)gid ) )
                        .ToList();
				}
				else if( data.encoding == "base64" )
				{
					var encodedData = data.value.Trim();
					var decodedData = Convert.FromBase64String( encodedData );

					using( var stream = OpenStream( decodedData, data.compression ) )
						using( var reader = new BinaryReader( stream ) )
						{
							data.tiles = new List<TmxDataTile>();

							for( var y = 0; y < layer.width; y++ )
							{
								for( var x = 0; x < layer.height; x++ )
								{
									var gid = reader.ReadUInt32();
									data.tiles.Add( new TmxDataTile( gid ) );
								}
							}
						}
				}
			}

			return new TiledMapProcessorResult( map );
		}


		private static Stream OpenStream( byte[] decodedData, string compressionMode )
		{
			var memoryStream = new MemoryStream( decodedData, writable: false );
            
			if( compressionMode == "gzip" )
				return new GZipStream( memoryStream, CompressionMode.Decompress );

			if( compressionMode == "zlib" )
				return new ZlibStream( memoryStream, CompressionMode.Decompress );

			return memoryStream;
		}
	
	}
}