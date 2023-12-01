using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteTilemapCel : AsepriteCel
	{
		public readonly int Width;
		public readonly int Height;
		public readonly int BitsPerTile;
		public readonly uint TileIDBitmask;
		public readonly uint XFlipBitmask;
		public readonly uint YFlipBitmask;
		public readonly uint RotationBitmask;
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