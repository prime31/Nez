using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.Tiled;


namespace Nez.TiledMaps
{
	[ContentTypeWriter]
	public class TiledMapWriter : ContentTypeWriter<TmxMap>
	{
		const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
		const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
		const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;


		protected override void Write( ContentWriter writer, TmxMap map )
		{
			int largestTileWidth = 0, largestTileHeight = 0;
			foreach( var tileset in map.tilesets )
			{
				largestTileWidth = Math.Max( largestTileWidth, tileset.tileWidth );
				largestTileHeight = Math.Max( largestTileHeight, tileset.tileHeight );
			}

			writer.Write( hexToColor( map.backgroundColor ) );
			writer.Write( map.renderOrder.ToString() );
			writer.Write( map.firstGid );
			writer.Write( map.width );
			writer.Write( map.height );
			writer.Write( map.tileWidth );
			writer.Write( map.tileHeight );
			writer.Write( Convert.ToInt32( map.orientation ) );
			writer.Write( largestTileWidth );
			writer.Write( largestTileHeight );
			writeCustomProperties( writer, map.properties );

			writer.Write( map.tilesets.Count );
			foreach( var tileset in map.tilesets )
			{
				if( tileset.image != null )
					TiledMapProcessor.logger.LogMessage( "\nExpecting texture asset: {0}\n", tileset.image.source );
				
				writer.Write( tileset.isStandardTileset );

				if( tileset.image != null )
					writer.Write( removeExtension( tileset.image.source ) );
				else
					writer.Write( string.Empty );
				
				writer.Write( tileset.firstGid );
				writer.Write( tileset.tileWidth );
				writer.Write( tileset.tileHeight );
				writer.Write( tileset.spacing );
				writer.Write( tileset.margin );
				writer.Write( tileset.tileCount );
				writer.Write( tileset.columns );
				writeCustomProperties( writer, tileset.properties );

				writer.Write( tileset.tiles.Count );
				foreach( var tile in tileset.tiles )
				{
					TiledMapProcessor.logger.LogMessage( "writing tile: {0}", tile );
					writer.Write( tile.id );

					// animation frames
					writer.Write( tile.animationFrames.Count );
					foreach( var anim in tile.animationFrames )
					{
						writer.Write( anim.tileId );
						writer.Write( anim.duration );
					}

					// image is optional
					if( tile.image != null )
					{
						writer.Write( true );
						writer.Write( tile.sourceRect.X );
						writer.Write( tile.sourceRect.Y );
						writer.Write( tile.sourceRect.Width );
						writer.Write( tile.sourceRect.Height );
					}
					else
					{
						writer.Write( false );
					}

					writeCustomProperties( writer, tile.properties );
				}
			}


			writer.Write( map.layers.Count );
			foreach( var layer in map.layers )
			{
				writer.Write( layer.name );
				writer.Write( layer.visible );
				writer.Write( layer.opacity );
				writer.Write( new Vector2( layer.offsetx, layer.offsety ) );

				var tileLayer = layer as TmxTileLayer;
				if( tileLayer != null )
				{
					writer.Write( (int)TiledLayerType.Tile );
					writer.Write( tileLayer.data.tiles.Count );
					foreach( var tile in tileLayer.data.tiles )
					{
						// Read out the flags
						var flippedHorizontally = ( tile.gid & FLIPPED_HORIZONTALLY_FLAG ) != 0;
						var flippedVertically = ( tile.gid & FLIPPED_VERTICALLY_FLAG ) != 0;
						var flippedDiagonally = ( tile.gid & FLIPPED_DIAGONALLY_FLAG ) != 0;

						if( flippedHorizontally || flippedVertically || flippedDiagonally )
						{
							// Clear the flags
							tile.gid &= ~( FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG );
							tile.flippedHorizontally = flippedHorizontally;
							tile.flippedVertically = flippedVertically;
							tile.flippedDiagonally = flippedDiagonally;
						}
						writer.Write( tile.gid );
						writer.Write( tile.flippedHorizontally );
						writer.Write( tile.flippedVertically );
						writer.Write( tile.flippedDiagonally );
					}

					writer.Write( tileLayer.width ); 
					writer.Write( tileLayer.height );
				}

				var imageLayer = layer as TmxImageLayer;
				if( imageLayer != null )
				{
					writer.Write( (int)TiledLayerType.Image );
					writer.Write( removeExtension( imageLayer.image.source ) );

					TiledMapProcessor.logger.LogMessage( "Expecting texture asset: {0}\n", imageLayer.image.source );
				}

				writeCustomProperties( writer, layer.properties );
				TiledMapProcessor.logger.LogMessage( "done writing Layer: {0}", layer );
			}

			writer.Write( map.objectGroups.Count );
			foreach( var group in map.objectGroups )
			{
				writer.Write( group.name );
				writer.Write( hexToColor( group.color ) );
				writer.Write( group.visible );
				writer.Write( group.opacity );

				writeCustomProperties( writer, group.properties );

				writer.Write( group.objects.Count );
				foreach( var obj in group.objects )
				{
					writer.Write( obj.gid );
					writer.Write( obj.name ?? string.Empty );
					writer.Write( obj.type ?? string.Empty );
					writer.Write( (int)obj.x );
					writer.Write( (int)obj.y );
					writer.Write( (int)obj.width );
					writer.Write( (int)obj.height );
					writer.Write( obj.rotation );
					writer.Write( obj.visible );

					if( obj.ellipse != null )
					{
						writer.Write( "ellipse" );
					}
					else if( obj.image != null )
					{
						writer.Write( "image" );
					}
					else if( obj.polygon != null )
					{
						writer.Write( "polygon" );
						writePointList( writer, obj, obj.polygon.points );
					}
					else if( obj.polyline != null )
					{
						writer.Write( "polyline" );
						writePointList( writer, obj, obj.polyline.points );
					}
					else
					{
						writer.Write( "none" );
					}

					writer.Write( obj.type ?? string.Empty );

					writeCustomProperties( writer, obj.properties );
				}
				
				TiledMapProcessor.logger.LogMessage( "done writing ObjectGroup: {0}", group );
			}
		}


		static void writePointList( ContentWriter writer, TmxObject obj, List<Vector2> points )
		{
			writer.Write( points.Count );
			for( var i = 0; i < points.Count; i++ )
			{
				// here we offset each point by the actual x/y value of the object so they can be used directly
				var pt = points[i];
				pt.X += obj.x;
				pt.Y += obj.y;
				writer.Write( pt );
			}
		}


		static void writeCustomProperties( ContentWriter writer, List<TmxProperty> properties )
		{
			writer.Write( properties.Count );
			foreach( var mapProperty in properties )
			{
				writer.Write( mapProperty.name );
				writer.Write( mapProperty.value );
			}
		}


		static Color hexToColor( string hexValue )
		{
			if( string.IsNullOrEmpty( hexValue ) )
				return new Color( 128, 128, 128 );

			hexValue = hexValue.TrimStart( '#' );
			var r = int.Parse( hexValue.Substring( 0, 2 ), NumberStyles.HexNumber );
			var g = int.Parse( hexValue.Substring( 2, 2 ), NumberStyles.HexNumber );
			var b = int.Parse( hexValue.Substring( 4, 2 ), NumberStyles.HexNumber );
			return new Color( r, g, b );
		}
			

		static string removeExtension( string path )
		{
			var dotIndex = path.LastIndexOf( '.' );
			return dotIndex > 0 ? path.Substring( 0, dotIndex ) : path;
		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( Nez.Tiled.TiledMap ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( Nez.Tiled.TiledMapReader ).AssemblyQualifiedName;
		}

	}
}