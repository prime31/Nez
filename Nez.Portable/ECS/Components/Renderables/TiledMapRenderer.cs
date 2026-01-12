using Nez.Tiled;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Nez
{
	public class TiledMapRenderer : RenderableComponent, IUpdatable
	{
		public TmxMap TiledMap;

		public int PhysicsLayer = 1 << 0;

		/// <summary>
		/// if null, all layers will be rendered
		/// </summary>
		public ITmxLayer[] LayersToRender;

		public bool AutoUpdateTilesets = true;

		public override float Width => TiledMap.Width * TiledMap.TileWidth;
		public override float Height => TiledMap.Height * TiledMap.TileHeight;

		public TmxLayer CollisionLayer;

		bool _shouldCreateColliders;
		Collider[] _colliders;


		public TiledMapRenderer(TmxMap tiledMap, string collisionLayerName = null, bool shouldCreateColliders = true)
		{
			TiledMap = tiledMap;

			_shouldCreateColliders = shouldCreateColliders;

			if (collisionLayerName != null)
				CollisionLayer = tiledMap.GetLayer(collisionLayerName) as TmxLayer;
		}

		/// <summary>
		/// sets this component to only render a single layer
		/// </summary>
		/// <param name="layerName">Layer name.</param>
		/// <param name="separator">An optional separator character to use to get layers nested in group layers.</param>
		public void SetLayerToRender(string layerName, char separator = '/')
		{
			LayersToRender = new ITmxLayer[1];
			LayersToRender[0] = TiledMap.GetLayer(layerName, separator);
		}

		/// <summary>
		/// sets which layers should be rendered by this component by name. If you know the indices you can use
		/// <see cref="SetLayerIndicesToRender"/> instead.
		/// </summary>
		/// <param name="separator">An optional separator character to use to get layers nested in group layers.</param>
		/// <param name="layerNames">Layer names.</param>
		public void SetLayersToRender(char separator, params string[] layerNames)
		{
			LayersToRender = new ITmxLayer[layerNames.Length];

			for (var i = 0; i < layerNames.Length; i++)
				LayersToRender[i] = TiledMap.GetLayer(layerNames[i], separator);
		}

		/// <summary>
		/// sets which layers should be rendered by this component by name. If you know the indices you can use
		/// <see cref="SetLayerIndicesToRender"/> instead.
		/// </summary>
		/// <param name="layerNames">Layer names.</param>
		public void SetLayersToRender(params string[] layerNames) => SetLayersToRender('/', layerNames);

		/// <summary>
		/// sets which layers should be rendered by this component by index. Because the index is used on <see cref="TmxMap.Layers"/>,
		/// using this function restricts you to top-level layers in the map.
		/// </summary>
		/// <param name="layerIndices">Layer indices.</param>
		public void SetLayerIndicesToRender(params int[] layerIndices)
		{
			LayersToRender = new ITmxLayer[layerIndices.Length];

			for (int i = 0; i < layerIndices.Length; i++)
				LayersToRender[i] = TiledMap.Layers[i];
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
			return TiledMap.WorldToTilePositionX(xPos);
		}

		/// <summary>
		/// this method requires that you are using a collision layer setup in the constructor.
		/// </summary>
		public TmxLayerTile GetTileAtWorldPosition(Vector2 worldPos)
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
		public List<TmxLayerTile> GetTilesIntersectingBounds(Rectangle bounds)
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

		public override void OnAddedToEntity() => AddColliders();

		public override void OnRemovedFromEntity() => RemoveColliders();

		public virtual void Update()
		{
			if (AutoUpdateTilesets)
				TiledMap.Update();
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			if (LayersToRender == null)
			{
				TiledRendering.RenderMap(TiledMap, batcher, Entity.Transform.Position + _localOffset, Transform.Scale, LayerDepth, camera.Bounds);
			}
			else
			{
				foreach (var layer in LayersToRender)
				{
					if (layer.Visible)
						TiledRendering.RenderLayer(layer, batcher, Entity.Transform.Position + _localOffset, Transform.Scale, LayerDepth, camera.Bounds);
				}
			}
		}

		public override void DebugRender(Batcher batcher)
		{
			foreach (var group in TiledMap.ObjectGroups)
				TiledRendering.RenderObjectGroup(group, batcher, Entity.Transform.Position + _localOffset, Transform.Scale, LayerDepth);

			if (_colliders != null)
			{
				foreach (var collider in _colliders)
					collider.DebugRender(batcher);
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
	}
}
