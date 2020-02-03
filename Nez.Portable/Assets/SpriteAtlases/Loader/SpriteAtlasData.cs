using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Sprites
{
	/// <summary>
	/// temporary class used when loading a SpriteAtlas and by the sprite atlas editor
	/// </summary>
	internal class SpriteAtlasData
	{
		public List<string> Names = new List<string>();
		public List<Rectangle> SourceRects = new List<Rectangle>();
		public List<Vector2> Origins = new List<Vector2>();

		public List<string> AnimationNames = new List<string>();
		public List<int> AnimationFps = new List<int>();
		public List<List<int>> AnimationFrames = new List<List<int>>();

		public SpriteAtlas AsSpriteAtlas(Texture2D texture)
		{
			var atlas = new SpriteAtlas();
			atlas.Names = Names.ToArray();
			atlas.Sprites = new Sprite[atlas.Names.Length];

			for (var i = 0; i < atlas.Sprites.Length; i++)
				atlas.Sprites[i] = new Sprite(texture, SourceRects[i], Origins[i]);

			atlas.AnimationNames = AnimationNames.ToArray();
			atlas.SpriteAnimations = new SpriteAnimation[atlas.AnimationNames.Length];
			for (var i = 0; i < atlas.SpriteAnimations.Length; i++)
			{
				var sprites = new Sprite[AnimationFrames[i].Count];
				for (var j = 0; j < sprites.Length; j++)
					sprites[j] = atlas.Sprites[AnimationFrames[i][j]];
				atlas.SpriteAnimations[i] = new SpriteAnimation(sprites, (float)AnimationFps[i]);
			}

			return atlas;
		}

		public void Clear()
		{
			Names.Clear();
			SourceRects.Clear();
			Origins.Clear();

			AnimationNames.Clear();
			AnimationFps.Clear();
			AnimationFrames.Clear();
		}

		public void SaveToFile(string filename)
		{
			if (File.Exists(filename))
				File.Delete(filename);

			using (var writer = new StreamWriter(filename))
			{
				for (var i = 0; i < Names.Count; i++)
				{
					writer.WriteLine(Names[i]);

					var rect = SourceRects[i];
					writer.WriteLine("\t{0},{1},{2},{3}", rect.X, rect.Y, rect.Width, rect.Height);
					writer.WriteLine("\t{0},{1}", Origins[i].X, Origins[i].Y);
				}

				if (AnimationNames.Count > 0)
				{
					writer.WriteLine();

					for (var i = 0; i < AnimationNames.Count; i++)
					{
						writer.WriteLine(AnimationNames[i]);
						writer.WriteLine("\t{0}", AnimationFps[i]);
						writer.WriteLine("\t{0}", string.Join(",", AnimationFrames[i]));
					}
				}
			}
		}
	}

}
