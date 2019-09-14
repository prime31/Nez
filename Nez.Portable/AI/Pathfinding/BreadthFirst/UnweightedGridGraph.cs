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
		static readonly Point[] CARDINAL_DIRS = {
			new Point(1, 0),
			new Point(0, -1),
			new Point(-1, 0),
			new Point(0, 1),
		};

		static readonly Point[] COMPASS_DIRS = {
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
			_width = width;
			_height = height;
			_dirs = allowDiagonalSearch ? COMPASS_DIRS : CARDINAL_DIRS;
		}

		public UnweightedGridGraph(TmxLayer tiledLayer)
		{
			_width = tiledLayer.Width;
			_height = tiledLayer.Height;
			_dirs = CARDINAL_DIRS;

			for (var y = 0; y < tiledLayer.Map.Height; y++)
			{
				for (var x = 0; x < tiledLayer.Map.Width; x++)
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

		public bool IsNodePassable(Point node) => !Walls.Contains(node);

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
		/// convenience shortcut for calling BreadthFirstPathfinder.search
		/// </summary>
		public List<Point> Search(Point start, Point goal) => BreadthFirstPathfinder.Search(this, start, goal);
	}
}