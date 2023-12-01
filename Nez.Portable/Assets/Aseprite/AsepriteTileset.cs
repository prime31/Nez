using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteTileset
	{
		public readonly int ID;
		public readonly int TileCount;
		public readonly int TileWidth;
		public readonly int TileHeight;
		public readonly string Name;
		public readonly Color[] Pixels;

		internal AsepriteTileset(int id, int tileCount, int tileWidth, int tileHeight, string name, Color[] pixels)
		{
			ID = id;
			TileCount = tileCount;
			TileWidth = tileWidth;
			TileHeight = tileHeight;
			Name = name;
			Pixels = pixels;
		}
	}
}