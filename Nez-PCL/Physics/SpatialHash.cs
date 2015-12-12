using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Spatial
{	
	public class SpatialHash
	{
		int _cellSize;
		int _inverseCellSize;

		/// <summary>
		/// the Dictionary that holds all of the data
		/// </summary>
		IntIntDictionary _cellDict = new IntIntDictionary();

		/// <summary>
		/// shared HashSet used to return collision info
		/// </summary>
		HashSet<Collider> _tempHashset = new HashSet<Collider>();

		/// <summary>
		/// we keep this around to avoid allocating it every time a raycast happens
		/// </summary>
		RaycastHit[] _hitArray = new RaycastHit[1];


		public SpatialHash( int cellSize = 100 )
		{
			_cellSize = cellSize;
			_inverseCellSize = 1 / _cellSize;
		}


		Point cellCoords( int x, int y )
		{
			return new Point( Mathf.floorToInt( x * _inverseCellSize ), Mathf.floorToInt( y * _inverseCellSize ) );
		}


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
		/// <param name="obj">Object.</param>
		public void register( Collider obj )
		{
			var bounds = obj.bounds;
			var p1 = cellCoords( bounds.X, bounds.Y );
			var p2 = cellCoords( bounds.Right, bounds.Bottom );

			for( var x = p1.X; x <= p2.X; x++ )
			{
				for( var y = p1.Y; y <= p2.Y; y++ )
				{
					// we need to create the cell if there is none
					var c = cellAtPosition( x, y, true );
					c.Add( obj );
				}
			}
		}


		/// <summary>
		/// removes the object from the SpatialHash using the passed-in bounds to locate it
		/// </summary>
		/// <param name="obj">Object.</param>
		public void remove( Collider obj, ref Rectangle bounds )
		{
			var p1 = cellCoords( bounds.X, bounds.Y );
			var p2 = cellCoords( bounds.Right, bounds.Bottom );

			for( var x = p1.X; x <= p2.X; x++ )
			{
				for( var y = p1.Y; y <= p2.Y; y++ )
				{
					// the cell should always exist since this collider should be in all queryed cells
					cellAtPosition( x, y ).Remove( obj );
				}
			}
		}


		/// <summary>
		/// removes the object from the SpatialHash using a brute force approach
		/// </summary>
		/// <param name="obj">Object.</param>
		public void remove( Collider obj )
		{
			_cellDict.remove( obj );
		}


		public void clear()
		{
			_cellDict.clear();
		}



		/// <summary>
		/// returns all the Colliders in the SpatialHash
		/// </summary>
		/// <returns>The all objects.</returns>
		public HashSet<Collider> getAllObjects()
		{
			return _cellDict.getAllObjects();
		}


		/// <summary>
		/// returns all objects in cells that the bounding box intersects
		/// </summary>
		/// <returns>The neighbors.</returns>
		/// <param name="bounds">Bounds.</param>
		/// <param name="layerMask">Layer mask.</param>
		public HashSet<Collider> boxcastBroadphase( ref Rectangle bounds, Collider excludeCollider, int layerMask )
		{
			_tempHashset.Clear();

			var p1 = cellCoords( bounds.X, bounds.Y );
			var p2 = cellCoords( bounds.Right, bounds.Bottom );

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

						if( bounds.Intersects( collider.bounds ) )
							_tempHashset.Add( collider );
					}
				}
			}

			return _tempHashset;
		}


		/// <summary>
		/// casts a ray from start to end. Returns true if the ray hits a collider.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="distance">Distance.</param>
		public RaycastHit raycast( Vector2 start, Vector2 end, int layerMask )
		{
			// cleanse the collider before proceeding
			_hitArray[0].reset();
			raycastAll( start, end, _hitArray, layerMask );
			return _hitArray[0];
		}


		public void raycastAll( Vector2 start, Vector2 end, RaycastHit[] hits, int layerMask )
		{
			Debug.assertIsFalse( hits.Length == 0, "An empty hits array was passed in. No hits will ever be returned." );
			var hitCounter = 0;
			var ray = new Ray2D( start, end - start );

			// first we get a bounding box for the ray so that we can find all the potential hits
			var maxX = Math.Max( start.X, end.X );
			var minX = Math.Min( start.X, end.X );
			var maxY = Math.Max( start.Y, end.Y );
			var minY = Math.Min( start.Y, end.Y );

			var bounds = RectangleExt.fromFloats( minX, minY, minX + maxX, minY + maxY );
			var potentials = boxcastBroadphase( ref bounds, null, layerMask );
			float fraction;
			foreach( var pot in potentials )
			{
				// only hit triggers if we are set to do so
				if( pot.isTrigger && !Physics.raycastsHitTriggers )
					continue;

				// TODO: is rayIntersects performant enough? profile it. Collisions.rectToLine might be faster
				// TODO: this is only an AABB check. It should be defered to the collider for other shapes
				var colliderBounds = pot.bounds;
				if( RectangleExt.rayIntersects( ref colliderBounds, ray, out fraction ) && fraction <= 1.0f )
				{
					// if this is a BoxCollider we are all done. if it isnt we need to check for a more detailed collision
					if( pot is BoxCollider || pot.collidesWith( start, end ) )
					{
						float distance;
						Vector2.Distance( ref start, ref end, out distance );
						hits[hitCounter].setValues( pot, fraction, distance * fraction );

						// increment the hit counter and if it has reached the array size limit we are done
						hitCounter++;
						if( hitCounter == hits.Length )
							return;
					}
				}
			}
		}


		public Collider overlapRectangle( ref Rectangle rect, int layerMask )
		{
			var potentials = boxcastBroadphase( ref rect, null, layerMask );
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


		public Collider overlapCircle( Vector2 center, float radius, int layerMask )
		{
			var bounds = RectangleExt.fromFloats( center.X - radius, center.Y - radius, radius * 2f, radius * 2f );
			var potentials = boxcastBroadphase( ref bounds, null, layerMask );
			foreach( var collider in potentials )
			{
				if( collider is BoxCollider && Collisions.rectToCircle( collider.bounds, center, radius ) )
				{
					return collider;
				}
				else if( collider is CircleCollider && Collisions.circleToCircle( center, radius, collider.bounds.getCenter(), collider.bounds.Width * 0.5f ) )
				{
					return collider;
				}
				else if( collider is MultiCollider )
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
				else if( collider is PolygonCollider && Collisions.polygonToCircle( collider as PolygonCollider, center, radius ) )
				{
					return collider;
				}
				else
				{
					throw new NotImplementedException( "overlapCircle against this collider type are not implemented!" );
				}
			}

			return null;
		}

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
		long getKey( int x, int y )
		{
			return (long)x << 32 | (long)(uint)y;
		}


		public void add( int x, int y, List<Collider> list )
		{
			_store.Add( getKey( x, y ), list );
		}


		/// <summary>
		/// removes the collider from the Lists the Dictionary stores using a brute force approach
		/// </summary>
		/// <param name="obj">Object.</param>
		public void remove( Collider obj )
		{
			foreach( var list in _store.Values )
			{
				if( list.Contains( obj ) )
					list.Remove( obj );
			}
		}


		public bool tryGetValue( int x, int y, out List<Collider> list )
		{
			return _store.TryGetValue( getKey( x, y ), out list );
		}


		/// <summary>
		/// gets all the Colliders currently in the dictionary
		/// </summary>
		/// <returns>The all objects.</returns>
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

}
