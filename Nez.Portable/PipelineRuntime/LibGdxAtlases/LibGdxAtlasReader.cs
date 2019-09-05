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
				var subtextures = new Subtexture[regionCount];
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
						subtextures[i] = new NinePatchSubtexture(texture, rect, reader.ReadInt32(), reader.ReadInt32(),
							reader.ReadInt32(), reader.ReadInt32());
					else
						subtextures[i] = new Subtexture(texture, rect);

					var hasPads = reader.ReadBoolean();
					if (hasPads)
					{
						((NinePatchSubtexture) subtextures[i]).HasPadding = true;
						((NinePatchSubtexture) subtextures[i]).PadLeft = reader.ReadInt32();
						((NinePatchSubtexture) subtextures[i]).PadRight = reader.ReadInt32();
						((NinePatchSubtexture) subtextures[i]).PadTop = reader.ReadInt32();
						((NinePatchSubtexture) subtextures[i]).PadBottom = reader.ReadInt32();
					}

					var index = reader.ReadInt32();

					// animation
					if (index != -1)
					{
						List<Subtexture> frames;
						if (!atlasContainer.Animations.TryGetValue(name, out frames))
						{
							frames = new List<Subtexture>();
							atlasContainer.Animations[name] = frames;
						}

						frames.Insert(index, subtextures[i]);
					}

					regionNames[i] = name;
				}

				var atlas = new TextureAtlas(regionNames, subtextures);
				atlasContainer.Atlases.Add(atlas);
			}

			return atlasContainer;
		}
	}
}