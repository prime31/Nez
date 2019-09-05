using System;


namespace Nez.Tiled
{
	public class TiledTileAnimationFrame
	{
		/// <summary>
		/// tileId for this frame of the animation
		/// </summary>
		public readonly int TileId;

		/// <summary>
		/// duration in seconds for this frame of the animation
		/// </summary>
		public float Duration;


		public TiledTileAnimationFrame(int tileId, float duration)
		{
			this.TileId = tileId;
			this.Duration = duration / 1000;
		}
	}
}