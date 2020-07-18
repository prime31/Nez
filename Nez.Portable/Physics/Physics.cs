using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Spatial;


namespace Nez
{
	public static class Physics
	{
		static SpatialHash _spatialHash;

		/// <summary>
		/// default value for all methods that accept a layerMask
		/// </summary>
		public const int AllLayers = -1;

		/// <summary>
		/// convenience field for storing a gravity value globally 
		/// </summary>
		public static Vector2 Gravity = new Vector2(0, 300f);

		/// <summary>
		/// cell size used when reset is called and a new SpatialHash is created
		/// </summary>
		public static int SpatialHashCellSize = 100;

		/// <summary>
		/// Do raycasts detect Colliders configured as triggers?
		/// </summary>
		public static bool RaycastsHitTriggers = false;

		/// <summary>
		/// Do ray/line casts that start inside a collider detect those colliders?
		/// </summary>
		public static bool RaycastsStartInColliders = false;

		/// <summary>
		/// we keep this around to avoid allocating it every time a raycast happens
		/// </summary>
		static RaycastHit[] _hitArray = new RaycastHit[1];

		/// <summary>
		/// allocation avoidance for overlap checks and shape casts
		/// </summary>
		static Collider[] _colliderArray = new Collider[1];


		public static void Reset()
		{
			_spatialHash = new SpatialHash(SpatialHashCellSize);
			_hitArray[0].Reset();
			_colliderArray[0] = null;
		}


		/// <summary>
		/// removes all colliders from the SpatialHash
		/// </summary>
		public static void Clear()
		{
			_spatialHash.Clear();
		}


		/// <summary>
		/// debug draws the contents of the spatial hash. Note that Core.debugRenderEnabled must be true or nothing will be displayed.
		/// </summary>
		/// <param name="secondsToDisplay">Seconds to display.</param>
		internal static void DebugDraw(float secondsToDisplay)
		{
			_spatialHash.DebugDraw(secondsToDisplay, 2f);
		}


		#region Collider management

		/// <summary>
		/// gets all the Colliders managed by the SpatialHash
		/// </summary>
		/// <returns>The all colliders.</returns>
		public static IEnumerable<Collider> GetAllColliders()
		{
			return _spatialHash.GetAllObjects();
		}


		/// <summary>
		/// adds the collider to the physics system
		/// </summary>
		/// <param name="collider">Collider.</param>
		public static void AddCollider(Collider collider)
		{
			_spatialHash.Register(collider);
		}


		/// <summary>
		/// removes the collider from the physics system
		/// </summary>
		/// <returns>The collider.</returns>
		/// <param name="collider">Collider.</param>
		public static void RemoveCollider(Collider collider)
		{
			_spatialHash.Remove(collider);
		}


		/// <summary>
		/// updates the colliders position in the physics system. This essentially just removes then re-adds the Collider with its
		/// new bounds
		/// </summary>
		/// <param name="collider">Collider.</param>
		public static void UpdateCollider(Collider collider)
		{
			_spatialHash.Remove(collider);
			_spatialHash.Register(collider);
		}

		#endregion


		/// <summary>
		/// casts a line from start to end and returns the first hit of a collider that matches layerMask
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static RaycastHit Linecast(Vector2 start, Vector2 end, int layerMask = AllLayers)
		{
			// cleanse the collider before proceeding
			_hitArray[0].Reset();
			LinecastAll(start, end, _hitArray, layerMask);
			return _hitArray[0];
		}


		/// <summary>
		/// casts a line through the spatial hash and fills the hits array up with any colliders that the line hits
		/// </summary>
		/// <returns>The all.</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="hits">Hits.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static int LinecastAll(Vector2 start, Vector2 end, RaycastHit[] hits, int layerMask = AllLayers)
		{
			Insist.IsFalse(hits.Length == 0, "An empty hits array was passed in. No hits will ever be returned.");
			return _spatialHash.Linecast(start, end, hits, layerMask);
		}


		/// <summary>
		/// check if any collider falls within a rectangular area
		/// </summary>
		/// <returns>The rectangle.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider OverlapRectangle(RectangleF rect, int layerMask = AllLayers)
		{
			return OverlapRectangle(ref rect, layerMask);
		}


