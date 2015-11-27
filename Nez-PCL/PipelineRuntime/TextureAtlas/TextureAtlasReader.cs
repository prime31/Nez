using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace Nez.TextureAtlases
{
	public class TextureAtlasReader : ContentTypeReader<TextureAtlas>
	{
		protected override TextureAtlas Read( ContentReader input, TextureAtlas existingInstance )
		{
			if( existingInstance != null )
			{
				// Read the texture into the existing texture instance
				input.ReadObject<Texture2D>( existingInstance.texture );

				// Discard the rest of the SpriteSheet data as we are only reloading GPU resources for now
				input.ReadObject<List<Rectangle>>();
				input.ReadObject<Dictionary<string,int>>();
				input.ReadObject<Dictionary<string,Point>>();
				input.ReadInt32();

				return existingInstance;
			}
			else
			{
				// Create a fresh TextureAtlas instance
				var texture = input.ReadObject<Texture2D>();
				var spriteRectangles = input.ReadObject<List<Rectangle>>();
				var spriteNames = input.ReadObject<Dictionary<string,int>>();
				var spriteAnimationDetails = input.ReadObject<Dictionary<string,Point>>();
				var animationFPS = input.ReadInt32();

				return new TextureAtlas( texture, spriteRectangles, spriteNames, spriteAnimationDetails, animationFPS );
			}
		}
	}
}
