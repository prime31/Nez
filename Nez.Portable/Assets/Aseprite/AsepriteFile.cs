using System.Collections.Generic;

namespace Nez.Aseprite
{
	public sealed class AsepriteFile
	{
		public readonly int Width;
		public readonly int Height;
		public readonly AsepriteColorDepth ColorDepth;
		public readonly List<AsepriteFrame> Frames;
		public readonly List<AsepriteLayer> Layers;
		public readonly List<AsepriteTag> Tags;
		public readonly List<AsepriteSlice> Slices;
		public readonly List<string> Warnings;
		public readonly AsepritePalette Palette;
		public AsepriteUserData UserData { get; }

		internal AsepriteFile(AsepritePalette palette, int width, int height, AsepriteColorDepth colorDepth, List<AsepriteFrame> frames, List<AsepriteLayer> layers, List<AsepriteTag> tags, List<AsepriteSlice> slices, List<string> warnings)
		{
			Width = width;
			Height = height;
			ColorDepth = colorDepth;
			Frames = frames;
			Tags = tags;
			Slices = slices;
			Warnings = warnings;
			Palette = palette;
		}

	}
}