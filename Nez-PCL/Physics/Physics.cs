using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Spatial;


namespace Nez
{
	public static class Physics
	{
		static SpatialHash _spatialHash;

		public const int AllLayers = -1;
		public static int spatialHashCellSize = 100;

		/// <summary>
		/// Do raycasts detect Colliders configured as triggers?
		/// </summary>
		public static bool raycastsHitTriggers = false;

		/// <summary>
		/// Do ray/line casts that start inside a collider detect those colliders?
		/// </summary>
		public static bool raycastsStartInColliders = false;


		/// <summary>
		/// we keep this around to avoid allocating it every time a raycast happens
		/// </summary>
		static RaycastHit[] _hitArray = new RaycastHit[1];


		public static void reset()
		{
			_spatialHash = new SpatialHash( spatialHashCellSize );
		}


		/// <summary>
		/// debug draws the contents of the spatial hash. Note that Core.debugRenderEnabled must be true or nothing will be displayed.
		/// </summary>
		/// <param name="secondsToDisplay">Seconds to display.</param>
		public static void debugDraw( float secondsToDisplay )
		{
			_spatialHash.debugDraw( secondsToDisplay, 2f );
		}


		#region Collider management

		/// <summary>
		/// gets all the Colliders managed by the SpatialHash
		/// </summary>
		/// <returns>The all colliders.</returns>
		public static HashSet<Collider> getAllColliders()
		{
			return _spatialHash.getAllObjects();
		}


		/// <summary>
		/// adds the collider to the physics system
		/// </summary>
		/// <param name="collider">Collider.</param>
		public static void addCollider( Collider collider )
		{
			_spatialHash.register( collider );
		}


		/// <summary>
		/// removes the collider from the physics system. uses brute force if useColliderBoundsForRemovalLookup is false. Only resort
		/// to the brute force method if you already moved the Entity/Collider before wanting to remove it.
		/// </summary>
		/// <param name="collider">Collider.</param>
		/// <param name="useColliderBoundsForRemovalLookup">If set to <c>true</c> use collider bounds for removal lookup.</param>
		public static void removeCollider( Collider collider, bool useColliderBoundsForRemovalLookup )
		{
			if( useColliderBoundsForRemovalLookup )
			{
				var bounds = collider.bounds;
				_spatialHash.remove( collider, ref bounds );
			}
			else
			{
				_spatialHash.remove( collider );
			}
		}


		/// <summary>
		/// removes teh collider from the phyics system. bounds should be the last bounds value that the collider had in the physics system.
		/// </summary>
		/// <param name="collider">Collider.</param>
		/// <param name="bounds">Bounds.</param>
		public static void removeCollider( Collider collider, ref Rectangle bounds )
		{
			_spatialHash.remove( collider, ref bounds );
		}


		/// <summary>
		/// updates the colliders position in the physics system. preUpdateColliderBounds should be the bounds of the collider before it
		/// was changed
		/// </summary>
		/// <param name="collider">Collider.</param>
		/// <param name="colliderBounds">Collider bounds.</param>
		public static void updateCollider( Collider collider, ref Rectangle preUpdateColliderBounds )
		{
			_spatialHash.remove( collider, ref preUpdateColliderBounds );
			_spatialHash.register( collider );
		}

		#endregion


		/// <summary>
		/// casts a ray from start to end and returns the first hit of a collider that matches layerMask
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static RaycastHit linecast( Vector2 start, Vector2 end, int layerMask = AllLayers )
		{
			// cleanse the collider before proceeding
			_hitArray[0].reset();
			linecastAll( start, end, _hitArray, layerMask );
			return _hitArray[0];
		}


		public static int linecastAll( Vector2 start, Vector2 end, RaycastHit[] hits, int layerMask = AllLayers )
		{
			Assert.isFalse( hits.Length == 0, "An empty hits array was passed in. No hits will ever be returned." );
			return _spatialHash.linecastAll( start, end, hits, layerMask );
		}


		/// <summary>
		/// check if a collider falls within a rectangular area
		/// </summary>
		/// <returns>The rectangle.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider overlapRectangle( Rectangle rect, int layerMask = AllLayers )
		{
			return overlapRectangle( ref rect, layerMask );
		}


