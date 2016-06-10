using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Tiled;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// basic grid graph with support for one type of weighted node
	/// </summary>
	public class WeightedGridGraph : IWeightedGraph<Point>
	{
		public static readonly Point[] DIRS = new []
		{
			new Point( 1, 0 ),
			new Point( 0, -1 ),
			new Point( -1, 0 ),
			new Point( 0, 1 )
		};

		public HashSet<Point> walls = new HashSet<Point>();
		public HashSet<Point> weightedNodes = new HashSet<Point>();
		public int defaultWeight = 1;
		public int weightedNodeWeight = 5;

		int _width, _height;


		public WeightedGridGraph( int width, int height )
		{
			this._width = width;
			this._height = height;
		}


		public WeightedGridGraph( TiledTileLayer tiledLayer )
		{
			_width = tiledLayer.width;
			_height = tiledLayer.height;

			for( var y = 0; y < tiledLayer.tilemap.height; y++ )
			{
				for( var x = 0; x < tiledLayer.tilemap.width; x++ )
				{
					if( tiledLayer.getTile( x, y ) != null )
						walls.Add( new Point( x, y ) );
				}
			}
		}


		bool isNodeInBounds( Point node )
		{
			return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
		}


		bool isNodePassable( Point node )
		{
			return !walls.Contains( node );
		}


		public List<Point> search( Point start, Point goal )
		{
			return AStarPathfinder.search( this, start, goal );
		}


		#region IWeightedGraph implementation

		IEnumerable<Point> IWeightedGraph<Point>.getNeighbors( Point node )
		{
			foreach( var dir in DIRS )
			{
				var next = new Point( node.X + dir.X, node.Y + dir.Y );
				if( isNodeInBounds( next ) && isNodePassable( next ) )
					yield return next;
			}
		}


		int IWeightedGraph<Point>.cost( Point from, Point to )
		{
			return weightedNodes.Contains( to ) ? weightedNodeWeight : defaultWeight;
		}


		int IWeightedGraph<Point>.heuristic( Point node, Point goal )
		{
			return Math.Abs( node.X - goal.X ) + Math.Abs( node.Y - goal.Y );
		}

		#endregion

	}
}

