using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	public class Physics
	{
		SpatialHash _spatialHash;

		
		public Physics( int cellSize = 100 )
		{
			_spatialHash = new SpatialHash( cellSize );
		}


		/// <summary>
		/// adds the collider to the physics system
		/// </summary>
		/// <param name="collider">Collider.</param>
		public void addCollider( Collider collider )
		{
			_spatialHash.register( collider );
		}


		/// <summary>
		/// revmoes the collider from the physics system. uses brute force if useColliderBoundsForRemovalLookup is false
		/// </summary>
		/// <param name="collider">Collider.</param>
		/// <param name="useColliderBoundsForRemovalLookup">If set to <c>true</c> use collider bounds for removal lookup.</param>
		public void removeCollider( Collider collider, bool useColliderBoundsForRemovalLookup )
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
		public void removeCollider( Collider collider, ref Rectangle bounds )
		{
			_spatialHash.remove( collider, ref bounds );
		}


		/// <summary>
		/// updates the colliders position in the physics system. preUpdateColliderBounds should be the bounds of the collider before it
		/// was changed
		/// </summary>
		/// <param name="collider">Collider.</param>
		/// <param name="colliderBounds">Collider bounds.</param>
		public void updateCollider( Collider collider, ref Rectangle preUpdateColliderBounds )
		{
			_spatialHash.remove( collider, ref preUpdateColliderBounds );
			_spatialHash.register( collider );
		}


		// TODO: all boxcast methods should sort nearest-to-furthest
		public HashSet<Collider> boxcast( Rectangle bounds )
		{
			return _spatialHash.boxcast( ref bounds );
		}


		/// <summary>
		/// returns a HashSet of all colliders that are intersected by collider.bounds excluding the passed-in collider (self)
		/// </summary>
		/// <returns>The neighbors excluding self.</returns>
		/// <param name="collider">Collider.</param>
		public HashSet<Collider> boxcastExcludingSelf( Collider collider )
		{
			var bounds = collider.bounds;
			return _spatialHash.boxcast( ref bounds, collider );
		}


		/// <summary>
		/// returns a HashSet of all colliders that are intersected by bounds excluding the passed-in collider (self).
		/// this method is useful if you want to create the swept bounds on your own for other queries
		/// </summary>
		/// <returns>The excluding self.</returns>
		/// <param name="collider">Collider.</param>
		/// <param name="bounds">Bounds.</param>
		public HashSet<Collider> boxcastExcludingSelf( Collider collider, ref Rectangle bounds )
		{
			return _spatialHash.boxcast( ref bounds, collider );
		}


		/// <summary>
		/// returns a HashSet of all colliders that are intersected by collider.bounds expanded to incorporate deltaX/deltaY
		/// excluding the passed-in collider (self)
		/// </summary>
		/// <returns>The neighbors excluding self.</returns>
		/// <param name="collider">Collider.</param>
		public HashSet<Collider> boxcastExcludingSelf( Collider collider, float deltaX, float deltaY )
		{
			var sweptBounds = collider.bounds.getSweptBroadphaseBounds( deltaX, deltaY );
			return _spatialHash.boxcast( ref sweptBounds, collider );
		}


		public bool raycast( Vector2 start, Vector2 end )
		{
			return _spatialHash.raycast( start, end );
		}

	}
}

