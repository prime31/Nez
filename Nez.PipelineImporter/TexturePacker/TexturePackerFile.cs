using System.Collections.Generic;
using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerFile
	{
		[JsonProperty("frames")] public List<TexturePackerRegion> Regions;

		[JsonProperty("meta")] public TexturePackerMeta Metadata;

		/// <summary>
		/// stores a map of the name of the sprite animation (derived from texturepacker filename metadata) to an array. 
		/// Each entry in the list refers to index of the corresponding subtexture
		/// </summary>
		public Dictionary<string, List<int>> SpriteAnimationDetails;
	}
}