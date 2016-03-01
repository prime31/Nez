using System;
using Nez.Tiled;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	public class TiledMapComponent : RenderableComponent, IUpdatable
	{
		public TiledMap tiledmap;

		/// <summary>
		/// if null, all layers will be rendered
		/// </summary>
		public int[] layerIndicesToRender;

		public override float width
		{
			get { return tiledmap.width; }
		}

		public override float height
		{
			get { return tiledmap.height; }
		}

		public TiledTileLayer collisionLayer;
		Collider[] _colliders;


		public TiledMapComponent( TiledMap tiledmap, string collisionLayerName = null )
		{
			this.tiledmap = tiledmap;

			if( collisionLayerName != null )
				collisionLayer = tiledmap.getLayer<TiledTileLayer>( collisionLayerName );
		}


		/// <summary>
		/// this method requires that you are using a collision layer setup in the constructor.
		/// </summary>
		/// <returns>The tile at world position.</returns>
		/// <param name="worldPos">World position.</param>
		public TiledTile getTileAtWorldPosition( Vector2 worldPos )
		{
			// offset the passed in world position to compensate for the entity position
			worldPos -= entity.transform.position + _localPosition;
			return collisionLayer.getTileAtWorldPosition( worldPos );
		}


		/// <summary>
		/// gets all the non-empty tiles that intersect the passed in bounds
		/// </summary>
		/// <returns>The tiles intersecting bounds.</returns>
		/// <param name="bounds">Bounds.</param>
		public List<TiledTile> getTilesIntersectingBounds( Rectangle bounds )
		{
			// offset the passed in world position to compensate for the entity position
			bounds.Location -= ( entity.transform.position + _localPosition ).ToPoint();
			return collisionLayer.getTilesIntersectingBounds( bounds );
		}


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


		public void update()
		{
			tiledmap.update();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( layerIndicesToRender == null )
			{
				tiledmap.draw( graphics.spriteBatch, entity.transform.position + _localPosition, layerDepth, camera.bounds );
			}
			else
			{
				for( var i = 0; i < tiledmap.layers.Count; i++ )
				{
					if( layerIndicesToRender.contains( i ) && tiledmap.layers[i].visible )
						tiledmap.layers[i].draw( graphics.spriteBatch, entity.transform.position + _localPosition, layerDepth, camera.bounds );
				}
			}
		}


		public override void debugRender( Graphics graphics )
		{
			foreach( var group in tiledmap.objectGroups )
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
			var renderPosition = entity.transform.position + _localPosition;

			// create colliders for the rects we received
			_colliders = new Collider[collisionRects.Count];
			for( var i = 0; i < collisionRects.Count; i++ )
			{
				var collider = new BoxCollider( collisionRects[i].X + renderPosition.X, collisionRects[i].Y + renderPosition.Y, collisionRects[i].Width, collisionRects[i].Height );
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
				Physics.removeCollider( collider, true );
			_colliders = null;
		}

		#endregion


		#region Rendering helpers

		void renderObjectGroup( TiledObjectGroup group, Graphics graphics )
		{
			var renderPosition = entity.transform.position + _localPosition;

			foreach( var obj in group.objects )
			{
				if( !obj.visible )
					continue;

				switch( obj.tiledObjectType )
				{
					case TiledObject.TiledObjectType.Ellipse:
						graphics.spriteBatch.drawCircle( new Vector2( renderPosition.X + obj.x + obj.width * 0.5f, renderPosition.Y + obj.y + obj.height * 0.5f ), obj.width * 0.5f, group.color );
						break;
					case TiledObject.TiledObjectType.Image:
						throw new NotImplementedException( "Image layers are not yet supported" );
					case TiledObject.TiledObjectType.Polygon:
						graphics.spriteBatch.drawPoints( renderPosition, obj.polyPoints, group.color, true );
						break;
					case TiledObject.TiledObjectType.Polyline:
						graphics.spriteBatch.drawPoints( renderPosition, obj.polyPoints, group.color, false );
						break;
					case TiledObject.TiledObjectType.None:
						graphics.spriteBatch.drawHollowRect( renderPosition.X + obj.x, renderPosition.Y + obj.y, obj.width, obj.height, group.color );
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		#endregion

	}
}

