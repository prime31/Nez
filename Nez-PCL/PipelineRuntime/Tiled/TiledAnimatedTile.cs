using System;


namespace Nez.Tiled
{
	public class TiledAnimatedTile : TiledTile
	{
		public new TiledTilesetTile tilesetTile;

		float _elapsedTime;
		int _currentFrame;


		public TiledAnimatedTile( int id, TiledTilesetTile tilesetTile ) : base( id )
		{
			this.tilesetTile = tilesetTile;
		}


		public void update()
		{
			_elapsedTime += Time.deltaTime;

			if( _elapsedTime > tilesetTile.animationFrames[_currentFrame].duration )
			{
				_currentFrame = Mathf.incrementWithWrap( _currentFrame, tilesetTile.animationFrames.Count );
				// HACK: still not quite sure why we have to resolve the global tildId with the tileset.firstId here...
				id = tilesetTile.animationFrames[_currentFrame].tileId + tileset.firstId;
				_elapsedTime = 0;
			}
		}
	}
}

