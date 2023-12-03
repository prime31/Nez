using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a tileset used by tilemap cels.
	/// </summary>
	public sealed class AsepriteTileset
	{
		/// <summary>
		/// The ID of this tileset assigned in Aseprite.
		/// </summary>
		public readonly int ID;

		/// <summary>
		/// The total number of tiles in this tileset.
		/// </summary>
		public readonly int TileCount;

		/// <summary>
		/// The width, in tiles, of this tileset.
		/// </summary>
		public readonly int TileWidth;

		/// <summary>
		/// The height, in tiles, of this tileset.
		/// </summary>
		public readonly int TileHeight;

		/// <summary>
		/// The name assigned to this tileset in Aseprite.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// An array of color elements that represent the pixel data for image of this tileset.  Order of elements is
		/// from top-left pixel read left-to-right top-to-bottom.
		/// </summary>
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