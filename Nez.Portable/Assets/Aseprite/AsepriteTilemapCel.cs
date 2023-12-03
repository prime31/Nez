using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a single cel in a frame that contains tilemap data.
	/// </summary>
	public sealed class AsepriteTilemapCel : AsepriteCel
	{
		/// <summary>
		/// The width, in tiles, of this cel.
		/// </summary>
		public readonly int Width;

		/// <summary>
		/// The height, in tiles, of this cel.
		/// </summary>
		public readonly int Height;

		/// <summary>
		/// The total number of bits per tile for each tile in this cel.
		/// </summary>
		public readonly int BitsPerTile;

		/// <summary>
		/// The bitmask used to determine the ID of tiles for this cel.
		/// </summary>
		public readonly uint TileIDBitmask;

		/// <summary>
		/// The bitmask used to determine the x-flip property of the tiles in this cel.
		/// </summary>
		public readonly uint XFlipBitmask;

		/// <summary>
		/// The bitmask used to determine the y-flip property of the tiles in this cel.
		/// </summary>
		public readonly uint YFlipBitmask;

		/// <summary>
		/// The bitmask used to determine the rotation property of the tiles in this cel.
		/// </summary>
		public readonly uint RotationBitmask;

		/// <summary>
		/// The collection of tiles that make up this cel.  Tile elements are in order of left-to-right, read
		/// top-to-bottom.
		/// </summary>
		public readonly AsepriteTile[] Tiles;

		internal AsepriteTilemapCel(int width, int height, int bitsPerTile, uint tileIDBitmask, uint xFlipBitmask, uint yFlipBitmask, uint rotationBitmask, AsepriteTile[] tiles, AsepriteLayer layer, Point position, int opacity)
			: base(layer, position, opacity)
		{
			Width = width;
			Height = height;
			BitsPerTile = bitsPerTile;
			TileIDBitmask = tileIDBitmask;
			XFlipBitmask = xFlipBitmask;
			YFlipBitmask = yFlipBitmask;
			RotationBitmask = rotationBitmask;
			Tiles = tiles;
		}

	}
}