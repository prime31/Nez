using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zlib;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Nez.TextureAtlasGenerator;
using Microsoft.Xna.Framework.Graphics;
using Nez.PipelineImporter;


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

			// deal with tilesets that have image collections
			foreach( var tileset in map.tilesets )
				setTilesetTextureIfNecessary( tileset, context );

			return new TiledMapProcessorResult( map );
		}


		static void setTilesetTextureIfNecessary( TmxTileset tileset, ContentProcessorContext context )
		{
			if( tileset.image != null )
				return;

			tileset.isStandardTileset = false;

			var imagePaths = new List<string>();
			foreach( var tile in tileset.tiles )
			{
				if( tile.image != null && !imagePaths.Contains( tile.image.source ) )
					imagePaths.Add( tile.image.source );
			}

			context.Logger.LogMessage( "\n\t --- need to pack images: {0}\n", imagePaths.Count );
			var sourceSprites = new List<BitmapContent>();

			// Loop over each input sprite filename
			foreach( var inputFilename in imagePaths )
			{
				// Store the name of this sprite.
				var spriteName = Path.GetFileName( inputFilename );

				var absolutePath = PathHelper.getAbsolutePath( inputFilename, tileset.mapFolder );
				context.Logger.LogMessage( "Adding texture: {0}", spriteName );

				// Load the sprite texture into memory.
				var textureReference = new ExternalReference<TextureContent>( absolutePath );
				var texture = context.BuildAndLoadAsset<TextureContent,TextureContent>( textureReference, "TextureProcessor" );

				sourceSprites.Add( texture.Faces[0][0] );
			}

			var spriteRectangles = new List<Rectangle>();

			// pack all the sprites into a single large texture.
			var packedSprites = TextureAtlasPacker.packSprites( sourceSprites, spriteRectangles, false, context );
			context.Logger.LogMessage( "packed: {0}", packedSprites );

			// save out a PNG with our atlas
			var bm = new System.Drawing.Bitmap( packedSprites.Width, packedSprites.Height );
			for( var x = 0; x < packedSprites.Width; x++ )
			{
				for( var y = 0; y < packedSprites.Height; y++ )
				{
					var col = packedSprites.GetPixel( x, y );
					var color = System.Drawing.Color.FromArgb( col.A, col.R, col.G, col.B );
					bm.SetPixel( x, y, color );
				}
			}

			var atlasFilename = tileset.name + "-atlas.png";
			bm.Save( Path.Combine( tileset.mapFolder, atlasFilename ), System.Drawing.Imaging.ImageFormat.Png );
			context.Logger.LogImportantMessage( "\n-- generated atlas {0}. Make sure you add it to the Pipeline tool!", atlasFilename );

			// set the new atlas as our tileset source image
			tileset.image = new TmxImage();
			tileset.image.source = atlasFilename;

			// last step: set the new atlas info and source rectangle for each tile
			foreach( var tile in tileset.tiles )
			{
				if( tile.image == null )
					continue;

				tile.sourceRect = spriteRectangles[imagePaths.IndexOf( tile.image.source )];
			}
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