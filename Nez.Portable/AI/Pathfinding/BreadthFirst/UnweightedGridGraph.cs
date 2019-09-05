using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Tiled;


namespace Nez.AI.Pathfinding
{
	/// <summary>
	/// basic unweighted grid graph for use with the BreadthFirstPathfinder
	/// </summary>
	public class UnweightedGridGraph : IUnweightedGraph<Point>
	{
		static readonly Point[] CARDINAL_DIRS = new[]
		{
			new Point(1, 0),
			new Point(0, -1),
			new Point(-1, 0),
			new Point(0, 1),
		};

		static readonly Point[] COMPASS_DIRS = new[]
		{
			new Point(1, 0),
			new Point(1, -1),
			new Point(0, -1),
			new Point(-1, -1),
			new Point(-1, 0),
			new Point(-1, 1),
			new Point(0, 1),
			new Point(1, 1),
		};

		public HashSet<Point> Walls = new HashSet<Point>();

		int _width, _height;
		Point[] _dirs;
		List<Point> _neighbors = new List<Point>(4);


		public UnweightedGridGraph(int width, int height, bool allowDiagonalSearch = false)
		{
			this._width = width;
			this._height = height;
			this._dirs = allowDiagonalSearch ? COMPASS_DIRS : CARDINAL_DIRS;
		}


		public UnweightedGridGraph(TiledTileLayer tiledLayer)
		{
			_width = tiledLayer.Width;
			_height = tiledLayer.Height;
			_dirs = CARDINAL_DIRS;

			for (var y = 0; y < tiledLayer.TiledMap.Height; y++)
			{
				for (var x = 0; x < tiledLayer.TiledMap.Width; x++)
				{
					if (tiledLayer.GetTile(x, y) != null)
						Walls.Add(new Point(x, y));
				}
			}
		}


		public bool IsNodeInBounds(Point node)
		{
			return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
		}


		public bool IsNodePassable(Point node)
		{
			return !Walls.Contains(node);
		}


		IEnumerable<Point> IUnweightedGraph<Point>.GetNeighbors(Point node)
		{
			_neighbors.Clear();

			foreach (var dir in _dirs)
			{
				var next = new Point(node.X + dir.X, node.Y + dir.Y);
				if (IsNodeInBounds(next) && IsNodePassable(next))
					_neighbors.Add(next);
			}

			return _neighbors;
		}


		/// <summary>
		/// convenience shortcut for clling BreadthFirstPathfinder.search
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		public List<Point> Search(Point start, Point goal)
		{
			return BreadthFirstPathfinder.Search(this, start, goal);
		}
	}
}