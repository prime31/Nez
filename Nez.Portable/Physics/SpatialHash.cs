using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Spatial
{
	public class SpatialHash
	{
		public Rectangle gridBounds = new Rectangle();


		RaycastResultParser _raycastParser;

		/// <summary>
		/// the size of each cell in the hash
		/// </summary>
		int _cellSize;

		/// <summary>
		/// 1 over the cell size. cached result due to it being used a lot.
		/// </summary>
		float _inverseCellSize;

		/// <summary>
		/// cached box used for overlap checks
		/// </summary>
		Box _overlapTestBox = new Box( 0f, 0f );

		/// <summary>
		/// cached circle used for overlap checks
		/// </summary>
		Circle _overlapTestCirce = new Circle( 0f );

		/// <summary>
		/// the Dictionary that holds all of the data
		/// </summary>
		IntIntDictionary _cellDict = new IntIntDictionary();

		/// <summary>
		/// shared HashSet used to return collision info
		/// </summary>
		HashSet<Collider> _tempHashset = new HashSet<Collider>();


		public SpatialHash( int cellSize = 100 )
		{
			_cellSize = cellSize;
			_inverseCellSize = 1f / _cellSize;
			_raycastParser = new RaycastResultParser();
		}


		/// <summary>
		/// gets the cell x,y values for a world-space x,y value
		/// </summary>
		/// <returns>The coords.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		Point cellCoords( int x, int y )
		{
			return new Point( Mathf.floorToInt( x * _inverseCellSize ), Mathf.floorToInt( y * _inverseCellSize ) );
		}


		/// <summary>
		/// gets the cell x,y values for a world-space x,y value
		/// </summary>
		/// <returns>The coords.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		Point cellCoords( float x, float y )
		{
			return new Point( Mathf.floorToInt( x * _inverseCellSize ), Mathf.floorToInt( y * _inverseCellSize ) );
		}


		/// <summary>
		/// gets the cell at the world-space x,y value. If the cell is empty and createCellIfEmpty is true a new cell will be created.
		/// </summary>
		/// <returns>The at position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="createCellIfEmpty">If set to <c>true</c> create cell if empty.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		List<Collider> cellAtPosition( int x, int y, bool createCellIfEmpty = false )
		{
			List<Collider> cell = null;
			if( !_cellDict.tryGetValue( x, y, out cell ) )
			{
				if( createCellIfEmpty )
				{
					cell = new List<Collider>();
					_cellDict.add( x, y, cell );
				}
			}

			return cell;
		}


		/// <summary>
		/// adds the object to the SpatialHash
		/// </summary>
		/// <param name="collider">Object.</param>
		public void register( Collider collider )
		{
			var bounds = collider.bounds;
			collider.registeredPhysicsBounds = bounds;
			var p1 = cellCoords( bounds.x, bounds.y );
			var p2 = cellCoords( bounds.right, bounds.bottom );

			// update our bounds to keep track of our grid size
			if( !gridBounds.Contains( p1 ) )
				RectangleExt.union( ref gridBounds, ref p1, out gridBounds );

			if( !gridBounds.Contains( p2 ) )
				RectangleExt.union( ref gridBounds, ref p2, out gridBounds );

			for( var x = p1.X; x <= p2.X; x++ )
			{
				for( var y = p1.Y; y <= p2.Y; y++ )
				{
					// we need to create the cell if there is none
					var c = cellAtPosition( x, y, true );
					c.Add( collider );
				}
			}
		}


		/// <summary>
		/// removes the object from the SpatialHash
		/// </summary>
		/// <param name="collider">Collider.</param>
		public void remove( Collider collider )
		{
			var bounds = collider.registeredPhysicsBounds;
			var p1 = cellCoords( bounds.x, bounds.y );
			var p2 = cellCoords( bounds.right, bounds.bottom );

			for( var x = p1.X; x <= p2.X; x++ )
			{
				for( var y = p1.Y; y <= p2.Y; y++ )
				{
					// the cell should always exist since this collider should be in all queryed cells
					var cell = cellAtPosition( x, y );
					Assert.isNotNull( cell, "removing Collider [{0}] from a cell that it is not present in", collider );
					if( cell != null )
						cell.Remove( collider );
				}
			}
		}


		/// <summary>
		/// removes the object from the SpatialHash using a brute force approach
		/// </summary>
		/// <param name="obj">Object.</param>
		public void removeWithBruteForce( Collider obj )
		{
			_cellDict.remove( obj );
		}


		public void clear()
		{
			_cellDict.clear();
		}


		/// <summary>
		/// debug draws the contents of the spatial hash. Note that Core.debugRenderEnabled must be true or nothing will be displayed.
		/// </summary>
		/// <param name="secondsToDisplay">Seconds to display.</param>
		/// <param name="textScale">Text scale.</param>
		public void debugDraw( float secondsToDisplay, float textScale = 1f )
		{
			for( var x = gridBounds.X; x <= gridBounds.Right; x++ )
			{
				for( var y = gridBounds.Y; y <= gridBounds.Bottom; y++ )
				{
					var cell = cellAtPosition( x, y );
					if( cell != null && cell.Count > 0 )
						debugDrawCellDetails( x, y, cell.Count, secondsToDisplay, textScale );
				}
			}
		}


		void debugDrawCellDetails( int x, int y, int cellCount, float secondsToDisplay = 0.5f, float textScale = 1f )
		{
			Debug.drawHollowRect( new Rectangle( x * _cellSize, y * _cellSize, _cellSize, _cellSize ), Color.Red, secondsToDisplay );

			if( cellCount > 0 )
			{
				var textPosition = new Vector2( (float)x * (float)_cellSize + 0.5f * _cellSize, (float)y * (float)_cellSize + 0.5f * _cellSize );
				Debug.drawText( Graphics.instance.bitmapFont, cellCount.ToString(), textPosition, Color.DarkGreen, secondsToDisplay, textScale );
			}
		}


		/// <summary>
		/// returns all the Colliders in the SpatialHash
		/// </summary>
		/// <returns>The all objects.</returns>
		public HashSet<Collider> getAllObjects()
		{
			return _cellDict.getAllObjects();
		}


		#region hash queries

		/// <summary>
		/// returns all objects in cells that the bounding box intersects
		/// </summary>
		/// <returns>The neighbors.</returns>
		/// <param name="bounds">Bounds.</param>
		/// <param name="layerMask">Layer mask.</param>
		public HashSet<Collider> aabbBroadphase( ref RectangleF bounds, Collider excludeCollider, int layerMask )
		{
			_tempHashset.Clear();

			var p1 = cellCoords( bounds.x, bounds.y );
			var p2 = cellCoords( bounds.right, bounds.bottom );

			for( var x = p1.X; x <= p2.X; x++ )
			{
				for( var y = p1.Y; y <= p2.Y; y++ )
				{
					var cell = cellAtPosition( x, y );
					if( cell == null )
						continue;

					// we have a cell. loop through and fetch all the Colliders
					for( var i = 0; i < cell.Count; i++ )
					{
						var collider = cell[i];

						// skip this collider if it is our excludeCollider or if it doesnt match our layerMask
						if( collider == excludeCollider || !Flags.isFlagSet( layerMask, collider.physicsLayer ) )
							continue;

						if( bounds.intersects( collider.bounds ) )
							_tempHashset.Add( collider );
					}
				}
			}

			return _tempHashset;
		}


		/// <summary>
		/// casts a line through the spatial hash and fills the hits array up with any colliders that the line hits
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="hits">Hits.</param>
		/// <param name="layerMask">Layer mask.</param>
		public int linecast( Vector2 start, Vector2 end, RaycastHit[] hits, int layerMask )
		{
			var ray = new Ray2D( start, end );
			_raycastParser.start( ref ray, hits, layerMask );

			// get our start/end position in the same space as our grid
			start.X *= _inverseCellSize;
			start.Y *= _inverseCellSize;
			var endCell = cellCoords( end.X, end.Y );

			// TODO: check gridBounds to ensure the ray starts/ends in the grid. watch out for end cells since they report out of bounds due to int comparison

			// what voxel are we on
			var intX = Mathf.floorToInt( start.X );
			var intY = Mathf.floorToInt( start.Y );

			// which way we go
			var stepX = Math.Sign( ray.direction.X );
			var stepY = Math.Sign( ray.direction.Y );

            // we make sure that if we're on the same line or row we don't step 
            // in the unneeded direction
			if (intX == endCell.X) stepX = 0;
			if (intY == endCell.Y) stepY = 0;

			// Calculate cell boundaries. when the step is positive, the next cell is after this one meaning we add 1.
			// If negative, cell is before this one in which case dont add to boundary
			var boundaryX = intX + ( stepX > 0 ? 1 : 0 );
			var boundaryY = intY + ( stepY > 0 ? 1 : 0 );

			// determine the value of t at which the ray crosses the first vertical voxel boundary. same for y/horizontal.
			// The minimum of these two values will indicate how much we can travel along the ray and still remain in the current voxel
			// may be infinite for near vertical/horizontal rays
			var tMaxX = ( boundaryX - start.X ) / ray.direction.X;
			var tMaxY = ( boundaryY - start.Y ) / ray.direction.Y;
			if( ray.direction.X == 0f || stepX == 0)
				tMaxX = float.PositiveInfinity;
            if( ray.direction.Y == 0f || stepY == 0 )
				tMaxY = float.PositiveInfinity;

			// how far do we have to walk before crossing a cell from a cell boundary. may be infinite for near vertical/horizontal rays
			var tDeltaX = stepX / ray.direction.X;
			var tDeltaY = stepY / ray.direction.Y;

			// start walking and returning the intersecting cells.
			var cell = cellAtPosition( intX, intY );
			//debugDrawCellDetails( intX, intY, cell != null ? cell.Count : 0 );
			if( cell != null && _raycastParser.checkRayIntersection( intX, intY, cell ) )
			{
				_raycastParser.reset();
				return _raycastParser.hitCounter;
			}

			while( intX != endCell.X || intY != endCell.Y )
			{
				if( tMaxX < tMaxY )
				{
					intX += stepX;
					tMaxX += tDeltaX;
				}
				else
				{
					intY += stepY;
					tMaxY += tDeltaY;
				}

				cell = cellAtPosition( intX, intY );
				if( cell != null && _raycastParser.checkRayIntersection( intX, intY, cell ) )
				{
					_raycastParser.reset();
					return _raycastParser.hitCounter;
				}
			}

			// make sure we are reset
			_raycastParser.reset();
			return _raycastParser.hitCounter;
		}
	

		/// <summary>
		/// gets all the colliders that fall within the specified rect
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="results">Results.</param>
		/// <param name="layerMask">Layer mask.</param>
		public int overlapRectangle( ref RectangleF rect, Collider[] results, int layerMask )
		{
			_overlapTestBox.updateBox( rect.width, rect.height );
			_overlapTestBox.position = rect.location;

			var resultCounter = 0;
			var potentials = aabbBroadphase( ref rect, null, layerMask );
			foreach( var collider in potentials )
			{
				if( collider is BoxCollider )
				{
					results[resultCounter] = collider;
					resultCounter++;
				}
				else if( collider is CircleCollider )
				{
					if( Collisions.rectToCircle( ref rect, collider.bounds.center, collider.bounds.width * 0.5f ) )
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else if( collider is PolygonCollider )
				{
					if( collider.shape.overlaps( _overlapTestBox ) )
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else
				{
					throw new NotImplementedException( "overlapRectangle against this collider type is not implemented!" );
				}

				// if our results array is full return
				if( resultCounter == results.Length )
					return resultCounter;
			}

			return resultCounter;
		}


		/// <summary>
		/// gets all the colliders that fall within the specified circle
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="circleCenter">Circle center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="results">Results.</param>
		/// <param name="layerMask">Layer mask.</param>
		public int overlapCircle( Vector2 circleCenter, float radius, Collider[] results, int layerMask )
		{
			var bounds = new RectangleF( circleCenter.X - radius, circleCenter.Y - radius, radius * 2f, radius * 2f );

			_overlapTestCirce.radius = radius;
			_overlapTestCirce.position = circleCenter;

			var resultCounter = 0;
			var potentials = aabbBroadphase( ref bounds, null, layerMask );
			foreach( var collider in potentials )
			{
				if( collider is BoxCollider )
				{
					results[resultCounter] = collider;
					resultCounter++;
				}
				else if( collider is CircleCollider )
				{
					if( collider.shape.overlaps( _overlapTestCirce ) )
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else if( collider is PolygonCollider )
				{
					if( collider.shape.overlaps( _overlapTestCirce ) )
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else
				{
					throw new NotImplementedException( "overlapCircle against this collider type is not implemented!" );
				}

				// if our results array is full return
				if( resultCounter == results.Length )
					return resultCounter;
			}

			return resultCounter;
		}

		#endregion
	}


	/// <summary>
	/// wraps a Unit32,List<Collider> Dictionary. It's main purpose is to hash the int,int x,y coordinates into a single
	/// Uint32 key which hashes perfectly resulting in an O(1) lookup.
	/// </summary>
	class IntIntDictionary
	{
		Dictionary<long,List<Collider>> _store = new Dictionary<long,List<Collider>>();


		/// <summary>
		/// computes and returns a hash key based on the x and y value. basically just packs the 2 ints into a long.
		/// </summary>
		/// <returns>The key.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		long getKey( int x, int y )
		{
			return (long)x << 32 | (long)(uint)y;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void add( int x, int y, List<Collider> list )
		{
			_store.Add( getKey( x, y ), list );
		}


		/// <summary>
		/// removes the collider from the Lists the Dictionary stores using a brute force approach
		/// </summary>
		/// <param name="obj">Object.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void remove( Collider obj )
		{
			foreach( var list in _store.Values )
			{
				if( list.Contains( obj ) )
					list.Remove( obj );
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool tryGetValue( int x, int y, out List<Collider> list )
		{
			return _store.TryGetValue( getKey( x, y ), out list );
		}


		/// <summary>
		/// gets all the Colliders currently in the dictionary
		/// </summary>
		/// <returns>The all objects.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public HashSet<Collider> getAllObjects()
		{
			var set = new HashSet<Collider>();

			foreach( var list in _store.Values )
				set.UnionWith( list );

			return set;
		}


		/// <summary>
		/// clears the backing dictionary
		/// </summary>
		public void clear()
		{
			_store.Clear();
		}

	}


	class RaycastResultParser
	{
		public int hitCounter;

		static Comparison<RaycastHit> compareRaycastHits = ( a, b ) => { return a.distance.CompareTo( b.distance ); };

		//int _cellSize;
		//Rectangle _hitTesterRect; see note in checkRayIntersection
		RaycastHit[] _hits;
		RaycastHit _tempHit;
		List<Collider> _checkedColliders = new List<Collider>();
		List<RaycastHit> _cellHits = new List<RaycastHit>();
		Ray2D _ray;
		int _layerMask;


		public void start( ref Ray2D ray, RaycastHit[] hits, int layerMask )
		{
			_ray = ray;
			_hits = hits;
			_layerMask = layerMask;
			hitCounter = 0;
		}


		/// <summary>
		/// returns true if the hits array gets filled. cell must not be null!
		/// </summary>
		/// <returns><c>true</c>, if ray intersection was checked, <c>false</c> otherwise.</returns>
		/// <param name="ray">Ray.</param>
		/// <param name="cellX">Cell x.</param>
		/// <param name="cellY">Cell y.</param>
		/// <param name="cell">Cell.</param>
		/// <param name="hits">Hits.</param>
		/// <param name="hitCounter">Hit counter.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool checkRayIntersection( int cellX, int cellY, List<Collider> cell )
		{
			float fraction;
			for( var i = 0; i < cell.Count; i++ )
			{
				var potential = cell[i];

				// manage which colliders we already processed
				if( _checkedColliders.Contains( potential ) )
					continue;
				
				_checkedColliders.Add( potential );

				// only hit triggers if we are set to do so
				if( potential.isTrigger && !Physics.raycastsHitTriggers )
					continue;

				// make sure the Collider is on the layerMask
				if( !Flags.isFlagSet( _layerMask, potential.physicsLayer ) )
					continue;

				// TODO: is rayIntersects performant enough? profile it. Collisions.rectToLine might be faster
				// TODO: if the bounds check returned more data we wouldnt have to do any more for a BoxCollider check
				// first a bounds check before doing a shape test
				var colliderBounds = potential.bounds;
				if( colliderBounds.rayIntersects( ref _ray, out fraction ) && fraction <= 1.0f )
				{
					if( potential.shape.collidesWithLine( _ray.start, _ray.end, out _tempHit ) )
					{
						// check to see if the raycast started inside the collider if we should excluded those rays
						if( !Physics.raycastsStartInColliders && potential.shape.containsPoint( _ray.start ) )
							continue;
						
						// TODO: make sure the collision point is in the current cell and if it isnt store it off for later evaluation
						// this would be for giant objects with odd shapes that bleed into adjacent cells
						//_hitTesterRect.X = cellX * _cellSize;
						//_hitTesterRect.Y = cellY * _cellSize;
						//if( !_hitTesterRect.Contains( _tempHit.point ) )

						_tempHit.collider = potential;
						_cellHits.Add( _tempHit );
					}
				}
			}

			if( _cellHits.Count == 0 )
				return false;

			// all done processing the cell. sort the results and pack the hits into the result array
			_cellHits.Sort( compareRaycastHits );
			for( var i = 0; i < _cellHits.Count; i++ )
			{
				_hits[hitCounter] = _cellHits[i];

				// increment the hit counter and if it has reached the array size limit we are done
				hitCounter++;
				if( hitCounter == _hits.Length )
					return true;
			}

			return false;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void reset()
		{
			_hits = null;
			_checkedColliders.Clear();
			_cellHits.Clear();
		}
	
	}

}
