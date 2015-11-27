#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetProcessor.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System.ComponentModel;


#endregion

#region Using Statements
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


#endregion

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
		[DefaultValue( false )]
		public bool Compress
		{
			get { return _compress; }
			set { _compress = value; }
		}


		/// <summary>
		/// Converts an array of sprite filenames into a sprite sheet object.
		/// </summary>
		public override TextureAtlasContent Process( string[] input, ContentProcessorContext context )
		{
			logger = context.Logger;
			var spriteSheet = new TextureAtlasContent();
			var sourceSprites = new List<BitmapContent>();

			// Loop over each input sprite filename.
			foreach( string inputFilename in input )
			{
				// Store the name of this sprite.
				var spriteName = Path.GetFileNameWithoutExtension( inputFilename );
				spriteSheet.spriteNames.Add( spriteName, sourceSprites.Count );
				context.Logger.LogMessage( "Adding texture: {0}", spriteName );

				// Load the sprite texture into memory.
				var textureReference = new ExternalReference<TextureContent>( inputFilename );
				var texture = context.BuildAndLoadAsset<TextureContent,TextureContent>( textureReference, "TextureProcessor" );

				sourceSprites.Add( texture.Faces[0][0] );
			}

			// Pack all the sprites into a single large texture.
			var packedSprites = TextureAtlasPacker.PackSprites( sourceSprites, spriteSheet.spriteRectangles, _compress, context );

			spriteSheet.texture.Mipmaps.Add( packedSprites );
			
			if( _compress )
				spriteSheet.texture.ConvertBitmapType( typeof( Dxt5BitmapContent ) );

			return spriteSheet;
		}
	}
}