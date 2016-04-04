using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using System;


namespace Nez.TextureAtlasGenerator
{
	/// <summary>
	/// Custom content processor takes an array of individual sprite filenames (which
	/// will typically be imported from an XML file), reads them all into memory,
	/// arranges them onto a single larger texture, and returns the resulting sprite
	/// sheet object.
	/// </summary>
	[ContentProcessor( DisplayName = "Texture Atlas Generator Processor" )]
	public class TextureAtlasProcessor : ContentProcessor<string[],TextureAtlasContent>
	{
		public static ContentBuildLogger logger;

		[Description( "Enable/disable texture compression" )]
		[DefaultValue( false )]
		public bool compressTexture { get; set; } = false;

		[Description( "FPS value used when creating SpriteAnimations" )]
		[DefaultValue( 10f )]
		public float animationFPS { get; set; } = 10f;

		[Description( "If true, subdirectory prefixes are stripped from image file names. Image file names should be unique." )]
		[DefaultValue( true )]
		public bool flattenPaths { get; set; } = true;


		/// <summary>
		/// Converts an array of sprite filenames into a texture atlas object.
		/// </summary>
		public override TextureAtlasContent Process( string[] input, ContentProcessorContext context )
		{
			logger = context.Logger;
			var textureAtlas = new TextureAtlasContent
			{
				animationFPS = (int)animationFPS
			};
			var sourceSprites = new List<BitmapContent>();
			var imagePaths = new List<string>();

			// first, we need to sort through and figure out which passed in paths are images and which are folders
			foreach( var inputPath in input )
			{
				// first, the easy one. if it isnt a directory its an image so just add it
				if( !Directory.Exists( inputPath ) )
				{
					if( isValidImageFile( inputPath ) )
						imagePaths.Add( inputPath );
					continue;
				}

				// we have a directory. we need to recursively add all images in all subfolders
				processDirectory( inputPath, imagePaths, textureAtlas );
			}

			// Loop over each input sprite filename
			foreach( var inputFilename in imagePaths )
			{
				// Store the name of this sprite.
				var spriteName = getSpriteNameFromFilename( inputFilename, input );
				textureAtlas.spriteNames.Add( spriteName, sourceSprites.Count );
				context.Logger.LogMessage( "Adding texture: {0}", spriteName );

				// Load the sprite texture into memory.
				var textureReference = new ExternalReference<TextureContent>( inputFilename );
				var texture = context.BuildAndLoadAsset<TextureContent,TextureContent>( textureReference, "TextureProcessor" );

				if( inputFilename.Contains( ".9" ) )
				{
					logger.LogMessage( "\tprocessing nine patch texture" );
					textureAtlas.nineSliceSplits[spriteName] = processNinePatchTexture( texture );
				}
				
				sourceSprites.Add( texture.Faces[0][0] );
			}

			// Pack all the sprites into a single large texture.
			var packedSprites = TextureAtlasPacker.packSprites( sourceSprites, textureAtlas.spriteRectangles, compressTexture, context );
			textureAtlas.texture.Mipmaps.Add( packedSprites );
			
			if( compressTexture )
				textureAtlas.texture.ConvertBitmapType( typeof( Dxt5BitmapContent ) );

			return textureAtlas;
		}


		string getSpriteNameFromFilename( string filepath, string[] input )
		{
			try
			{
				if( flattenPaths )
					return getFileNameWithoutExtension( filepath );

				// if this was a directly specified image path in the XML return it directly
				if( input.contains( filepath ) )
					return Path.GetFileNameWithoutExtension( filepath );
				
				// return the folder-filename as our first option
				var name = Path.GetFileNameWithoutExtension( filepath );
				var folder = filepath.Remove( filepath.LastIndexOf( Path.DirectorySeparatorChar ) );
				folder = folder.Substring( folder.LastIndexOf( Path.DirectorySeparatorChar ) + 1 );

				return string.Format( "{0}-{1}", folder, name );
			}
			catch( Exception )
			{
				return getFileNameWithoutExtension( filepath );
			}
		}


		void processDirectory( string directory, List<string> imagePaths, TextureAtlasContent textureAtlas )
		{
			var allFolders = Directory.GetDirectories( directory, "*", SearchOption.TopDirectoryOnly );
			foreach( var folder in allFolders )
				processDirectory( folder, imagePaths, textureAtlas );

			// handle the files in this directory
			var didFindImages = false;
			var animationStartIndex = imagePaths.Count;
			var allFiles = Directory.GetFiles( directory, "*.*", SearchOption.TopDirectoryOnly );
			foreach( var file in allFiles )
			{
				if( isValidImageFile( file ) )
				{
					didFindImages = true;
					imagePaths.Add( file );
				}
			}
			var animationEndIndex = imagePaths.Count - 1;

			if( didFindImages )
			{
				logger.LogMessage( "----- adding animation: {0}, frames: [{1} - {2}]", Path.GetFileName( directory ), animationStartIndex, animationEndIndex );
				textureAtlas.spriteAnimationDetails.Add( Path.GetFileName( directory ), new Point( animationStartIndex, animationEndIndex ) );
			}
		}


		/// <summary>
		/// locates the black pixels from a nine patch image and sets the splits for this image
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="spriteName">Sprite name.</param>
		int[] processNinePatchTexture( TextureContent texture )
		{
			// left, right, top, bottom of nine patch splits
			var splits = new int[4];
			var bitmap = texture.Faces[0][0];
			var data = bitmap.GetPixelData();

			var padStart = -1;
			var padEnd = int.MinValue;
			for( var x = 0; x < bitmap.Width * 4; x += 4 )
			{
				// we only care about alpha so disregard r/g/b
				var alpha = data[x+3];
				if( alpha == 255 )
				{
					if( padStart == -1 )
						padStart = x / 4;
					else
						padEnd = Math.Max( padEnd, x / 4 );
				}
			}

			splits[0] = padStart;
			splits[1] = bitmap.Width - padEnd;


			padStart = -1;
			padEnd = int.MinValue;
			var rowStride = bitmap.Width * 4;
			for( var y = 0; y < bitmap.Height * 4; y += 4 )
			{
				var pixel = ( y / 4 ) * rowStride;

				// we only care about alpha so disregard r/g/b
				var alpha = data[pixel+3];
				if( alpha == 255 )
				{
					if( padStart == -1 )
						padStart = y / 4;
					else
						padEnd = Math.Max( padEnd, y / 4 );
				}
			}

			splits[2] = padStart;
			splits[3] = bitmap.Height - padEnd;

			logger.LogMessage( "\tnine patch details. l: {0}, r: {1}, t: {2}, b: {3}", splits[0], splits[1], splits[2], splits[3] );

			// copy the data to a new Bitmap excluding the outside 1 pixel border
			var output = new PixelBitmapContent<Color>( bitmap.Width - 2, bitmap.Height - 2 );
			BitmapContent.Copy( bitmap, new Rectangle( 1, 1, output.Width, output.Height ), output, new Rectangle( 0, 0, output.Width, output.Height ) );
			texture.Faces[0][0] = output;

			return splits;
		}


		bool isValidImageFile( string file )
		{
			var ext = Path.GetExtension( file );
			if( ext == ".DS_Store" )
				return false;

			return true;
		}


		string getFileNameWithoutExtension( string filepath )
		{
			// strip out our nine patch if we have one
			return Path.GetFileNameWithoutExtension( filepath.Replace( ".9", string.Empty ) );
		}

	}
}