		/// <summary>
		/// check if any collider falls within a rectangular area
		/// </summary>
		/// <returns>The rectangle.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider OverlapRectangle(ref RectangleF rect, int layerMask = AllLayers)
		{
			_colliderArray[0] = null;
			_spatialHash.OverlapRectangle(ref rect, _colliderArray, layerMask);
			return _colliderArray[0];
		}


		/// <summary>
		/// gets all the colliders that fall within the specified rect
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="results">Results.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static int OverlapRectangleAll(ref RectangleF rect, Collider[] results, int layerMask = AllLayers)
		{
			Insist.IsFalse(results.Length == 0,
				"An empty results array was passed in. No results will ever be returned.");
			return _spatialHash.OverlapRectangle(ref rect, results, layerMask);
		}


		/// <summary>
		/// check if any collider falls within a circular area. Returns the first Collider encountered.
		/// </summary>
		/// <returns>The circle.</returns>
		/// <param name="center">Center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static Collider OverlapCircle(Vector2 center, float radius, int layerMask = AllLayers)
		{
			_colliderArray[0] = null;
			_spatialHash.OverlapCircle(center, radius, _colliderArray, layerMask);
			return _colliderArray[0];
		}


		/// <summary>
		/// gets all the colliders that fall within the specified circle
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="center">Center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="results">Results.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static int OverlapCircleAll(Vector2 center, float radius, Collider[] results, int layerMask = AllLayers)
		{
			Insist.IsFalse(results.Length == 0,
				"An empty results array was passed in. No results will ever be returned.");
			return _spatialHash.OverlapCircle(center, radius, results, layerMask);
		}


		#region Broadphase methods

		/// <summary>
		/// returns all colliders with bounds that are intersected by collider.bounds. Note that this is a broadphase check so it
		/// only checks bounds and does not do individual Collider-to-Collider checks!
		/// </summary>
		/// <param name="bounds">Bounds.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static HashSet<Collider> BoxcastBroadphase(RectangleF rect, int layerMask = AllLayers)
		{
			return _spatialHash.AabbBroadphase(ref rect, null, layerMask);
		}


		/// <summary>
		/// returns all colliders with bounds that are intersected by collider.bounds. Note that this is a broadphase check so it
		/// only checks bounds and does not do individual Collider-to-Collider checks!
		/// </summary>
		/// <param name="bounds">Bounds.</param>
		/// <param name="layerMask">Layer mask.</param>
		public static HashSet<Collider> BoxcastBroadphase(ref RectangleF rect, int layerMask = AllLayers)
		{
			return _spatialHash.AabbBroadphase(ref rect, null, layerMask);
		}


		/// <summary>
		/// returns all colliders with bounds that are intersected by collider.bounds excluding the passed-in collider (self)
		/// </summary>
		/// <returns>The neighbors excluding self.</returns>
		/// <param name="collider">Collider.</param>
		public static HashSet<Collider> BoxcastBroadphaseExcludingSelf(Collider collider, int layerMask = AllLayers)
		{
			var bounds = collider.Bounds;
			return _spatialHash.AabbBroadphase(ref bounds, collider, layerMask);
		}


		/// <summary>
		/// returns all colliders that are intersected by bounds excluding the passed-in collider (self).
		/// this method is useful if you want to create the swept bounds on your own for other queries
		/// </summary>
		/// <returns>The excluding self.</returns>
		/// <param name="collider">Collider.</param>
		/// <param name="bounds">Bounds.</param>
		public static HashSet<Collider> BoxcastBroadphaseExcludingSelf(
			Collider collider, ref RectangleF rect, int layerMask = AllLayers)
		{
			return _spatialHash.AabbBroadphase(ref rect, collider, layerMask);
		}


		/// <summary>
		/// returns all colliders that are intersected by collider.bounds expanded to incorporate deltaX/deltaY
		/// excluding the passed-in collider (self)
		/// </summary>
		/// <returns>The neighbors excluding self.</returns>
		/// <param name="collider">Collider.</param>
		public static HashSet<Collider> BoxcastBroadphaseExcludingSelf(
			Collider collider, float deltaX, float deltaY, int layerMask = AllLayers)
		{
			var colliderBounds = collider.Bounds;
			var sweptBounds = colliderBounds.GetSweptBroadphaseBounds(deltaX, deltaY);
			return _spatialHash.AabbBroadphase(ref sweptBounds, collider, layerMask);
		}

		#endregion
	}
}