		/// <summary>
		/// check if a collider falls within a rectangular area
		/// </summary>
		/// <returns>The rectangle.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider overlapRectangle( ref Rectangle rect, int layerMask = AllLayers )
		{
			var potentials = _spatialHash.aabbBroadphase( ref rect, null, layerMask );
			foreach( var collider in potentials )
			{
				if( collider is BoxCollider )
				{
					return collider;
				}
				else if( collider is CircleCollider && Collisions.rectToCircle( ref rect, collider.bounds.getCenter(), collider.bounds.Width * 0.5f ) )
				{
					return collider;
				}
				else if( collider is MultiCollider )
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
				else if( collider is PolygonCollider )
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
				else
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
			}

			return null;			
		}


		/// <summary>
		/// check if a collider falls within a circular area
		/// </summary>
		/// <returns>The circle.</returns>
		/// <param name="center">Center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider overlapCircle( Vector2 center, float radius, int layerMask = AllLayers )
		{
			var bounds = RectangleExt.fromFloats( center.X - radius, center.Y - radius, radius * 2f, radius * 2f );
			var potentials = _spatialHash.aabbBroadphase( ref bounds, null, layerMask );
			foreach( var collider in potentials )
			{
				if( collider is BoxCollider )
				{
					if( Collisions.rectToCircle( collider.bounds, center, radius ) )
						return collider;
				}
				else if( collider is CircleCollider )
				{
					if( Collisions.circleToCircle( center, radius, collider.bounds.getCenter(), collider.bounds.Width * 0.5f ) )
						return collider;
				}
				else if( collider is MultiCollider )
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
				//else if( collider is PolygonCollider && Collisions.polygonToCircle( collider as PolygonCollider, center, radius ) )
				{
					return collider;
				}
				//else
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
			}

			return null;
		}


		/// <summary>
		/// check if a collider overlaps a point in space
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="point">Point.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider overlapPoint( Vector2 point, int layerMask = AllLayers )
		{
			throw new NotImplementedException();
		}


		#region Broadphase methods

		/// <summary>
		/// returns a HashSet of all colliders with bounds that are intersected by collider.bounds. Note that this is a broadphase check so it
		/// only checks bounds and does not do individual Collider-to-Collider checks!
		/// </summary>
		/// <param name="bounds">Bounds.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static HashSet<Collider> boxcastBroadphase( Rectangle rect, int layerMask = AllLayers )
		{
			return _spatialHash.aabbBroadphase( ref rect, null, layerMask );
		}


		/// <summary>
		/// returns a HashSet of all colliders with bounds that are intersected by collider.bounds excluding the passed-in collider (self)
		/// </summary>
		/// <returns>The neighbors excluding self.</returns>
		/// <param name="collider">Collider.</param>
		public static HashSet<Collider> boxcastBroadphaseExcludingSelf( Collider collider, int layerMask = AllLayers )
		{
			var bounds = collider.bounds;
			return _spatialHash.aabbBroadphase( ref bounds, collider, layerMask );
		}


		/// <summary>
		/// returns a HashSet of all colliders that are intersected by bounds excluding the passed-in collider (self).
		/// this method is useful if you want to create the swept bounds on your own for other queries
		/// </summary>
		/// <returns>The excluding self.</returns>
		/// <param name="collider">Collider.</param>
		/// <param name="bounds">Bounds.</param>
		public static HashSet<Collider> boxcastBroadphaseExcludingSelf( Collider collider, ref Rectangle rect, int layerMask = AllLayers )
		{
			return _spatialHash.aabbBroadphase( ref rect, collider, layerMask );
		}


		/// <summary>
		/// returns a HashSet of all colliders that are intersected by collider.bounds expanded to incorporate deltaX/deltaY
		/// excluding the passed-in collider (self)
		/// </summary>
		/// <returns>The neighbors excluding self.</returns>
		/// <param name="collider">Collider.</param>
		public static HashSet<Collider> boxcastBroadphaseExcludingSelf( Collider collider, float deltaX, float deltaY, int layerMask = AllLayers )
		{
			var sweptBounds = collider.bounds.getSweptBroadphaseBounds( deltaX, deltaY );
			return _spatialHash.aabbBroadphase( ref sweptBounds, collider, layerMask );
		}

		#endregion

	}
}

