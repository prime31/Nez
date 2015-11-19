using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez
{	
	public class SpatialHash
	{
		int _cellSize;
		int _inverseCellSize;
		IntIntDictionary _cellDict = new IntIntDictionary();
		HashSet<Collider> _tempHashset = new HashSet<Collider>();


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


		public HashSet<Collider> getAllObjects()
		{
			return _cellDict.getAllObjects();
		}


		/// <summary>
		/// returns all objects in cells that the bounding box intersects
		/// </summary>
		/// <returns>The neighbors.</returns>
		/// <param name="bounds">Bounds.</param>
		public HashSet<Collider> boxcast( ref Rectangle bounds, Collider excludeCollider = null )
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

						if( collider == excludeCollider )
							continue;

						if( bounds.Intersects( collider.bounds ) )
							_tempHashset.Add( collider );
					}
				}
			}

			return _tempHashset;
		}


		public bool raycast( Vector2 start, Vector2 end )
		{
			var ray = new Ray2D( start, end - start );

			// first we get a bounding box for the ray so that we can find all the potential hits
			var maxX = Math.Max( start.X, end.X );
			var minX = Math.Min( start.X, end.X );
			var maxY = Math.Max( start.Y, end.Y );
			var minY = Math.Min( start.Y, end.Y );

			var bounds = RectangleExtension.fromFloats( minX, minY, minX + maxX, minY + maxY );
			var potentials = boxcast( ref bounds );
			foreach( var pot in potentials )
			{
				// TODO: is rayIntersects performant enough? profile it.
				float distance;
				if( pot.bounds.rayIntersects( ray, out distance ) && distance <= 1.0f )
					return true;
			}

			return false;
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
