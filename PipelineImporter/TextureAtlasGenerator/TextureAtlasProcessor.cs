using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.ComponentModel;
using Microsoft.Xna.Framework;


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

		bool _compress;
		[Description( "Enable/disable texture compression" )]
		[DefaultValue( false )]
		public bool CompressTexture
		{
			get { return _compress; }
			set { _compress = value; }
		}


		float _animationFPS = 10f;
		[Description( "FPS value used when creating SpriteAnimations" )]
		[DefaultValue( 10f )]
		public float AnimationFPS
		{
			get { return _animationFPS; }
			set { _animationFPS = value; }
		}


		/// <summary>
		/// Converts an array of sprite filenames into a texture atlas object.
		/// </summary>
		public override TextureAtlasContent Process( string[] input, ContentProcessorContext context )
		{
			logger = context.Logger;
			var textureAtlas = new TextureAtlasContent
			{
				animationFPS = (int)AnimationFPS
			};
			var sourceSprites = new List<BitmapContent>();
			var imagePaths = new List<string>();

			// first, we need to sort through and figure out which passed in paths are images and which are folders
			foreach( var inputPath in input )
			{
				// first, the easy one. if it isnt a directory its an image so just add it
				if( !Directory.Exists( inputPath ) )
				{
					imagePaths.Add( inputPath );
					continue;
				}

				// we have a directory. we need to recursively add all images in all subfolders
				processDirectory( inputPath, imagePaths, textureAtlas );
			}

			// Loop over each input sprite filename.
			foreach( var inputFilename in imagePaths )
			{
				// Store the name of this sprite.
				var spriteName = Path.GetFileNameWithoutExtension( inputFilename );
				textureAtlas.spriteNames.Add( spriteName, sourceSprites.Count );
				context.Logger.LogMessage( "Adding texture: {0}", spriteName );

				// Load the sprite texture into memory.
				var textureReference = new ExternalReference<TextureContent>( inputFilename );
				var texture = context.BuildAndLoadAsset<TextureContent,TextureContent>( textureReference, "TextureProcessor" );

				sourceSprites.Add( texture.Faces[0][0] );
			}

			// Pack all the sprites into a single large texture.
			var packedSprites = TextureAtlasPacker.PackSprites( sourceSprites, textureAtlas.spriteRectangles, _compress, context );

			textureAtlas.texture.Mipmaps.Add( packedSprites );
			
			if( _compress )
				textureAtlas.texture.ConvertBitmapType( typeof( Dxt5BitmapContent ) );

			return textureAtlas;
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
				imagePaths.Add( file );
				didFindImages = true;
			}
			var animationEndIndex = imagePaths.Count - 1;

			if( didFindImages )
			{
				logger.LogMessage( "----- adding animation: {0}, frames: [{1} - {2}]", Path.GetFileName( directory ), animationStartIndex, animationEndIndex );
				textureAtlas.spriteAnimationDetails.Add( Path.GetFileName( directory ), new Point( animationStartIndex, animationEndIndex ) );
			}
		}

	}
}