using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Tiled;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// basic static grid graph for use with A*. Add walls to the walls HashSet and weighted nodes to the weightedNodes HashSet. This provides
	/// a very simple grid graph for A* with just two weights: defaultWeight and weightedNodeWeight.
	/// </summary>
	public class AstarGridGraph : IAstarGraph<Point>
	{
		public List<Point> dirs = new List<Point>
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
		List<Point> _neighbors = new List<Point>( 4 );


		public AstarGridGraph( int width, int height )
		{
			_width = width;
			_height = height;
		}


		/// <summary>
		/// creates a WeightedGridGraph from a TiledTileLayer. Present tile are walls and empty tiles are passable.
		/// </summary>
		/// <param name="tiledLayer">Tiled layer.</param>
		public AstarGridGraph( TiledTileLayer tiledLayer )
		{
			_width = tiledLayer.width;
			_height = tiledLayer.height;

			for( var y = 0; y < tiledLayer.tiledMap.height; y++ )
			{
				for( var x = 0; x < tiledLayer.tiledMap.width; x++ )
				{
					if( tiledLayer.getTile( x, y ) != null )
						walls.Add( new Point( x, y ) );
				}
			}
		}


		/// <summary>
		/// ensures the node is in the bounds of the grid graph
		/// </summary>
		/// <returns><c>true</c>, if node in bounds was ised, <c>false</c> otherwise.</returns>
		/// <param name="node">Node.</param>
		bool isNodeInBounds( Point node )
		{
			return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
		}


		/// <summary>
		/// checks if the node is passable. Walls are impassable.
		/// </summary>
		/// <returns><c>true</c>, if node passable was ised, <c>false</c> otherwise.</returns>
		/// <param name="node">Node.</param>
		bool isNodePassable( Point node )
		{
			return !walls.Contains( node );
		}


		/// <summary>
		/// convenience shortcut for calling AStarPathfinder.search
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		public List<Point> search( Point start, Point goal )
		{
			return AStarPathfinder.search( this, start, goal );
		}


		#region IAstarGraph implementation

		IEnumerable<Point> IAstarGraph<Point>.getNeighbors( Point node )
		{
			_neighbors.Clear();

			foreach( var dir in dirs )
			{
				var next = new Point( node.X + dir.X, node.Y + dir.Y );
				if( isNodeInBounds( next ) && isNodePassable( next ) )
					_neighbors.Add( next );
			}

			return _neighbors;
		}


		int IAstarGraph<Point>.cost( Point from, Point to )
		{
			return weightedNodes.Contains( to ) ? weightedNodeWeight : defaultWeight;
		}


		int IAstarGraph<Point>.heuristic( Point node, Point goal )
		{
			return Math.Abs( node.X - goal.X ) + Math.Abs( node.Y - goal.Y );
		}

		#endregion

	}
}

