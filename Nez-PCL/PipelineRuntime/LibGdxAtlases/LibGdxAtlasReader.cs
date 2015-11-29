using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Nez.TextureAtlases;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlasReader : ContentTypeReader<LibGdxAtlas>
	{
		protected override LibGdxAtlas Read( ContentReader reader, LibGdxAtlas existingInstance )
		{
			var atlasContainer = new LibGdxAtlas();
			var numPages = reader.ReadInt32();
			for( var p = 0; p < numPages; p++ )
			{
				var assetName = reader.getRelativeAssetPath( reader.ReadString() );
				var texture = reader.ContentManager.Load<Texture2D>( assetName );
				List<Rectangle> subtextures = new List<Rectangle>();
				Dictionary<string, int> map = new Dictionary<string, int>();
				var regionCount = reader.ReadInt32();
				for( var i = 0; i < regionCount; i++ )
				{
					Rectangle r = new Rectangle();
					var name = reader.ReadString();
					var x = reader.ReadInt32();
					var y = reader.ReadInt32();
					var width = reader.ReadInt32();
					var height = reader.ReadInt32();
					r.X = x;
					r.Y = y;
					r.Width = width;
					r.Height = height;
					subtextures.Add( r );
					map[name] = i;
				}
				var atlas = new TextureAtlas( texture, subtextures, map, new Dictionary<string, Point>(), 0 );
				atlasContainer.atlases.Add( atlas );
			}

			return atlasContainer;
		}

	}
}

