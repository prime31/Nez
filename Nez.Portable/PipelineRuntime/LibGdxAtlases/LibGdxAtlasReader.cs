using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Pipeline.Content;
using Microsoft.Xna.Framework;
using Nez.TextureAtlases;
using Nez.Textures;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlasReader : ContentTypeReader<LibGdxAtlas>
	{
		protected override LibGdxAtlas Read(ContentReader reader, LibGdxAtlas existingInstance)
		{
			var atlasContainer = new LibGdxAtlas();
			var numPages = reader.ReadInt32();
			for (var p = 0; p < numPages; p++)
			{
				var assetName = reader.GetRelativeAssetPath(reader.ReadString());
				var texture = reader.ContentManager.Load<Texture2D>(assetName);

				var regionCount = reader.ReadInt32();
				var sprites = new Sprite[regionCount];
				var regionNames = new string[regionCount];

				for (var i = 0; i < regionCount; i++)
				{
					var rect = new Rectangle();
					var name = reader.ReadString();
					rect.X = reader.ReadInt32();
					rect.Y = reader.ReadInt32();
					rect.Width = reader.ReadInt32();
					rect.Height = reader.ReadInt32();

					var hasSplits = reader.ReadBoolean();
					if (hasSplits)
						sprites[i] = new NinePatchSprite(texture, rect, reader.ReadInt32(), reader.ReadInt32(),
							reader.ReadInt32(), reader.ReadInt32());
					else
						sprites[i] = new Sprite(texture, rect);

					var hasPads = reader.ReadBoolean();
					if (hasPads)
					{
						((NinePatchSprite) sprites[i]).HasPadding = true;
						((NinePatchSprite) sprites[i]).PadLeft = reader.ReadInt32();
						((NinePatchSprite) sprites[i]).PadRight = reader.ReadInt32();
						((NinePatchSprite) sprites[i]).PadTop = reader.ReadInt32();
						((NinePatchSprite) sprites[i]).PadBottom = reader.ReadInt32();
					}

					var index = reader.ReadInt32();

					// animation
					if (index != -1)
					{
						List<Sprite> frames;
						if (!atlasContainer.Animations.TryGetValue(name, out frames))
						{
							frames = new List<Sprite>();
							atlasContainer.Animations[name] = frames;
						}

						frames.Insert(index, sprites[i]);
					}

					regionNames[i] = name;
				}

				var atlas = new TextureAtlas(regionNames, sprites);
				atlasContainer.Atlases.Add(atlas);
			}

			return atlasContainer;
		}
	}
}