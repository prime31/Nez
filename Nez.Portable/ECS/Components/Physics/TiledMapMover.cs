// #define DEBUG_MOVER

using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Tiled
{
	/// <summary>
	/// WIP
	/// The TiledMapMover is a helper for moving objects around in a gravity-based Tiled map. It requires that the Entity it is on has a BoxCollider. The
	/// BoxCollider will be used in conjunction with colliderHorizontal/VerticalInset for all collision detection.
	///
	/// One way platforms can be jumped down through by moving your Transform down 1 pixel and calling CollisionState.clearLastGroundTile.
	///
	/// If you plan to use slopes/one way platforms with the TiledMapMover some extra properties need to be added to your tiles in Tiled.
	/// They are listed below:
	/// - nez:isOneWayPlatform (bool): one way platforms will ignore all collisions except from above
	/// - nez:isSlope (bool): signifies if the tile is a slope. Requires the next two properties if it is
	/// - nez:slopeTopLeft (int): distance in pixels from the tiles top to the slope on the left side. For example, a 45 top-left to bottom-right
	/// tile |\ would have a slopeTopLeft of 0 and slopeTopRight of 15
	/// - nez:slopeTopRight (int): distance in pixels from the tiles top to the slope on the right side
	/// </summary>
	public class TiledMapMover :
#if DEBUG_MOVER
	RenderableComponent
#else
		Component
#endif
	{
		/// <summary>
		/// class used to house all the collision information from a call to move
		/// </summary>
		public class CollisionState
		{
			public bool Right, Left, Above, Below;
			public bool BecameGroundedThisFrame;
			public bool WasGroundedLastFrame;
			public bool IsGroundedOnOneWayPlatform;
			public float SlopeAngle;

			public bool HasCollision => Below || Right || Left || Above;

			// state used by the TiledMapMover
			internal SubpixelFloat _movementRemainderX, _movementRemainderY;
			internal TmxLayerTile _lastGroundTile;


			public void ClearLastGroundTile() => _lastGroundTile = null;


			public void Reset()
			{
				BecameGroundedThisFrame = IsGroundedOnOneWayPlatform = Right = Left = Above = Below = false;
				SlopeAngle = 0f;
			}

			/// <summary>
			/// resets collision state and does sub-pixel movement calculations
			/// </summary>
			/// <param name="motion">Motion.</param>
			public void Reset(ref Vector2 motion)
			{
				if (motion.X == 0)
					Right = Left = false;

				if (motion.Y == 0)
					Above = Below = false;

				BecameGroundedThisFrame = IsGroundedOnOneWayPlatform = false;
				SlopeAngle = 0f;

				// deal with subpixel movement, storing off any non-integar remainder for the next frame
				_movementRemainderX.Update(ref motion.X);
				_movementRemainderY.Update(ref motion.Y);

				// due to subpixel movement we might end up with 0 gravity when we really want there to be at least 1 pixel so slopes can work
				if (Below && motion.Y == 0 && _movementRemainderY.Remainder > 0)
				{
					motion.Y = 1;
					_movementRemainderY.Reset();
				}
			}


			public override string ToString()
			{
				return string.Format(
					"[CollisionState] r: {0}, l: {1}, a: {2}, b: {3}, angle: {4}, wasGroundedLastFrame: {5}, becameGroundedThisFrame: {6}",
					Right, Left, Above, Below, SlopeAngle, WasGroundedLastFrame, BecameGroundedThisFrame);
			}
		}

		/// <summary>
		/// the inset on the horizontal plane that the BoxCollider will be shrunk by when moving vertically
		/// </summary>
		public int ColliderHorizontalInset = 2;

		/// <summary>
		/// the inset on the vertical plane that the BoxCollider will be shrunk by when moving horizontally
		/// </summary>
		public int ColliderVerticalInset = 6;

		/// <summary>
		/// the TiledTileLayer used for collision checks
		/// </summary>
		public TmxLayer CollisionLayer;

		/// <summary>
		/// the TiledMap that contains collisionLayer
		/// </summary>
		public TmxMap TiledMap;

		/// <summary>
		/// temporary storage for all the coordinates of tiles that intersect the bounds being checked
		/// </summary>
		List<Point> _collidingTilesCoordinates = new List<Point>();

		/// <summary>
		/// temporary storage to avoid having to pass it around
		/// </summary>
		Rectangle _boxColliderBounds;


		public TiledMapMover()
		{ }

		public TiledMapMover(TmxLayer collisionLayer)
		{
			Insist.IsNotNull(collisionLayer, nameof(collisionLayer) + " is required");
			CollisionLayer = collisionLayer;
			TiledMap = collisionLayer.Map;
		}

		/// <summary>
		/// moves the Entity taking into account the tiled map
		/// </summary>
		/// <param name="motion">Motion.</param>
		/// <param name="boxCollider">Box collider.</param>
		public void Move(Vector2 motion, BoxCollider boxCollider, CollisionState collisionState)
		{
			if (TiledMap == null)
				return;

			// test for collisions then move the Entity
			TestCollisions(ref motion, boxCollider.Bounds, collisionState);

			boxCollider.UnregisterColliderWithPhysicsSystem();
			boxCollider.Entity.Transform.Position += motion;
			boxCollider.RegisterColliderWithPhysicsSystem();
		}

		public void TestCollisions(ref Vector2 motion, Rectangle boxColliderBounds, CollisionState collisionState)
		{
			_boxColliderBounds = boxColliderBounds;

			// save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
			collisionState.WasGroundedLastFrame = collisionState.Below;

			// reset our collisions state
			collisionState.Reset(ref motion);

			// reset rounded motion for us while dealing with subpixel movement so fetch the rounded values to use for our actual detection
			var motionX = (int)motion.X;
			var motionY = (int)motion.Y;

			// first, check movement in the horizontal dir
			if (motionX != 0)
			{
				var direction = motionX > 0 ? Edge.Right : Edge.Left;
				var sweptBounds = CollisionRectForSide(direction, motionX);

				int collisionResponse;
				if (TestMapCollision(sweptBounds, direction, collisionState, out collisionResponse))
				{
					// react to collision. get the distance between our leading edge and what we collided with
					motion.X = collisionResponse - boxColliderBounds.GetSide(direction);
					collisionState.Left = direction == Edge.Left;
					collisionState.Right = direction == Edge.Right;
					collisionState._movementRemainderX.Reset();
				}
				else
				{
					collisionState.Left = false;
					collisionState.Right = false;
				}
			}

			// next, check movement in the vertical dir
			{
				var direction = motionY >= 0 ? Edge.Bottom : Edge.Top;
				var sweptBounds = CollisionRectForSide(direction, motionY);
				sweptBounds.X += (int)motion.X;

				int collisionResponse;
				if (TestMapCollision(sweptBounds, direction, collisionState, out collisionResponse))
				{
					// react to collision. get the distance between our leading edge and what we collided with
					motion.Y = collisionResponse - boxColliderBounds.GetSide(direction);
					collisionState.Above = direction == Edge.Top;
					collisionState.Below = direction == Edge.Bottom;
					collisionState._movementRemainderY.Reset();

					if (collisionState.Below && collisionState._lastGroundTile != null &&
						collisionState._lastGroundTile.IsSlope())
						collisionState.SlopeAngle =
							MathHelper.ToDegrees((float)Math.Atan(collisionState._lastGroundTile.GetSlope()));
				}
				else
				{
					collisionState.Above = false;
					collisionState.Below = false;
					collisionState._lastGroundTile = null;
				}


				// when moving down we also check for collisions in the opposite direction. this needs to be done so that ledge bumps work when
				// a jump is made but misses by the colliderVerticalInset
				if (direction == Edge.Bottom)
				{
					direction = direction.OppositeEdge();
					sweptBounds = CollisionRectForSide(direction, 0);
					sweptBounds.X += (int)motion.X;
					sweptBounds.Y += (int)motion.Y;

					if (TestMapCollision(sweptBounds, direction, collisionState, out collisionResponse))
					{
						// react to collision. get the distance between our leading edge and what we collided with
						motion.Y = collisionResponse - boxColliderBounds.GetSide(direction);

						// if we collide here this is an overlap of a slope above us. this small bump down will prevent hitches when hitting
						// our head on a slope that connects to a solid tile. It puts us below the slope when the normal response would put us
						// above it
						motion.Y += 2;
						collisionState.Above = true;
					}
				}
			}

			// set our becameGrounded state based on the previous and current collision state
			if (!collisionState.WasGroundedLastFrame && collisionState.Below)
				collisionState.BecameGroundedThisFrame = true;
		}

		bool TestMapCollision(Rectangle collisionRect, Edge direction, CollisionState collisionState,
							  out int collisionResponse)
		{
			collisionResponse = 0;
			var side = direction.OppositeEdge();
			var perpindicularPosition = side.IsVertical() ? collisionRect.Center.X : collisionRect.Center.Y;
			var leadingPosition = collisionRect.GetSide(direction);
			var shouldTestSlopes = side.IsVertical();
			PopulateCollidingTiles(collisionRect, direction);

			for (var i = 0; i < _collidingTilesCoordinates.Count; i++)
			{
				var collidingTile = CollisionLayer.GetTile(_collidingTilesCoordinates[i].X, _collidingTilesCoordinates[i].Y);

				if (collidingTile == null)
					continue;

				// disregard horizontal collisions with tiles on the same row as a slope if the last tile we were grounded on was a slope.
				// the y collision response will push us up on the slope.
				if (direction.IsHorizontal() && collisionState._lastGroundTile != null &&
					collisionState._lastGroundTile.IsSlope() && IsSlopeCollisionRow(_collidingTilesCoordinates[i].Y))
					continue;

				if (TestTileCollision(collidingTile, _collidingTilesCoordinates[i].X, _collidingTilesCoordinates[i].Y, side, perpindicularPosition, leadingPosition,
					shouldTestSlopes, out collisionResponse))
				{
					// store off our last ground tile if we collided below
					if (direction == Edge.Bottom)
					{
						collisionState._lastGroundTile = collidingTile;
						collisionState.IsGroundedOnOneWayPlatform = collisionState._lastGroundTile.IsOneWayPlatform();
					}

					return true;
				}

				// special case for sloped ground tiles
				if (collisionState._lastGroundTile != null && direction == Edge.Bottom)
				{
					// if grounded on a slope and intersecting a slope or if grounded on a wall and intersecting a tall slope we go sticky.
					// tall slope here means one where the the slopeTopLeft/Right is 0, i.e. it connects to a wall
					var isHighSlopeNearest = collidingTile.IsSlope() &&
											 collidingTile.GetNearestEdge(_collidingTilesCoordinates[i].X, perpindicularPosition) ==
											 collidingTile.GetHighestSlopeEdge();
					if ((collisionState._lastGroundTile.IsSlope() && collidingTile.IsSlope()) ||
						(!collisionState._lastGroundTile.IsSlope() && isHighSlopeNearest))
					{
						// store off our last ground tile if we collided below
						collisionState._lastGroundTile = collidingTile;
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Checks whether collision is occurring with a slope on a given row.
		/// </summary>
		/// <returns>Whether collision is occurring with a slope on a given row</returns>
		/// <param name="rowY">the row to check</param>
		bool IsSlopeCollisionRow(int rowY)
		{
			for (var i = 0; i < _collidingTilesCoordinates.Count; i++)
			{
				var collidingTile = CollisionLayer.GetTile(_collidingTilesCoordinates[i].X, _collidingTilesCoordinates[i].Y);

				if (collidingTile != null && collidingTile.IsSlope() && _collidingTilesCoordinates[i].Y == rowY)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Tests the tile for a collision. Returns via out the position in world space where the collision occured.
		/// </summary>
		/// <returns>The tile collision.</returns>
		/// <param name="tile">Tile.</param>
		/// /// <param name="x">x position of the tile.</param>
		/// /// <param name="y">y position of the tile..</param>
		/// <param name="edgeToTest">the opposite side of movement, the side the leading edge will collide with</param>
		/// <param name="perpindicularPosition">Perpindicular position.</param>
		/// <param name="leadingPosition">Leading position.</param>
		/// <param name="shouldTestSlopes">Should test slopes.</param>
		/// <param name="collisionResponse">Collision response.</param>
		bool TestTileCollision(TmxLayerTile tile, int x, int y, Edge edgeToTest, int perpindicularPosition, int leadingPosition,
							   bool shouldTestSlopes, out int collisionResponse)
		{
			collisionResponse = leadingPosition;

			// one way platforms are only collideable from the top when the player is already above them
			if (tile.IsOneWayPlatform())
			{
				// only the top edge of one way platforms are checked for collisions
				if (edgeToTest != Edge.Top)
					return false;

				// our response should be the top of the platform
				collisionResponse = TiledMap.TileToWorldPositionX(y);
				return _boxColliderBounds.Bottom <= collisionResponse;
			}

			var forceSlopedTileCheckAsWall = false;

			// when moving horizontally the only time a slope is considered for collision testing is when its closest side is the tallest side
			// and we were not intesecting the tile before moving.
			// this prevents clipping through a tile when hitting its edge: -> |\
			if (edgeToTest.IsHorizontal() && tile.IsSlope() &&
				tile.GetNearestEdge(x, leadingPosition) == tile.GetHighestSlopeEdge())
			{
				var moveDir = edgeToTest.OppositeEdge();
				var leadingPositionPreMovement = _boxColliderBounds.GetSide(moveDir);

				// we need the tile x position that is on the opposite side of our move direction. Moving right we want the left edge
				var tileX = moveDir == Edge.Right
					? TiledMap.TileToWorldPositionX(x)
					: TiledMap.TileToWorldPositionX(x + 1);

				// using the edge before movement, we see if we were colliding before moving.
				var wasCollidingBeforeMove = moveDir == Edge.Right
					? leadingPositionPreMovement > tileX
					: leadingPositionPreMovement < tileX;

				// if we were not colliding before moving we need to consider this tile for a collision check as if it were a wall tile
				forceSlopedTileCheckAsWall = !wasCollidingBeforeMove;
			}


			if (forceSlopedTileCheckAsWall || !tile.IsSlope())
			{
				switch (edgeToTest)
				{
					case Edge.Top:
						collisionResponse = TiledMap.TileToWorldPositionY(y);
						break;
					case Edge.Bottom:
						collisionResponse = TiledMap.TileToWorldPositionY(y + 1);
						break;
					case Edge.Left:
						collisionResponse = TiledMap.TileToWorldPositionX(x);
						break;
					case Edge.Right:
						collisionResponse = TiledMap.TileToWorldPositionX(x + 1);
						break;
				}

				return true;
			}

			if (shouldTestSlopes)
			{
				var tileWorldX = TiledMap.TileToWorldPositionX(x);
				var tileWorldY = TiledMap.TileToWorldPositionX(y);
				var slope = tile.GetSlope();
				var offset = tile.GetSlopeOffset();

				// calculate the point on the slope at perpindicularPosition
				collisionResponse = (int)(edgeToTest.IsVertical()
					? slope * (perpindicularPosition - tileWorldX) + offset + tileWorldY
					: (perpindicularPosition - tileWorldY - offset) / slope + tileWorldX);
				var isColliding = edgeToTest.IsMax()
					? leadingPosition <= collisionResponse
					: leadingPosition >= collisionResponse;

				// this code ensures that we dont consider collisions on a slope while jumping up that dont intersect our collider.
				// It also makes sure when testing the bottom edge that the leadingPosition is actually above the collisionResponse.
				// HACK: It isn't totally perfect but it does the job
				if (isColliding && edgeToTest == Edge.Bottom && leadingPosition <= collisionResponse)
					isColliding = false;

				return isColliding;
			}

			return false;
		}

		/// <summary>
		/// gets a list of all the tiles intersecting bounds. The returned list is ordered for collision detection based on the
		/// direction passed in so they can be processed in order.
		/// </summary>
		/// <returns>The colliding tiles.</returns>
		/// <param name="bounds">Bounds.</param>
		/// <param name="direction">Direction.</param>
		void PopulateCollidingTiles(Rectangle bounds, Edge direction)
		{
			_collidingTilesCoordinates.Clear();
			var isHorizontal = direction.IsHorizontal();
			var primaryAxis = isHorizontal ? Axis.X : Axis.Y;
			var oppositeAxis = primaryAxis == Axis.X ? Axis.Y : Axis.X;

			var oppositeDirection = direction.OppositeEdge();
			var firstPrimary = WorldToTilePosition(bounds.GetSide(oppositeDirection), primaryAxis);
			var lastPrimary = WorldToTilePosition(bounds.GetSide(direction), primaryAxis);
			var primaryIncr = direction.IsMax() ? 1 : -1;

			var min = WorldToTilePosition(isHorizontal ? bounds.Top : bounds.Left, oppositeAxis);
			var mid = WorldToTilePosition(isHorizontal ? bounds.GetCenter().Y : bounds.GetCenter().X, oppositeAxis);
			var max = WorldToTilePosition(isHorizontal ? bounds.Bottom : bounds.Right, oppositeAxis);

			var isPositive = mid - min < max - mid;
			var secondaryIncr = isPositive ? 1 : -1;
			var firstSecondary = isPositive ? min : max;
			var lastSecondary = !isPositive ? min : max;

			for (var primary = firstPrimary; primary != lastPrimary + primaryIncr; primary += primaryIncr)
			{
				for (var secondary = firstSecondary;
					secondary != lastSecondary + secondaryIncr;
					secondary += secondaryIncr)
				{
					var col = isHorizontal ? primary : secondary;
					var row = !isHorizontal ? primary : secondary;
					_collidingTilesCoordinates.Add(new Point(col, row));

#if DEBUG_MOVER
					if(direction.IsHorizontal())
					{
						var pos = TiledMap.TileToWorldPosition(new Point(col, row));
						_debugTiles.Add(new Rectangle((int)pos.X, (int)pos.Y, 16, 16));
					}
#endif
				}
			}
		}

		/// <summary>
		/// returns the tile position clamped to the tiled map
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="worldPosition">World position.</param>
		/// <param name="axis">Axis.</param>
		int WorldToTilePosition(float worldPosition, Axis axis)
		{
			if (axis == Axis.Y)
				return TiledMap.WorldToTilePositionY(worldPosition);

			return TiledMap.WorldToTilePositionX(worldPosition);
		}

		/// <summary>
		/// gets a collision rect for the given side expanded to take into account motion
		/// </summary>
		/// <returns>The rect for side.</returns>
		/// <param name="side">Side.</param>
		/// <param name="motion">Motion.</param>
		Rectangle CollisionRectForSide(Edge side, int motion)
		{
			Rectangle bounds;

			// for horizontal collision checks we use just a sliver for our bounds. Vertical gets the half rect so that it can properly push
			// up when intersecting a slope which is ignored when moving horizontally.
			if (side.IsHorizontal())
				bounds = _boxColliderBounds.GetRectEdgePortion(side);
			else
				bounds = _boxColliderBounds.GetHalfRect(side);

			// we contract horizontally for vertical movement and vertically for horizontal movement
			if (side.IsVertical())
				RectangleExt.Contract(ref bounds, ColliderHorizontalInset, 0);
			else
				RectangleExt.Contract(ref bounds, 0, ColliderVerticalInset);

			// finally expand the side in the direction of movement
			RectangleExt.ExpandSide(ref bounds, side, motion);

			return bounds;
		}


#if DEBUG_MOVER
		public override float Width { get { return 10000; } }
		public override float Height { get { return 10000; } }
		List<Rectangle> _debugTiles = new List<Rectangle>();

		public override void Render(Batcher batcher, Camera camera)
		{
			for (var i = 0; i < _debugTiles.Count; i++)
			{
				var t = _debugTiles[i];
				batcher.DrawHollowRect(t, Color.Yellow);

				Debug.DrawText(Graphics.Instance.BitmapFont, i.ToString(), t.Center.ToVector2(), Color.White);
			}
			_debugTiles.Clear();

			var bounds = CollisionRectForSide(Edge.Top, 0);
			batcher.DrawHollowRect(bounds, Color.Orchid);

			bounds = CollisionRectForSide(Edge.Bottom, 0);
			batcher.DrawHollowRect(bounds, Color.Orange);

			bounds = CollisionRectForSide(Edge.Right, 0);
			batcher.DrawHollowRect(bounds, Color.Blue);

			bounds = CollisionRectForSide(Edge.Left, 0);
			batcher.DrawHollowRect(bounds, Color.Green);
		}

#endif
	}
}
