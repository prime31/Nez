using System;
using Nez.Tiled;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	public class TiledMapComponent : RenderableComponent, IUpdatable
	{
		public TiledMap TiledMap;

		public int PhysicsLayer = 1 << 0;

		/// <summary>
		/// if null, all layers will be rendered
		/// </summary>
		public int[] LayerIndicesToRender;

		public override float Width => TiledMap.Width * TiledMap.TileWidth;
		public override float Height => TiledMap.Height * TiledMap.TileHeight;

		public TiledTileLayer CollisionLayer;

		bool _shouldCreateColliders;
		Collider[] _colliders;


		public TiledMapComponent(TiledMap tiledMap, string collisionLayerName = null, bool shouldCreateColliders = true)
		{
			this.TiledMap = tiledMap;
			_shouldCreateColliders = shouldCreateColliders;

			if (collisionLayerName != null)
				CollisionLayer = tiledMap.GetLayer<TiledTileLayer>(collisionLayerName);
		}

		/// <summary>
		/// sets this component to only render a single layer
		/// </summary>
		/// <param name="layerName">Layer name.</param>
		public void SetLayerToRender(string layerName)
		{
			LayerIndicesToRender = new int[1];
			LayerIndicesToRender[0] = TiledMap.GetLayerIndex(layerName);
		}

		/// <summary>
		/// sets which layers should be rendered by this component by name. If you know the indices you can set layerIndicesToRender directly.
		/// </summary>
		/// <param name="layerNames">Layer names.</param>
		public void SetLayersToRender(params string[] layerNames)
		{
			LayerIndicesToRender = new int[layerNames.Length];

			for (var i = 0; i < layerNames.Length; i++)
				LayerIndicesToRender[i] = TiledMap.GetLayerIndex(layerNames[i]);
		}


		#region TiledMap queries

		public int GetRowAtWorldPosition(float yPos)
		{
			yPos -= Entity.Transform.Position.Y + _localOffset.Y;
			return TiledMap.WorldToTilePositionY(yPos);
		}

		public int GetColumnAtWorldPosition(float xPos)
		{
			xPos -= Entity.Transform.Position.X + _localOffset.X;
			return TiledMap.WorldToTilePositionY(xPos);
		}

		/// <summary>
		/// this method requires that you are using a collision layer setup in the constructor.
		/// </summary>
		/// <returns>The tile at world position.</returns>
		/// <param name="worldPos">World position.</param>
		public TiledTile GetTileAtWorldPosition(Vector2 worldPos)
		{
			Insist.IsNotNull(CollisionLayer, "collisionLayer must not be null!");

			// offset the passed in world position to compensate for the entity position
			worldPos -= Entity.Transform.Position + _localOffset;
			return CollisionLayer.GetTileAtWorldPosition(worldPos);
		}

		/// <summary>
		/// gets all the non-empty tiles that intersect the passed in bounds for the collision layer. The returned List can be put back in the
		/// pool via ListPool.free.
		/// </summary>
		/// <returns>The tiles intersecting bounds.</returns>
		/// <param name="bounds">Bounds.</param>
		public List<TiledTile> GetTilesIntersectingBounds(Rectangle bounds)
		{
			Insist.IsNotNull(CollisionLayer, "collisionLayer must not be null!");

			// offset the passed in world position to compensate for the entity position
			bounds.Location -= (Entity.Transform.Position + _localOffset).ToPoint();
			return CollisionLayer.GetTilesIntersectingBounds(bounds);
		}

		#endregion


		#region Component overrides

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			// we only deal with positional changes here. TiledMaps cant be scaled.
			if (_shouldCreateColliders && comp == Transform.Component.Position)
			{
				RemoveColliders();
				AddColliders();
			}
		}

		public override void OnAddedToEntity()
		{
			AddColliders();
		}

		public override void OnRemovedFromEntity()
		{
			RemoveColliders();
		}

		void IUpdatable.Update()
		{
			TiledMap.Update();
		}

		public override void Render(Graphics graphics, Camera camera)
		{
			if (LayerIndicesToRender == null)
			{
				TiledMap.Draw(graphics.Batcher, Entity.Transform.Position + _localOffset, Transform.Scale, LayerDepth,
					camera.Bounds);
			}
			else
			{
				for (var i = 0; i < TiledMap.Layers.Count; i++)
				{
					if (TiledMap.Layers[i].Visible && LayerIndicesToRender.Contains(i))
						TiledMap.Layers[i].Draw(graphics.Batcher, Entity.Transform.Position + _localOffset,
							Transform.Scale, LayerDepth, camera.Bounds);
				}
			}
		}

		public override void DebugRender(Graphics graphics)
		{
			foreach (var group in TiledMap.ObjectGroups)
				RenderObjectGroup(group, graphics);

			if (_colliders != null)
			{
				foreach (var collider in _colliders)
					collider.DebugRender(graphics);
			}
		}

		#endregion


		#region Colliders

		public void AddColliders()
		{
			if (CollisionLayer == null || !_shouldCreateColliders)
				return;

			// fetch the collision layer and its rects for collision
			var collisionRects = CollisionLayer.GetCollisionRectangles();

			// create colliders for the rects we received
			_colliders = new Collider[collisionRects.Count];
			for (var i = 0; i < collisionRects.Count; i++)
			{
				var collider = new BoxCollider(collisionRects[i].X + _localOffset.X,
					collisionRects[i].Y + _localOffset.Y, collisionRects[i].Width, collisionRects[i].Height);
				collider.PhysicsLayer = PhysicsLayer;
				collider.Entity = Entity;
				_colliders[i] = collider;

				Physics.AddCollider(collider);
			}
		}

		public void RemoveColliders()
		{
			if (_colliders == null)
				return;

			foreach (var collider in _colliders)
				Physics.RemoveCollider(collider);
			_colliders = null;
		}

		#endregion


		#region Rendering helpers

		void RenderObjectGroup(TiledObjectGroup group, Graphics graphics)
		{
			var renderPosition = Entity.Transform.Position + _localOffset;

			foreach (var obj in group.Objects)
			{
				if (!obj.Visible)
					continue;

				switch (obj.TiledObjectType)
				{
					case TiledObjectType.Ellipse:
						graphics.Batcher.DrawCircle(
							new Vector2(renderPosition.X + obj.X + obj.Width * 0.5f,
								renderPosition.Y + obj.Y + obj.Height * 0.5f), obj.Width * 0.5f, group.Color);
						break;
					case TiledObjectType.Image:
						throw new NotImplementedException("Image layers are not yet supported");
					case TiledObjectType.Polygon:
						graphics.Batcher.DrawPoints(renderPosition, obj.PolyPoints, group.Color, true);
						break;
					case TiledObjectType.Polyline:
						graphics.Batcher.DrawPoints(renderPosition, obj.PolyPoints, group.Color, false);
						break;
					case TiledObjectType.None:
						graphics.Batcher.DrawHollowRect(renderPosition.X + obj.X, renderPosition.Y + obj.Y, obj.Width,
							obj.Height, group.Color);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		#endregion
	}
}