using System;
using Nez.Tiled;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	public class TiledMapComponent : RenderableComponent, IUpdatable
	{
		public TiledMap tiledMap;

		public int physicsLayer = 1 << 0;

		/// <summary>
		/// if null, all layers will be rendered
		/// </summary>
		public int[] layerIndicesToRender;

		public override float width
		{
			get { return tiledMap.width * tiledMap.tileWidth; }
		}

		public override float height
		{
			get { return tiledMap.height * tiledMap.tileHeight; }
		}

		public TiledTileLayer collisionLayer;
		Collider[] _colliders;


		public TiledMapComponent( TiledMap tiledmap, string collisionLayerName = null )
		{
			this.tiledMap = tiledmap;

			if( collisionLayerName != null )
				collisionLayer = tiledmap.getLayer<TiledTileLayer>( collisionLayerName );
		}


		/// <summary>
		/// sets this component to only render a single layer
		/// </summary>
		/// <param name="layerName">Layer name.</param>
		public void setLayerToRender( string layerName )
		{
			layerIndicesToRender = new int[1];
			layerIndicesToRender[0] = tiledMap.getLayerIndex( layerName );
		}


		/// <summary>
		/// sets which layers should be rendered by this component by name. If you know the indices you can set layerIndicesToRender directly.
		/// </summary>
		/// <param name="layerNames">Layer names.</param>
		public void setLayersToRender( params string[] layerNames )
		{
			layerIndicesToRender = new int[layerNames.Length];

			for( var i = 0; i < layerNames.Length; i++ )
				layerIndicesToRender[i] = tiledMap.getLayerIndex( layerNames[i] );
		}


		#region TiledMap queries

		public int getRowAtWorldPosition( float yPos )
		{
			yPos -= entity.transform.position.Y + _localOffset.Y;
			return tiledMap.worldToTilePositionY( yPos );
		}


		public int getColumnAtWorldPosition( float xPos )
		{
			xPos -= entity.transform.position.X + _localOffset.X;
			return tiledMap.worldToTilePositionY( xPos );
		}


		/// <summary>
		/// this method requires that you are using a collision layer setup in the constructor.
		/// </summary>
		/// <returns>The tile at world position.</returns>
		/// <param name="worldPos">World position.</param>
		public TiledTile getTileAtWorldPosition( Vector2 worldPos )
		{
			Assert.isNotNull( collisionLayer, "collisionLayer must not be null!" );

			// offset the passed in world position to compensate for the entity position
			worldPos -= entity.transform.position + _localOffset;
			return collisionLayer.getTileAtWorldPosition( worldPos );
		}


		/// <summary>
		/// gets all the non-empty tiles that intersect the passed in bounds for the collision layer. The returned List can be put back in the
		/// pool via ListPool.free.
		/// </summary>
		/// <returns>The tiles intersecting bounds.</returns>
		/// <param name="bounds">Bounds.</param>
		public List<TiledTile> getTilesIntersectingBounds( Rectangle bounds )
		{
			Assert.isNotNull( collisionLayer, "collisionLayer must not be null!" );

			// offset the passed in world position to compensate for the entity position
			bounds.Location -= ( entity.transform.position + _localOffset ).ToPoint();
			return collisionLayer.getTilesIntersectingBounds( bounds );
		}

		#endregion


		#region Component overrides

		public override void onEntityTransformChanged()
		{
			removeColliders();
			addColliders();
		}


		public override void onAddedToEntity()
		{
			addColliders();
		}


		public override void onRemovedFromEntity()
		{
			removeColliders();
		}


		void IUpdatable.update()
		{
			tiledMap.update();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( layerIndicesToRender == null )
			{
				tiledMap.draw( graphics.batcher, entity.transform.position + _localOffset, layerDepth, camera.bounds );
			}
			else
			{
				for( var i = 0; i < tiledMap.layers.Count; i++ )
				{
					if( tiledMap.layers[i].visible && layerIndicesToRender.contains( i ) )
						tiledMap.layers[i].draw( graphics.batcher, entity.transform.position + _localOffset, layerDepth, camera.bounds );
				}
			}
		}


		public override void debugRender( Graphics graphics )
		{
			foreach( var group in tiledMap.objectGroups )
				renderObjectGroup( group, graphics );

			if( _colliders != null )
			{
				foreach( var collider in _colliders )
					collider.debugRender( graphics );
			}
		}

		#endregion


		#region Colliders

		public void addColliders()
		{
			if( collisionLayer == null )
				return;

			// fetch the collision layer and its rects for collision
			var collisionRects = collisionLayer.getCollisionRectangles();
			var renderPosition = entity.transform.position + _localOffset;

			// create colliders for the rects we received
			_colliders = new Collider[collisionRects.Count];
			for( var i = 0; i < collisionRects.Count; i++ )
			{
				var collider = new BoxCollider( collisionRects[i].X + renderPosition.X, collisionRects[i].Y + renderPosition.Y, collisionRects[i].Width, collisionRects[i].Height );
				collider.physicsLayer = physicsLayer;
				collider.entity = entity;
				_colliders[i] = collider;

				Physics.addCollider( collider );
			}
		}


		public void removeColliders()
		{
			if( _colliders == null )
				return;

			foreach( var collider in _colliders )
				Physics.removeCollider( collider );
			_colliders = null;
		}

		#endregion


		#region Rendering helpers

		void renderObjectGroup( TiledObjectGroup group, Graphics graphics )
		{
			var renderPosition = entity.transform.position + _localOffset;

			foreach( var obj in group.objects )
			{
				if( !obj.visible )
					continue;

				switch( obj.tiledObjectType )
				{
					case TiledObject.TiledObjectType.Ellipse:
						graphics.batcher.drawCircle( new Vector2( renderPosition.X + obj.x + obj.width * 0.5f, renderPosition.Y + obj.y + obj.height * 0.5f ), obj.width * 0.5f, group.color );
						break;
					case TiledObject.TiledObjectType.Image:
						throw new NotImplementedException( "Image layers are not yet supported" );
					case TiledObject.TiledObjectType.Polygon:
						graphics.batcher.drawPoints( renderPosition, obj.polyPoints, group.color, true );
						break;
					case TiledObject.TiledObjectType.Polyline:
						graphics.batcher.drawPoints( renderPosition, obj.polyPoints, group.color, false );
						break;
					case TiledObject.TiledObjectType.None:
						graphics.batcher.drawHollowRect( renderPosition.X + obj.x, renderPosition.Y + obj.y, obj.width, obj.height, group.color );
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		#endregion

	}
}

