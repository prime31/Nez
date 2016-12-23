using System;


namespace Nez.Tiled
{
	public class TiledTileAnimationFrame
	{
		/// <summary>
		/// tileId for this frame of the animation
		/// </summary>
		public readonly int tileId;

		/// <summary>
		/// duration in seconds for this frame of the animation
		/// </summary>
		public float duration;


		public TiledTileAnimationFrame( int tileId, float duration )
		{
			this.tileId = tileId;
			this.duration = duration / 1000;
		}
	}
}

