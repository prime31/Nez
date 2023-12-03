namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a single tile in a tilemap cel.
	/// </summary>
	public sealed class AsepriteTile
	{
		/// <summary>
		/// The ID of the tile in the tileset this tile represents.
		/// </summary>
		public readonly uint ID;

		/// <summary>
		/// A value that indicates if this tile is flipped along the x-axis.
		/// </summary>
		public readonly uint XFlip;

		/// <summary>
		/// A value that indicates if this tile is flipped along the y-axis.
		/// </summary>
		public readonly uint YFlip;

		/// <summary>
		/// A value that indicates the amount of 90deg clockwise rotation applied to this tile.
		/// </summary>
		public readonly uint Rotate90;

		internal AsepriteTile(uint id, uint xFlip, uint yFlip, uint rotate)
		{
			ID = id;
			XFlip = xFlip;
			YFlip = yFlip;
			Rotate90 = rotate;
		}
	}
}