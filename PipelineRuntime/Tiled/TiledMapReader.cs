using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;


namespace Nez.Tiled
{
	public class TiledMapReader : ContentTypeReader<TiledMap>
	{
		protected override TiledMap Read( ContentReader reader, TiledMap existingInstance )
		{
			var backgroundColor = reader.ReadColor();
			var renderOrder = (TiledRenderOrder)Enum.Parse( typeof( TiledRenderOrder ), reader.ReadString(), true );
			var tiledMap = new TiledMap( firstGid: reader.ReadInt32(),
										 width: reader.ReadInt32(),
				                         height: reader.ReadInt32(),
				                         tileWidth: reader.ReadInt32(),
				                         tileHeight: reader.ReadInt32(),
				                         orientation: (TiledMapOrientation)reader.ReadInt32() )
			{
				backgroundColor = backgroundColor,
				renderOrder = renderOrder
			};

			readCustomProperties( reader, tiledMap.properties );

			var tilesetCount = reader.ReadInt32();
			for( var i = 0; i < tilesetCount; i++ )
			{
				var textureAssetName = reader.getRelativeAssetPath( reader.ReadString() );
				var texture = reader.ContentManager.Load<Texture2D>( textureAssetName );
				var tileset = tiledMap.createTileset(
					                          texture: texture,
					                          firstId: reader.ReadInt32(),
					                          tileWidth: reader.ReadInt32(),
					                          tileHeight: reader.ReadInt32(),
					                          spacing: reader.ReadInt32(),
					                          margin: reader.ReadInt32() );
				readCustomProperties( reader, tileset.properties );
			}

			var layerCount = reader.ReadInt32();
			for( var i = 0; i < layerCount; i++ )
			{
				var layer = readLayer( reader, tiledMap );
				readCustomProperties( reader, layer.properties );
			}


			var objectGroupCount = reader.ReadInt32();
			for( var i = 0; i < objectGroupCount; i++ )
			{
				readObjectGroup( reader, tiledMap );
			}

			return tiledMap;
		}


		private static void readCustomProperties( ContentReader reader, Dictionary<string,string> properties )
		{
			var count = reader.ReadInt32();
			for( var i = 0; i < count; i++ )
				properties.Add( reader.ReadString(), reader.ReadString() );
		}


		private static TiledLayer readLayer( ContentReader reader, TiledMap tiledMap )
		{
			var layerName = reader.ReadString();
			var visible = reader.ReadBoolean();
			var opacity = reader.ReadSingle();
			var layerType = reader.ReadString();

			TiledLayer layer;
			if( layerType == "TileLayer" )
				layer = readTileLayer( reader, tiledMap, layerName );
			else if( layerType == "ImageLayer" )
				layer = readImageLayer( reader, tiledMap, layerName );
			else
				throw new NotSupportedException( string.Format( "Layer type {0} with name {1} is not supported", layerType, layerName ) );


			layer.visible = visible;
			layer.opacity = opacity;

			return layer;
		}


		private static TiledTileLayer readTileLayer( ContentReader reader, TiledMap tileMap, string layerName )
		{
			var tileDataCount = reader.ReadInt32();
			var tileData = new TiledTile[tileDataCount];

			for( var d = 0; d < tileDataCount; d++ )
			{
				var tileId = reader.ReadInt32();
				var flippedHorizonally = reader.ReadBoolean();
				var flippedVertically = reader.ReadBoolean();
				var flippedDiagonally = reader.ReadBoolean();

				// dont add null tiles
				if( tileId != 0 )
				{
					tileData[d] = new TiledTile( tileId )
					{
						flippedHorizonally = flippedHorizonally,
						flippedVertically = flippedVertically,
						flippedDiagonally = flippedDiagonally
					};
				}
				else
				{
					tileData[d] = null;
				}
			}

			return tileMap.createTileLayer(
				name: layerName,
				width: reader.ReadInt32(),
				height: reader.ReadInt32(),
				data: tileData );
		}


		private static TiledImageLayer readImageLayer( ContentReader reader, TiledMap tileMap, string layerName )
		{
			var assetName = reader.getRelativeAssetPath( reader.ReadString() );
			var texture = reader.ContentManager.Load<Texture2D>( assetName );
			var position = reader.ReadVector2();

			return tileMap.createImageLayer( layerName, texture, position );
		}


		private static TiledObjectGroup readObjectGroup( ContentReader reader, TiledMap tiledMap )
		{
			var objectGroup = tiledMap.createObjectGroup(
				reader.ReadString(), reader.ReadColor(), reader.ReadBoolean(), reader.ReadSingle() );

			readCustomProperties( reader, objectGroup.properties );

			var objectCount = reader.ReadInt32();
			objectGroup.objects = new TiledObject[objectCount];
			for( var i = 0; i < objectCount; i++ )
			{
				var obj = new TiledObject()
				{
					gid = reader.ReadInt32(),
					name = reader.ReadString(),
					type = reader.ReadString(),
					x = reader.ReadInt32(),
					y = reader.ReadInt32(),
					width = reader.ReadInt32(),
					height = reader.ReadInt32(),
					rotation = reader.ReadInt32(),
					visible = reader.ReadBoolean()
				};
						
				var tiledObjectType = reader.ReadString();
				if( tiledObjectType == "ellipse" )
				{
					// ellipse has no extra props
					obj.tiledObjectType = TiledObject.TiledObjectType.Ellipse;
				}
				else if( tiledObjectType == "image" )
				{
					obj.tiledObjectType = TiledObject.TiledObjectType.Image;
					throw new NotImplementedException( "Image objects are not yet implemented" );
				}
				else if( tiledObjectType == "polygon" )
				{
					obj.tiledObjectType = TiledObject.TiledObjectType.Polygon;
					obj.polyPoints = readVector2Array( reader );
				}
				else if( tiledObjectType == "polyline" )
				{
					obj.tiledObjectType = TiledObject.TiledObjectType.Polyline;
					obj.polyPoints = readVector2Array( reader );
				}
				else
				{
					obj.tiledObjectType = TiledObject.TiledObjectType.None;
				}

				obj.objectType = reader.ReadString();

				readCustomProperties( reader, obj.properties );

				objectGroup.objects[i] = obj;
			}

			return objectGroup;
		}


		static Vector2[] readVector2Array( ContentReader reader )
		{
			var pointCount = reader.ReadInt32();
			var points = new Vector2[pointCount];

			for( var i = 0; i < pointCount; i++ )
				points[i] = reader.ReadVector2();

			return points;
		}
	
	}
}