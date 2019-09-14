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
		public static readonly Point[] CARDINAL_DIRS = {
			new Point(1, 0),
			new Point(0, -1),
			new Point(-1, 0),
			new Point(0, 1)
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
		public HashSet<Point> WeightedNodes = new HashSet<Point>();
		public int DefaultWeight = 1;
		public int WeightedNodeWeight = 5;

		int _width, _height;
		Point[] _dirs;
		List<Point> _neighbors = new List<Point>(4);


		public WeightedGridGraph(int width, int height, bool allowDiagonalSearch = false)
		{
			_width = width;
			_height = height;
			_dirs = allowDiagonalSearch ? COMPASS_DIRS : CARDINAL_DIRS;
		}

		/// <summary>
		/// creates a WeightedGridGraph from a TiledTileLayer. Present tile are walls and empty tiles are passable.
		/// </summary>
		/// <param name="tiledLayer">Tiled layer.</param>
		public WeightedGridGraph(TmxLayer tiledLayer)
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

		/// <summary>
		/// ensures the node is in the bounds of the grid graph
		/// </summary>
		/// <returns><c>true</c>, if node in bounds was ised, <c>false</c> otherwise.</returns>
		bool IsNodeInBounds(Point node)
		{
			return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
		}

		/// <summary>
		/// checks if the node is passable. Walls are impassable.
		/// </summary>
		/// <returns><c>true</c>, if node passable was ised, <c>false</c> otherwise.</returns>
		public bool IsNodePassable(Point node) => !Walls.Contains(node);

		/// <summary>
		/// convenience shortcut for calling AStarPathfinder.search
		/// </summary>
		public List<Point> Search(Point start, Point goal) => WeightedPathfinder.Search(this, start, goal);

		#region IWeightedGraph implementation

		IEnumerable<Point> IWeightedGraph<Point>.GetNeighbors(Point node)
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

		int IWeightedGraph<Point>.Cost(Point from, Point to)
		{
			return WeightedNodes.Contains(to) ? WeightedNodeWeight : DefaultWeight;
		}

		#endregion
	}
}