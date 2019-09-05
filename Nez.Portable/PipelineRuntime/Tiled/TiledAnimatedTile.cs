using System;


namespace Nez.Tiled
{
	public class TiledAnimatedTile : TiledTile
	{
		public new TiledTilesetTile TilesetTile;

		float _elapsedTime;
		int _currentFrame;


		public TiledAnimatedTile(int id, TiledTilesetTile tilesetTile) : base(id)
		{
			this.TilesetTile = tilesetTile;
		}


		public void Update()
		{
			_elapsedTime += Time.DeltaTime;

			if (_elapsedTime > TilesetTile.AnimationFrames[_currentFrame].Duration)
			{
				_currentFrame = Mathf.IncrementWithWrap(_currentFrame, TilesetTile.AnimationFrames.Count);

				// HACK: still not quite sure why we have to resolve the global tildId with the tileset.firstId here...
				Id = TilesetTile.AnimationFrames[_currentFrame].TileId + Tileset.FirstId;
				_elapsedTime = 0;
			}
		}
	}
}