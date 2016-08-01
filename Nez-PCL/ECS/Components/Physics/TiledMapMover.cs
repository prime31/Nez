//#define DEBUG_MOVER
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Tiled
{
	/// <summary>
	/// WIP
	/// The TiledMapMover is a helper for moving objects around in a gravity-based Tiled map. It requires that the Entity it is on has a BoxCollider. The
	/// BoxCollider will be used in conjuntion with colliderHorizontal/VerticalInset for all collision detection.
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
			public bool right, left, above, below;
			public bool becameGroundedThisFrame;
			public bool wasGroundedLastFrame;
			public bool isGroundedOnOneWayPlatform;
			public float slopeAngle;

			public bool hasCollision { get { return below || right || left || above; } }


			public void reset()
			{
				becameGroundedThisFrame = isGroundedOnOneWayPlatform = right = left = above = below = false;
				slopeAngle = 0f;
			}


			public void reset( Vector2 motion )
			{
				if( motion.X == 0 )
					right = left = false;

				if( motion.Y == 0 )
					above = below = false;
				
				becameGroundedThisFrame = isGroundedOnOneWayPlatform = false;
				slopeAngle = 0f;
			}


			public override string ToString()
			{
				return string.Format( "[CollisionState] r: {0}, l: {1}, a: {2}, b: {3}, angle: {4}, wasGroundedLastFrame: {5}, becameGroundedThisFrame: {6}",
					right, left, above, below, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame );
			}

		}


		/// <summary>
		/// current collision state of the Entity
		/// </summary>
		public CollisionState collisionState = new CollisionState();

		/// <summary>
		/// the inset on the horizontal plane that the BoxCollider will be shrunk by when moving vertically
		/// </summary>
		public int colliderHorizontalInset = 2;

		/// <summary>
		/// the inset on the vertical plane that the BoxCollider will be shrunk by when moving horizontally
		/// </summary>
		public int colliderVerticalInset = 6;

		/// <summary>
		/// the TiledTileLayer used for collision checks
		/// </summary>
		public TiledTileLayer collisionLayer;

		TiledMap _tiledMap;
		BoxCollider _collider;
		float _movementRemainderX, _movementRemainderY;
		TiledTile _lastGroundTile;

		/// <summary>
		/// temporary storage for all the tiles that intersect the bounds being checked
		/// </summary>
		List<TiledTile> _collidingTiles = new List<TiledTile>();


		public TiledMapMover( TiledTileLayer collisionLayer )
		{
			this.collisionLayer = collisionLayer;
			_tiledMap = collisionLayer.tiledMap;
			Assert.isNotNull( collisionLayer, nameof( collisionLayer ) + " is required" );
		}


		public override void onAddedToEntity()
		{
			//setRenderLayer( -1 );
			_collider = entity.getCollider<BoxCollider>();
			Assert.isNotNull( _collider, "Entity must have a BoxCollider" );
		}


		/// <summary>
		/// moves the Entity taking into account the tiled map
		/// </summary>
		/// <param name="motion">Motion.</param>
		public void move( Vector2 motion )
		{
			//_debugTiles.Clear();
			// save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
			collisionState.wasGroundedLastFrame = collisionState.below;

			// reset our collisions state
			collisionState.reset( motion );

			// deal with subpixel movement, storing off any non-integar remainder for the next frame
			_movementRemainderX += motion.X;
			var motionX = Mathf.truncateToInt( _movementRemainderX );
			_movementRemainderX -= motionX;
			motion.X = motionX;

			_movementRemainderY += motion.Y;
			var motionY = Mathf.truncateToInt( _movementRemainderY );
			_movementRemainderY -= motionY;

			// due to subpixel movement we might end up with 0 gravity when we really want there to be at least 1 pixel so slopes can work
			if( collisionState.below && motionY == 0 && _movementRemainderY > 0 )
			{
				motionY = 1;
				_movementRemainderY = 0;
			}

			motion.Y = motionY;

			// store off the bounds so we can move it around without affecting the actual Transform position
			var colliderBounds = _collider.bounds;

			// first, check movement in the horizontal dir
			if( motionX != 0 )
			{
				var direction = motionX > 0 ? Edge.Right : Edge.Left;
				var sweptBounds = collisionRectForSide( direction, motionX );

				int collisionResponse;
				if( testMapCollision( sweptBounds, direction, out collisionResponse ) )
				{
					// react to collision. get the distance between our leading edge and what we collided with
					motion.X = collisionResponse - colliderBounds.getSide( direction );
					collisionState.left = direction == Edge.Left;
					collisionState.right = direction == Edge.Right;
					_movementRemainderX = 0;
				}
				else
				{
					collisionState.left = false;
					collisionState.right = false;
				}
			}

			// next, check movement in the vertical dir
			{
				var direction = motionY >= 0 ? Edge.Bottom : Edge.Top;
				var sweptBounds = collisionRectForSide( direction, motionY );
				sweptBounds.X += (int)motion.X;

				int collisionResponse;
				if( testMapCollision( sweptBounds, direction, out collisionResponse ) )
				{
					// react to collision. get the distance between our leading edge and what we collided with
					motion.Y = collisionResponse - colliderBounds.getSide( direction );
					collisionState.above = direction == Edge.Top;
					collisionState.below = direction == Edge.Bottom;
					_movementRemainderY = 0;
				}
				else
				{
					collisionState.above = false;
					collisionState.below = false;
					_lastGroundTile = null;
				}


				// when moving down we also check for collisions in the opposite direction. this needs to be done so that ledge bumps work when
				// a jump is made but misses by the colliderVerticalInset
				if( direction == Edge.Bottom )
				{
					direction = direction.oppositeEdge();
					sweptBounds = collisionRectForSide( direction, 0 );
					sweptBounds.X += (int)motion.X;
					sweptBounds.Y += (int)motion.Y;

					if( testMapCollision( sweptBounds, direction, out collisionResponse ) )
					{
						// react to collision. get the distance between our leading edge and what we collided with
						motion.Y = collisionResponse - colliderBounds.getSide( direction );
						// if we collide here this is an overlap of a slope above us. this small bump down will prevent hitches when hitting
						// our head on a slope that connects to a solid tile. It puts us below the slope when the normal response would put us
						// above it
						motion.Y += 2;
						collisionState.above = true;
					}
				}
			}

			// move then update our state
			_collider.unregisterColliderWithPhysicsSystem();
			transform.position += motion;
			_collider.registerColliderWithPhysicsSystem();

			// set our becameGrounded state based on the previous and current collision state
			if( !collisionState.wasGroundedLastFrame && collisionState.below )
				collisionState.becameGroundedThisFrame = true;
		}


		bool testMapCollision( Rectangle collisionRect, Edge direction, out int collisionResponse )
		{
			collisionResponse = 0;
			var side = direction.oppositeEdge();
			var perpindicularPosition = side.isVertical() ? collisionRect.Center.X : collisionRect.Center.Y;
			var leadingPosition = collisionRect.getSide( direction );
			var shouldTestSlopes = side.isVertical();
			populateCollidingTiles( collisionRect, direction );

			for( var i = 0; i < _collidingTiles.Count; i++ )
			{
				if( _collidingTiles[i] == null )
					continue;

				// disregard horizontal collisions if the last tile we were grounded on was a slope. Our y movement will push us up on the slope.
				// this is not a fantastic solution
				if( direction.isHorizontal() && _lastGroundTile != null && _lastGroundTile.isSlope() && _collidingTiles[i].isSlope() )
					return false;
				
				if( testTileCollision( _collidingTiles[i], side, perpindicularPosition, leadingPosition, shouldTestSlopes, out collisionResponse ) )
				{
					// store off our last ground tile if we collided below
					if( direction == Edge.Bottom )
					{
						_lastGroundTile = _collidingTiles[i];
						collisionState.isGroundedOnOneWayPlatform = _lastGroundTile.isOneWayPlatform();
					}
					
					return true;
				}

				// special case for sloped ground tiles
				if( _lastGroundTile != null && direction == Edge.Bottom )
				{
					// if grounded on a slope and intersecting a slope or if grounded on a wall and intersecting a tall slope we go sticky.
					// tall slope here means one where the the slopeTopLeft/Right is 0, i.e. it connects to a wall
					if( ( _lastGroundTile.isSlope() && _collidingTiles[i].isSlope() ) || ( !_lastGroundTile.isSlope() && _collidingTiles[i].isSlope() /* should be tall slope check */ ) )
					{
						// store off our last ground tile if we collided below
						_lastGroundTile = _collidingTiles[i];
						return true;
					}
				}
			}

			return false;
		}


		/// <summary>
		/// Tests the tile for a collision. Returns via out the position in world space where the collision occured.
		/// </summary>
		/// <returns>The tile collision.</returns>
		/// <param name="tile">Tile.</param>
		/// <param name="edgeToTest">the opposite side of movement, the side the leading edge will collide with</param>
		/// <param name="perpindicularPosition">Perpindicular position.</param>
		/// <param name="leadingPosition">Leading position.</param>
		/// <param name="shouldTestSlopes">Should test slopes.</param>
		/// <param name="collisionResponse">Collision response.</param>
		bool testTileCollision( TiledTile tile, Edge edgeToTest, int perpindicularPosition, int leadingPosition, bool shouldTestSlopes, out int collisionResponse )
		{
			collisionResponse = leadingPosition;

			// one way platforms are only collideable from the top when the player is already above them
			if( tile.isOneWayPlatform() )
			{
				// only the top edge of one way platforms are checked for collisions
				if( edgeToTest != Edge.Top )
					return false;

				// our response should be the top of the platform
				collisionResponse = _tiledMap.tileToWorldPositionX( tile.y );
				return _collider.bounds.bottom <= collisionResponse;
			}

			var forceSlopedTileCheckAsWall = false;

			// when moving horizontally the only time a slope is considered for collision testing is when its closest side is the tallest side
			// and we were not intesecting the tile before moving.
			// this prevents clipping through a tile when hitting its edge: -> |\
			if( edgeToTest.isHorizontal() && tile.isSlope() && tile.getNearestEdge( leadingPosition ) == tile.getHighestSlopeEdge() )
			{
				var moveDir = edgeToTest.oppositeEdge();
				var leadingPositionPreMovement = _collider.bounds.getSide( moveDir );

				// we need the tile x position that is on the opposite side of our move direction. Moving right we want the left edge
				var tileX = moveDir == Edge.Right ? _tiledMap.tileToWorldPositionX( tile.x ) : _tiledMap.tileToWorldPositionX( tile.x + 1 );

				// using the edge before movement, we see if we were colliding before moving.
				var wasCollidingBeforeMove = moveDir == Edge.Right ? leadingPositionPreMovement > tileX : leadingPositionPreMovement < tileX;

				// if we were not colliding before moving we need to consider this tile for a collision check as if it were a wall tile
				forceSlopedTileCheckAsWall = !wasCollidingBeforeMove;
			}

			if( forceSlopedTileCheckAsWall || !tile.isSlope() )
			{				
				switch( edgeToTest )
				{
					case Edge.Top:
						collisionResponse = _tiledMap.tileToWorldPositionY( tile.y );
						break;
					case Edge.Bottom:
						collisionResponse = _tiledMap.tileToWorldPositionY( tile.y + 1 );
						break;
					case Edge.Left:
						collisionResponse = _tiledMap.tileToWorldPositionX( tile.x );
						break;
					case Edge.Right:
						collisionResponse = _tiledMap.tileToWorldPositionX( tile.x + 1 );
						break;
				}

				return true;
			}

			if( shouldTestSlopes )
			{
				var tileWorldX = _tiledMap.tileToWorldPositionX( tile.x );
				var tileWorldY = _tiledMap.tileToWorldPositionX( tile.y );
				var slope = tile.getSlope();
				var offset = tile.getSlopeOffset();

				// calculate the point on the slope at perpindicularPosition
				collisionResponse = (int)( edgeToTest.isVertical() ? slope * ( perpindicularPosition - tileWorldX ) + offset + tileWorldY : ( perpindicularPosition - tileWorldY - offset ) / slope + tileWorldX );
				var isColliding = edgeToTest.isMax() ? leadingPosition <= collisionResponse : leadingPosition >= collisionResponse;

				// this code ensures that we dont consider collisions on a slope while jumping up that dont intersect our collider.
				// It also makes sure when testing the bottom edge that the leadingPosition is actually above the collisionResponse.
				// It isn't totally perfect but it does the job
				if( isColliding && edgeToTest == Edge.Bottom && leadingPosition <= collisionResponse )
					isColliding = false;

				// if we are grounded on a slope store off the slopeAngle. Remember, edgeToTest is the opposite edge of our movement direction.
				if( isColliding && edgeToTest == Edge.Top )
					collisionState.slopeAngle = MathHelper.ToDegrees( (float)Math.Atan( slope ) );

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
		void populateCollidingTiles( Rectangle bounds, Edge direction )
		{
			_collidingTiles.Clear();
			var isHorizontal = direction.isHorizontal();
			var primaryAxis = isHorizontal ? Axis.X : Axis.Y;
			var oppositeAxis = primaryAxis == Axis.X ? Axis.Y : Axis.X;

			var oppositeDirection = direction.oppositeEdge();
			var firstPrimary = worldToTilePosition( bounds.getSide( oppositeDirection ), primaryAxis );
			var lastPrimary = worldToTilePosition( bounds.getSide( direction ), primaryAxis );
			var primaryIncr = direction.isMax() ? 1 : -1;

			var min = worldToTilePosition( isHorizontal ? bounds.Top : bounds.Left, oppositeAxis );
			var mid = worldToTilePosition( isHorizontal ? bounds.getCenter().Y : bounds.getCenter().X, oppositeAxis );
			var max = worldToTilePosition( isHorizontal ? bounds.Bottom : bounds.Right, oppositeAxis );

			var isPositive = mid - min < max - mid;
			var secondaryIncr = isPositive ? 1 : -1;
			var firstSecondary = isPositive ? min : max;
			var lastSecondary = !isPositive ? min : max;

			for( var primary = firstPrimary; primary != lastPrimary + primaryIncr; primary += primaryIncr )
			{
				for( var secondary = firstSecondary; secondary != lastSecondary + secondaryIncr; secondary += secondaryIncr )
				{
					var col = isHorizontal ? primary : secondary;
					var row = !isHorizontal ? primary : secondary;
					_collidingTiles.Add( collisionLayer.getTile( col, row ) );
				}
			}
		}


		/// <summary>
		/// returns the tile position clamped to the tiled map
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="worldPosition">World position.</param>
		/// <param name="axis">Axis.</param>
		int worldToTilePosition( float worldPosition, Axis axis )
		{
			if( axis == Axis.Y )
				return _tiledMap.worldToTilePositionY( worldPosition );
			return _tiledMap.worldToTilePositionX( worldPosition );
		}


		/// <summary>
		/// gets a collision rect for the given side expanded to take into account motion
		/// </summary>
		/// <returns>The rect for side.</returns>
		/// <param name="side">Side.</param>
		/// <param name="motion">Motion.</param>
		Rectangle collisionRectForSide( Edge side, int motion )
		{
			var bounds = (Rectangle)_collider.bounds;

			// for horizontal collision checks we use just a sliver for our bounds. Vertical gets the half rect so that it can properly push
			// up when intersecting a slope which is ignored when moving horizontally.
			if( side.isHorizontal() )
				bounds = bounds.getRectEdgePortion( side );
			else
				bounds = bounds.getHalfRect( side );

			// we contract horizontally for vertical movement and vertically for horizontal movement
			if( side.isVertical() )
				RectangleExt.contract( ref bounds, colliderHorizontalInset, 0 );
			else
				RectangleExt.contract( ref bounds, 0, colliderVerticalInset );

			// finally expand the side in the direction of movement
			RectangleExt.expandSide( ref bounds, side, motion );

			return bounds;
		}


		#if DEBUG_MOVER

		public override float width { get { return 10000; } }
		public override float height { get { return 10000; } }
		List<TiledTile> _debugTiles = new List<TiledTile>();

		public override void render( Graphics graphics, Camera camera )
		{
			for( var i = 0; i < _debugTiles.Count; i++ )
			{
				var t = _debugTiles[i];
				if( t == null )
					continue;
				var pos = t.getWorldPosition( _tiledMap );
				graphics.batcher.drawHollowRect( pos, _tiledMap.tileWidth, _tiledMap.tileHeight, Color.Yellow );

				Debug.drawText( Graphics.instance.bitmapFont, i.ToString(), pos, Color.White );
			}

			if( _lastGroundTile != null )
			{
				var pos = _lastGroundTile.getWorldPosition( _tiledMap );
				graphics.batcher.drawHollowRect( pos, _tiledMap.tileWidth, _tiledMap.tileHeight, Color.Magenta );
			}

			var bounds = collisionRectForSide( Edge.Top, 0 );
			graphics.batcher.drawHollowRect( bounds, Color.Orchid );

			bounds = collisionRectForSide( Edge.Bottom, 0 );
			graphics.batcher.drawHollowRect( bounds, Color.Orange );

			bounds = collisionRectForSide( Edge.Right, 0 );
			graphics.batcher.drawHollowRect( bounds, Color.Blue );

			bounds = collisionRectForSide( Edge.Left, 0 );
			graphics.batcher.drawHollowRect( bounds, Color.Green );
		}

		#endif

	}
}

