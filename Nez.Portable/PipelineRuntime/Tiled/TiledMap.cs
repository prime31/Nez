using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledMap
	{
		#region Fields and Properties

		public readonly int firstGid;
		public readonly int width;
		public readonly int height;
		public readonly int tileWidth;
		public readonly int tileHeight;
		public Color? backgroundColor;
		public TiledRenderOrder renderOrder;
		public readonly TiledMapOrientation orientation;
		public readonly Dictionary<string, string> properties = new Dictionary<string, string>();
		public readonly List<TiledLayer> layers = new List<TiledLayer>();
		public readonly List<TiledImageLayer> imageLayers = new List<TiledImageLayer>();
		public readonly List<TiledObjectGroup> objectGroups = new List<TiledObjectGroup>();
		public readonly List<TiledTileset> tilesets = new List<TiledTileset>();

		public int widthInPixels { get { return width * tileWidth; } }

		public int heightInPixels { get { return height * tileHeight; } }

		public int largestTileWidth;
		public int largestTileHeight;

		internal bool requiresLargeTileCulling;
		internal List<TiledAnimatedTile> _animatedTiles = new List<TiledAnimatedTile>();

		#endregion


		public TiledMap(
			int firstGid,
			int width,
			int height,
			int tileWidth,
			int tileHeight,
			TiledMapOrientation orientation = TiledMapOrientation.Orthogonal )
		{
			this.firstGid = firstGid;
			this.width = width;
			this.height = height;
			this.tileWidth = tileWidth;
			this.tileHeight = tileHeight;
			this.orientation = orientation;
		}


		#region Tileset and Layer creation

		public TiledTileset createTileset( Texture2D texture, int firstId, int tileWidth, int tileHeight, bool isStandardTileset, int spacing = 2, int margin = 2, int tileCount = 1, int columns = 1 )
		{
			TiledTileset tileset;
			if( isStandardTileset )
				tileset = new TiledTileset( texture, firstId, tileWidth, tileHeight, spacing, margin, tileCount, columns );
			else
				tileset = new TiledImageCollectionTileset( texture, firstId );

			if( tileset.tileWidth > largestTileWidth )
				largestTileWidth = tileset.tileWidth;

			if( tileset.tileHeight > largestTileHeight )
				largestTileHeight = tileset.tileHeight;

			tilesets.Add( tileset );

			return tileset;
		}


		public TiledLayer createTileLayer( string name, int width, int height )
		{
			if( orientation == TiledMapOrientation.Orthogonal )
			{
				var layer = new TiledTileLayer( this, name, width, height );
				layers.Add( layer );
				return layer;
			}

			if( orientation == TiledMapOrientation.Isometric )
			{
				var layer = new TiledIsometricTiledLayer( this, name, width, height );
				layers.Add( layer );
				return layer;
			}

			throw new NotImplementedException();
		}


		public TiledLayer createTileLayer( string name, int width, int height, TiledTile[] tiles )
		{
			if( orientation == TiledMapOrientation.Orthogonal )
			{
				var layer = new TiledTileLayer( this, name, width, height, tiles );
				layers.Add( layer );
				return layer;
			}

			if( orientation == TiledMapOrientation.Isometric )
			{
				var layer = new TiledIsometricTiledLayer( this, name, width, height, tiles );
				layers.Add( layer );
				return layer;
			}

			throw new NotImplementedException();
		}


		public TiledImageLayer createImageLayer( string name, Texture2D texture )
		{
			var layer = new TiledImageLayer( name, texture );
			layers.Add( layer );
			return layer;
		}


		public TiledObjectGroup createObjectGroup( string name, Color color, bool visible, float opacity )
		{
			var group = new TiledObjectGroup( name, color, visible, opacity );
			objectGroups.Add( group );
			return group;
		}

		#endregion


		#region Tileset and Layer getters

		/// <summary>
		/// gets the TiledTileset for the given tileId
		/// </summary>
		/// <returns>The tileset for tile identifier.</returns>
		/// <param name="tileId">Identifier.</param>
		public TiledTileset getTilesetForTileId( int tileId )
		{
			for( var i = tilesets.Count - 1; i >= 0; i-- )
			{
				if( tilesets[i].firstId <= tileId )
					return tilesets[i];
			}

			throw new Exception( string.Format( "tileId {0} was not foind in any tileset", tileId ) );
		}


		/// <summary>
		/// returns the TiledTilesetTile for the given id or null if none exists. TiledTilesetTiles exist only for animated tiles and tiles with
		/// properties set.
		/// </summary>
		/// <returns>The tileset tile.</returns>
		/// <param name="id">Identifier.</param>
		public TiledTilesetTile getTilesetTile( int id )
		{
			for( var i = tilesets.Count - 1; i >= 0; i-- )
			{
				if( tilesets[i].firstId <= id )
				{
					for( var j = 0; j < tilesets[i].tiles.Count; j++ )
					{
						// id is a gid so we need to subtract the tileset.firstId to get a local id
						if( tilesets[i].tiles[j].id == id - tilesets[i].firstId )
							return tilesets[i].tiles[j];
					}
				}
			}

			return null;
		}


		/// <summary>
		/// gets the index in the layers List of the layer with name
		/// </summary>
		/// <returns>The layer index.</returns>
		/// <param name="name">Name.</param>
		public int getLayerIndex( string name )
		{
			for( var i = 0; i < layers.Count; i++ )
			{
				if( layers[i].name == name )
					return i;
			}

			throw new Exception( "could not find the layer: " + name );
		}


		/// <summary>
		/// gets the TiledLayer by name
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="name">Name.</param>
		public TiledLayer getLayer( string name )
		{
			for( var i = 0; i < layers.Count; i++ )
			{
				if( layers[i].name == name )
					return layers[i];
			}
			return null;
		}


		/// <summary>
		/// gets the TiledLayer at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns>The Layer.</returns>
		public TiledLayer getLayer( int index )
		{
			return layers[index];
		}


		/// <summary>
		/// gets the TiledLayer by index
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getLayer<T>( int index ) where T : TiledLayer
		{
			return (T)getLayer( index );
		}


		/// <summary>
		/// gets the TiledLayer by name
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getLayer<T>( string name ) where T : TiledLayer
		{
			return (T)getLayer( name );
		}


		/// <summary>
		/// gets the TiledObjectGroup with the given name
		/// </summary>
		/// <returns>The object group.</returns>
		/// <param name="name">Name.</param>
		public TiledObjectGroup getObjectGroup( string name )
		{
			for( var i = 0; i < objectGroups.Count; i++ )
			{
				if( objectGroups[i].name == name )
					return objectGroups[i];
			}
			return null;
		}

		#endregion


		/// <summary>
		/// handles calling update on all animated tiles
		/// </summary>
		public void update()
		{
			for( var i = 0; i < _animatedTiles.Count; i++ )
				_animatedTiles[i].update();
		}


		/// <summary>
		/// calls draw on each visible layer
		/// </summary>
		/// <param name="batcher">Sprite batch.</param>
		/// <param name="position">Position.</param>
		/// <param name="layerDepth">Layer depth.</param>
		/// <param name="cameraClipBounds">Camera clip bounds.</param>
		public void draw( Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds )
		{
			// render any visible image or tile layer
			foreach( var layer in layers )
			{
				if( !layer.visible )
					continue;

				layer.draw( batcher, position, layerDepth, cameraClipBounds );
			}
		}


		#region world/local conversion

		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="pos">Position.</param>
		public Point worldToTilePosition( Vector2 pos, bool clampToTilemapBounds = true )
		{
			return new Point( worldToTilePositionX( pos.X, clampToTilemapBounds ), worldToTilePositionY( pos.Y, clampToTilemapBounds ) );
		}

		/// <summary>
		/// converts from world to tile position for isometric map clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="pos">Position.</param>
		public Point isometricWorldToTilePosition( Vector2 pos, bool clampToTilemapBounds = true )
		{
			return isometricWorldToTilePosition( pos.X, pos.Y, clampToTilemapBounds );
		}

		/// <summary>
		/// converts from world to tile position for isometric map clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public Point isometricWorldToTilePosition( float x, float y, bool clampToTilemapBounds = true )
		{
			x -= ( height - 1 ) * tileWidth / 2;
			var tileX = Mathf.fastFloorToInt( ( y / tileHeight ) + ( x / tileWidth ) );
			var tileY = Mathf.fastFloorToInt( ( -x / tileWidth ) + ( y / tileHeight ) );
			if( !clampToTilemapBounds )
				return new Point( tileX, tileY );
			return new Point( Mathf.clamp( tileX, 0, width - 1 ), Mathf.clamp( tileY, 0, height - 1 ) );
		}

		/// converts from isometric tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="pos">Position.</param>
		public Vector2 isometricTileToWorldPosition( Point pos )
		{
			return isometricTileToWorldPosition( pos.X, pos.Y );
		}

		/// <summary>
		/// converts from isometric tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public Vector2 isometricTileToWorldPosition( int x, int y )
		{
			var worldX = x * tileWidth / 2 - y * tileWidth / 2 + ( height - 1 ) * tileWidth / 2;
			var worldY = y * tileHeight / 2 + x * tileHeight / 2;
			return new Vector2( worldX, worldY );
		}

		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position x.</returns>
		/// <param name="x">The x coordinate.</param>
		public int worldToTilePositionX( float x, bool clampToTilemapBounds = true )
		{
			var tileX = Mathf.fastFloorToInt( x / tileWidth );
			if( !clampToTilemapBounds )
				return tileX;
			return Mathf.clamp( tileX, 0, width - 1 );
		}


		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position y.</returns>
		/// <param name="y">The y coordinate.</param>
		public int worldToTilePositionY( float y, bool clampToTilemapBounds = true )
		{
			var tileY = Mathf.fastFloorToInt( y / tileHeight );
			if( !clampToTilemapBounds )
				return tileY;
			return Mathf.clamp( tileY, 0, height - 1 );
		}

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="pos">Position.</param>
		public Vector2 tileToWorldPosition( Point pos )
		{
			return new Vector2( tileToWorldPositionX( pos.X ), tileToWorldPositionY( pos.Y ) );
		}

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position x.</returns>
		/// <param name="x">The x coordinate.</param>
		public int tileToWorldPositionX( int x )
		{
			return x * tileWidth;
		}


		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position y.</returns>
		/// <param name="y">The y coordinate.</param>
		public int tileToWorldPositionY( int y )
		{
			return y * tileHeight;
		}

		#endregion

	}
}
