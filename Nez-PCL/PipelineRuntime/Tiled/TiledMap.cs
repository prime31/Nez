using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;


namespace Nez.Tiled
{
	public class TiledMap
	{
		#region Fields and Properties

		public int firstGid;
		public int width;
		public int height;
		public int tileWidth;
		public int tileHeight;
		public Color? backgroundColor;
		public TiledRenderOrder renderOrder;
		public Dictionary<string,string> properties;
		public TiledMapOrientation orientation;
		public List<TiledLayer> layers;
		public List<TiledImageLayer> imageLayers = new List<TiledImageLayer>();
		public List<TiledTileLayer> tileLayers = new List<TiledTileLayer>();
		public List<TiledObjectGroup> objectGroups;


		public int widthInPixels
		{
            // annoyingly we have to compensate 1 pixel per tile, seems to be a bug in MonoGame?
			get { return width * tileWidth - width; }       
		}

		public int heightInPixels
		{
			get { return height * tileHeight - height; }
		}

		readonly List<TiledTileset> _tilesets;

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
			properties = new Dictionary<string,string>();

			layers = new List<TiledLayer>();
			_tilesets = new List<TiledTileset>();
			objectGroups = new List<TiledObjectGroup>();
			this.orientation = orientation;
		}


		public TiledTileset createTileset( Texture2D texture, int firstId, int tileWidth, int tileHeight, int spacing = 2, int margin = 2 )
		{
			var tileset = new TiledTileset( texture, firstId, tileWidth, tileHeight, spacing, margin );
			_tilesets.Add( tileset );
			return tileset;
		}


		public TiledTileLayer createTileLayer( string name, int width, int height, TiledTile[] data )
		{
			var layer = new TiledTileLayer( this, name, width, height, data );
			layers.Add( layer );
			return layer;
		}


		public TiledImageLayer createImageLayer( string name, Texture2D texture, Vector2 position )
		{
			var layer = new TiledImageLayer( name, texture, position );
			layers.Add( layer );
			return layer;
		}


		public TiledObjectGroup createObjectGroup( string name, Color color, bool visible, float opacity )
		{
			var group = new TiledObjectGroup( name, color, visible, opacity );
			objectGroups.Add( group );
			return group;
		}


		public TiledLayer getLayer( string name )
		{
			foreach( var layer in layers )
			{
				if( layer.name == name )
					return layer;
			}
			return null;
		}


		public T getLayer<T>( string name ) where T : TiledLayer
		{
			return (T)getLayer( name );
		}


		public void draw( SpriteBatch spriteBatch, Vector2 position, float layerDepth, Rectangle cameraClipBounds )
		{
			// render any visible image or tile layer
			foreach( var layer in layers )
			{
				if( !layer.visible )
					continue;

				layer.draw( spriteBatch, position, layerDepth, cameraClipBounds );
			}
		}


		public void drawWithoutCullOrPositioning( SpriteBatch spriteBatch, bool useMapBackgroundColor = false )
		{
			if( useMapBackgroundColor && backgroundColor.HasValue )
				spriteBatch.GraphicsDevice.Clear( backgroundColor.Value );

			foreach( var layer in layers )
				layer.draw( spriteBatch );
		}


		public Subtexture getTileRegion( int id )
		{
			if( id == 0 )
				return null;

			for( var i = _tilesets.Count - 1; i >= 0; i-- )
			{
				if( _tilesets[i].firstId <= id )
					return _tilesets[i].getTileRegion( id );
			}

			throw new InvalidOperationException( string.Format( "No tileset found for id {0}", id ) );
		}


		public int worldPositionToTilePositionX( float x )
		{
			var tileX = (int)Math.Floor( x / tileWidth );
			return MathHelper.Clamp( tileX, 0, width - 1 );
		}


		public int worldPositionToTilePositionY( float y )
		{
			var tileY = (int)Math.Floor( y / tileHeight );
			return MathHelper.Clamp( tileY, 0, height - 1 );
		}
	
	}
}
