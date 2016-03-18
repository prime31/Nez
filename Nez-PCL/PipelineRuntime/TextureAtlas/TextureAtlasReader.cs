using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nez.Textures;


namespace Nez.TextureAtlases
{
	public class TextureAtlasReader : ContentTypeReader<TextureAtlas>
	{
		protected override TextureAtlas Read( ContentReader input, TextureAtlas existingInstance )
		{
			if( existingInstance != null )
			{
				// read the texture
				var texture = input.ReadObject<Texture2D>();

				foreach( var subtexture in existingInstance.subtextures )
					subtexture.texture2D = texture;

				// discard the rest of the SpriteSheet data as we are only reloading GPU resources for now
				input.ReadObject<List<Rectangle>>();
				input.ReadObject<string[]>();
				input.ReadObject<Dictionary<string,Point>>();
				input.ReadObject<Dictionary<string,int[]>>();
				input.ReadInt32();

				return existingInstance;
			}
			else
			{
				// create a fresh TextureAtlas instance
				var texture = input.ReadObject<Texture2D>();
				var spriteRectangles = input.ReadObject<List<Rectangle>>();
				var spriteNames = input.ReadObject<string[]>();
				var spriteAnimationDetails = input.ReadObject<Dictionary<string,Point>>();
				var splits = input.ReadObject < Dictionary<string,int[]>>();
				var animationFPS = input.ReadInt32();

				// create subtextures
				var subtextures = new Subtexture[spriteNames.Length];
				for( var i = 0; i < spriteNames.Length; i++ )
				{
					// check to see if this is a nine patch
					if( splits.ContainsKey( spriteNames[i] ) )
					{
						var split = splits[spriteNames[i]];
						subtextures[i] = new NinePatchSubtexture( texture, split[0], split[1], split[2], split[3] );
					}
					else
					{
						subtextures[i] = new Subtexture( texture, spriteRectangles[i] );
					}
				}
				
				return new TextureAtlas( spriteNames, subtextures, spriteAnimationDetails, animationFPS );
			}
		}
	}
}
