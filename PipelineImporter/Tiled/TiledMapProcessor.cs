using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zlib;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;


namespace Nez
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

				if( data.Encoding == "csv" )
				{
					data.Tiles = data.Value
                        .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                        .Select( int.Parse )
                        .Select( gid => new TmxDataTile( (uint)gid ) )
                        .ToList();
				}
				else if( data.Encoding == "base64" )
				{
					var encodedData = data.Value.Trim();
					var decodedData = Convert.FromBase64String( encodedData );

					using( var stream = OpenStream( decodedData, data.Compression ) )
						using( var reader = new BinaryReader( stream ) )
						{
							data.Tiles = new List<TmxDataTile>();

							for( var y = 0; y < layer.width; y++ )
							{
								for( var x = 0; x < layer.height; x++ )
								{
									var gid = reader.ReadUInt32();
									data.Tiles.Add( new TmxDataTile( gid ) );
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