using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Tiled
{
	/// <summary>
	/// helper class to deal with rendering Tiled maps
	/// </summary>
	public static class TiledRendering
	{
		/// <summary>
		/// naively renders every layer present in the tilemap
		/// </summary>
		/// <param name="map"></param>
		/// <param name="batcher"></param>
		/// <param name="scale"></param>
		/// <param name="layerDepth"></param>
		/// <param name="cameraClipBounds"></param>
		public static void RenderMap(TmxMap map, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth, RectangleF cameraClipBounds)
		{
			for (var index = 0; index < map.Layers.Count; index++)
			{
				var layer = map.Layers[index];
				
				if (layer is TmxLayer tmxLayer && tmxLayer.Visible)
					RenderLayer(tmxLayer, batcher, position, scale, layerDepth, cameraClipBounds);
				else if (layer is TmxImageLayer tmxImageLayer && tmxImageLayer.Visible)
					RenderImageLayer(tmxImageLayer, batcher, position, scale, layerDepth);
				else if (layer is TmxGroup tmxGroup && tmxGroup.Visible)
					RenderGroup(tmxGroup, batcher, position, scale, layerDepth);
				else if (layer is TmxObjectGroup tmxObjGroup && tmxObjGroup.Visible)
					RenderObjectGroup(tmxObjGroup, batcher, position, scale, layerDepth);
			}
		}

		/// <summary>
		/// renders the ITmxLayer by calling through to the concrete type's render method
		/// </summary>
		public static void RenderLayer(ITmxLayer layer, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth, RectangleF cameraClipBounds)
		{
			if (layer is TmxLayer tmxLayer && tmxLayer.Visible)
				RenderLayer(tmxLayer, batcher, position, scale, layerDepth, cameraClipBounds);
			else if (layer is TmxImageLayer tmxImageLayer && tmxImageLayer.Visible)
				RenderImageLayer(tmxImageLayer, batcher, position, scale, layerDepth);
			else if (layer is TmxGroup tmxGroup && tmxGroup.Visible)
				RenderGroup(tmxGroup, batcher, position, scale, layerDepth);
			else if (layer is TmxObjectGroup tmxObjGroup && tmxObjGroup.Visible)
				RenderObjectGroup(tmxObjGroup, batcher, position, scale, layerDepth);
		}

		/// <summary>
		/// renders all tiles with no camera culling performed
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="batcher"></param>
		/// <param name="position"></param>
		/// <param name="scale"></param>
		/// <param name="layerDepth"></param>
		public static void RenderLayer(TmxLayer layer, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth)
		{
			if (!layer.Visible)
				return;

			var tileWidth = layer.Map.TileWidth * scale.X;
			var tileHeight = layer.Map.TileHeight * scale.Y;

			var color = Color.White;
			color.A = (byte)(layer.Opacity * 255);

			for (var i = 0; i < layer.Grid.Length; i++)
			{
				var tile = layer.GetTile(i);
				if (tile == null)
					continue;

				var x = i % layer.Map.TileWidth;
				var y = i / layer.Map.TileWidth;
				
				RenderTile(tile,x ,y, batcher, position,
					scale, tileWidth, tileHeight,
					color, layerDepth, layer.Map.Orientation,
					layer.Map.Width, layer.Map.Height);
			}
		}

		/// <summary>
		/// renders all tiles that are inside <paramref name="cameraClipBounds"/>
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="batcher"></param>
		/// <param name="position"></param>
		/// <param name="scale"></param>
		/// <param name="layerDepth"></param>
		/// <param name="cameraClipBounds"></param>
		public static void RenderLayer(TmxLayer layer, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth, RectangleF cameraClipBounds)
		{
			if (!layer.Visible)
				return;

			position += layer.Offset;

			// offset it by the entity position since the tilemap will always expect positions in its own coordinate space
			cameraClipBounds.Location -= position;

			var tileWidth = layer.Map.TileWidth * scale.X;
			var tileHeight = layer.Map.TileHeight * scale.Y;

			Point min, max;

			(min, max, cameraClipBounds) = GetLayerCullBounds(layer, scale, cameraClipBounds, tileWidth, tileHeight);

			var color = Color.White;
			color.A = (byte)(layer.Opacity * 255);

			// loop through and draw all the non-culled tiles
			for (var y = min.Y; y <= max.Y; y++)
			{
				for (var x = min.X; x <= max.X; x++)
				{
					var tile = layer.GetTile(x, y);
					if (tile != null)
						RenderTile(tile, x ,y, batcher, position,
							scale, tileWidth, tileHeight,
							color, layerDepth, layer.Map.Orientation,
							layer.Map.Width, layer.Map.Height);
				}
			}
		}

		private static (Point, Point, RectangleF) GetLayerCullBounds(TmxLayer layer, Vector2 scale, RectangleF cameraClipBounds, float tileWidth, float tileHeight)
		{
			var min = Point.Zero;
			var max = Point.Zero;

			// we expand our cameraClipBounds by the excess tile width/height of the largest tiles to ensure we include tiles whose
			// origin might be outside of the cameraClipBounds
			switch (layer.Map.Orientation)
			{
				case OrientationType.Hexagonal:
				case OrientationType.Isometric:
					if (layer.Map.RequiresLargeTileCulling)
					{
						cameraClipBounds.Location -= new Vector2(
							layer.Map.MaxTileWidth,
							layer.Map.MaxTileHeight - layer.Map.TileWidth
						);
						cameraClipBounds.Size += new Vector2(
							layer.Map.MaxTileWidth,
							layer.Map.MaxTileHeight - layer.Map.TileHeight
						);
					}

					max = new Point(layer.Map.Width - 1, layer.Map.Height - 1);

					break;
				case OrientationType.Staggered:
					throw new NotImplementedException(
						"Staggered Tiled maps are not yet supported."
					);
				case OrientationType.Unknown:
				case OrientationType.Orthogonal:
				default:
					if (layer.Map.RequiresLargeTileCulling)
					{
						min.X = layer.Map.WorldToTilePositionX(cameraClipBounds.Left - (layer.Map.MaxTileWidth * scale.X - tileWidth));
						min.Y = layer.Map.WorldToTilePositionY(cameraClipBounds.Top - (layer.Map.MaxTileHeight * scale.Y - tileHeight));
						max.X = layer.Map.WorldToTilePositionX(cameraClipBounds.Right + (layer.Map.MaxTileWidth * scale.X - tileWidth));
						max.Y = layer.Map.WorldToTilePositionY(cameraClipBounds.Bottom + (layer.Map.MaxTileHeight * scale.Y - tileHeight));
					}
					else
					{
						min.X = layer.Map.WorldToTilePositionX(cameraClipBounds.Left);
						min.Y = layer.Map.WorldToTilePositionY(cameraClipBounds.Top);
						max.X = layer.Map.WorldToTilePositionX(cameraClipBounds.Right);
						max.Y = layer.Map.WorldToTilePositionY(cameraClipBounds.Bottom);
					}
					break;
			}

			return (min, max, cameraClipBounds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RenderTile(TmxLayerTile tile, int tileX, int tileY, Batcher batcher, Vector2 position,
				Vector2 scale, float tileWidth, float tileHeight,
				Color color, float layerDepth, OrientationType orientation,
				int mapWidth, int mapHeight)
		{
			var gid = tile.Gid;

			// animated tiles (and tiles from image tilesets) will be inside the Tileset itself, in separate TmxTilesetTile
			// objects, not to be confused with TmxLayerTiles which we are dealing with in this loop
			var tilesetTile = tile.TilesetTile;
			if (tilesetTile != null && tilesetTile.AnimationFrames.Count > 0)
				gid = tilesetTile.currentAnimationFrameGid;

			var sourceRect = tile.Tileset.TileRegions[gid];

			// for the y position, we need to take into account if the tile is larger than the tileHeight and shift. Tiled uses
			// a bottom-left coordinate system and MonoGame a top-left
			float tx, ty;

			switch (orientation)
			{
				case OrientationType.Hexagonal:
					bool isEvenRow = tileY % 2 == 0;

					if (isEvenRow)
					{
						tx = tileX * tileWidth;
						ty = tileY * tileHeight * 0.75f;
					}
					else
					{
						tx = (tileWidth / 2) + (tileX * tileWidth);
						ty = tileY * tileHeight * 0.75f;
					}

					break;
				case OrientationType.Isometric:
					tx = tileX * tileWidth / 2 - tileY * tileWidth / 2 + (mapHeight - 1) * tileWidth / 2;
					ty = tileY * tileHeight / 2 + tileX * tileHeight / 2;
					break;
				case OrientationType.Staggered:
					throw new NotImplementedException(
						"Staggered Tiled maps are not yet supported."
					);

				case OrientationType.Unknown:
				case OrientationType.Orthogonal:
				default:
					tx = tileX * tileWidth;
					ty = tileY * tileHeight;
					break;
			}

			var rotation = 0f;

			var spriteEffects = SpriteEffects.None;
			if (tile.HorizontalFlip)
				spriteEffects |= SpriteEffects.FlipHorizontally;
			if (tile.VerticalFlip)
				spriteEffects |= SpriteEffects.FlipVertically;
			if (tile.DiagonalFlip)
			{
				if (tile.HorizontalFlip && tile.VerticalFlip)
				{
					spriteEffects ^= SpriteEffects.FlipVertically;
					rotation = MathHelper.PiOver2;
					tx += tileHeight + (sourceRect.Height * scale.Y - tileHeight);
					ty -= (sourceRect.Width * scale.X - tileWidth);
				}
				else if (tile.HorizontalFlip)
				{
					spriteEffects ^= SpriteEffects.FlipVertically;
					rotation = -MathHelper.PiOver2;
					ty += tileHeight;
				}
				else if (tile.VerticalFlip)
				{
					spriteEffects ^= SpriteEffects.FlipHorizontally;
					rotation = MathHelper.PiOver2;
					tx += tileWidth + (sourceRect.Height * scale.Y - tileHeight);
					ty += (tileWidth - sourceRect.Width * scale.X);
				}
				else
				{
					spriteEffects ^= SpriteEffects.FlipHorizontally;
					rotation = -MathHelper.PiOver2;
					ty += tileHeight;
				}
			}

			// if we had no rotations (diagonal flipping) shift our y-coord to account for any non map.tileSize tiles due to
			// Tiled being bottom-left origin
			if (rotation == 0)
				ty += (tileHeight - sourceRect.Height * scale.Y);

			var pos = new Vector2(tx, ty) + position;

			if (tile.Tileset.Image != null)
				batcher.Draw(tile.Tileset.Image.Texture, pos, sourceRect, color, rotation, Vector2.Zero, scale, spriteEffects, layerDepth);
			else
				batcher.Draw(tilesetTile.Image.Texture, pos, sourceRect, color, rotation, Vector2.Zero, scale, spriteEffects, layerDepth);
		}

		public static void RenderObjectGroup(TmxObjectGroup objGroup, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth)
		{
			if (!objGroup.Visible)
				return;

			for (var index = 0; index < objGroup.Objects.Count; index++)
			{
				var obj = objGroup.Objects[index];
				
				if (!obj.Visible)
					continue;

				// if we are not debug rendering, we only render Tile and Text types
				if (!Core.DebugRenderEnabled)
				{
					if (obj.ObjectType != TmxObjectType.Tile && obj.ObjectType != TmxObjectType.Text)
						continue;
				}

				var pos = position + new Vector2(obj.X, obj.Y) * scale;
				switch (obj.ObjectType)
				{
					case TmxObjectType.Basic:
						if(obj.Rotation != 0)
						{
							batcher.DrawHollowRect(pos, obj.Width * scale.X, obj.Height * scale.Y, objGroup.Color, Mathf.Radians(obj.Rotation), pos);
						}
						else
						{
							batcher.DrawHollowRect(pos, obj.Width * scale.X, obj.Height * scale.Y, objGroup.Color);
						}
						goto default;
					case TmxObjectType.Point:
						var size = objGroup.Map.TileWidth * 0.5f;
						pos.X -= size * 0.5f;
						pos.Y -= size * 0.5f;
						batcher.DrawPixel(pos, objGroup.Color, (int)size);
						goto default;
					case TmxObjectType.Tile:

						var spriteEffects = SpriteEffects.None;
						if (obj.Tile.HorizontalFlip)
							spriteEffects |= SpriteEffects.FlipHorizontally;
						if (obj.Tile.VerticalFlip)
							spriteEffects |= SpriteEffects.FlipVertically;

						var tileset = objGroup.Map.GetTilesetForTileGid(obj.Tile.Gid);
						var sourceRect = tileset.TileRegions[obj.Tile.Gid];
						batcher.Draw(tileset.Image.Texture, pos, sourceRect, Color.White, 0, Vector2.Zero, scale, spriteEffects, layerDepth);
						goto default;
					case TmxObjectType.Ellipse:
						pos = new Vector2(obj.X + obj.Width * 0.5f, obj.Y + obj.Height * 0.5f) * scale;
						batcher.DrawCircle(pos, obj.Width * 0.5f, objGroup.Color);
						goto default;
					case TmxObjectType.Polygon:
					case TmxObjectType.Polyline:
						var points = new Vector2[obj.Points.Length];
						for (var i = 0; i < obj.Points.Length; i++)
							points[i] = obj.Points[i] * scale;
						batcher.DrawPoints(pos, points, objGroup.Color, obj.ObjectType == TmxObjectType.Polygon);
						goto default;
					case TmxObjectType.Text:
						var fontScale = (float)obj.Text.PixelSize / Graphics.Instance.BitmapFont.LineHeight;
						batcher.DrawString(Graphics.Instance.BitmapFont, obj.Text.Value, pos, obj.Text.Color, Mathf.Radians(obj.Rotation), Vector2.Zero, fontScale, SpriteEffects.None, layerDepth);
						goto default;
					default:
						if (Core.DebugRenderEnabled)
							batcher.DrawString(Graphics.Instance.BitmapFont, $"{obj.Name} ({obj.Type})", pos - new Vector2(0, 15), Color.Black);
						break;
				}
			}
		}

		public static void RenderImageLayer(TmxImageLayer layer, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth)
		{
			if (!layer.Visible)
				return;

			var color = Color.White;
			color.A = (byte)(layer.Opacity * 255);

			var pos = position + new Vector2(layer.OffsetX, layer.OffsetY) * scale;
			batcher.Draw(layer.Image.Texture, pos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void RenderGroup(TmxGroup group, Batcher batcher, Vector2 position, Vector2 scale, float layerDepth)
		{
			if (!group.Visible)
				return;

			for (var index = 0; index < group.Layers.Count; index++)
			{
				var layer = group.Layers[index];
				
				if (layer is TmxGroup tmxSubGroup)
					RenderGroup(tmxSubGroup, batcher, position, scale, layerDepth);

				if (layer is TmxObjectGroup tmxObjGroup)
					RenderObjectGroup(tmxObjGroup, batcher, position, scale, layerDepth);

				if (layer is TmxLayer tmxLayer)
					RenderLayer(tmxLayer, batcher, position, scale, layerDepth);

				if (layer is TmxImageLayer tmxImageLayer)
					RenderImageLayer(tmxImageLayer, batcher, position, scale, layerDepth);
			}
		}

	}
}
