using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Content;
using Nez.Textures;


namespace Nez.BitmapFonts
{
	public class BitmapFontReader : ContentTypeReader<BitmapFont>
	{
		protected override BitmapFont Read( ContentReader reader, BitmapFont existingInstance )
		{
			var textureAssetCount = reader.ReadInt32();
			var assets = new List<string>();

			for( var i = 0; i < textureAssetCount; i++ )
			{
				var assetName = reader.ReadString();
				assets.Add( assetName );
			}

			var textures = assets
                .Select( textureName => reader.ContentManager.Load<Texture2D>( reader.getRelativeAssetPath( textureName ) ) )
                .ToArray();

			var lineHeight = reader.ReadInt32();
			var regionCount = reader.ReadInt32();
			var regions = new BitmapFontRegion[regionCount];

			for( var r = 0; r < regionCount; r++ )
			{
				var character = (char)reader.ReadInt32();
				var textureIndex = reader.ReadInt32();
				var x = reader.ReadInt32();
				var y = reader.ReadInt32();
				var width = reader.ReadInt32();
				var height = reader.ReadInt32();
				var xOffset = reader.ReadInt32();
				var yOffset = reader.ReadInt32();
				var xAdvance = reader.ReadInt32();
				var textureRegion = new Subtexture( textures[textureIndex], x, y, width, height );
				regions[r] = new BitmapFontRegion( textureRegion, character, xOffset, yOffset, xAdvance );
			}
            
			return new BitmapFont( regions, lineHeight );
		}
	}
}