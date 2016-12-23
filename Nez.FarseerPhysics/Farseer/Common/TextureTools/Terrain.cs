using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Common.TextureTools
{
	/// <summary>
	/// Simple class to maintain a terrain. It can keep track
	/// </summary>
	public class Terrain
	{
		#region Properties/Fields

		/// <summary>
		/// World to manage terrain in.
		/// </summary>
		public World world;

		/// <summary>
		/// Center of terrain in world units.
		/// </summary>
		public Vector2 center;

		/// <summary>
		/// Width of terrain in world units.
		/// </summary>
		public float width;

		/// <summary>
		/// Height of terrain in world units.
		/// </summary>
		public float height;

		/// <summary>
		/// Points per each world unit used to define the terrain in the point cloud.
		/// </summary>
		public int pointsPerUnit;

		/// <summary>
		/// Points per cell.
		/// </summary>
		public int cellSize;

		/// <summary>
		/// Points per sub cell.
		/// </summary>
		public int subCellSize;

		/// <summary>
		/// Number of iterations to perform in the Marching Squares algorithm.
		/// Note: More then 3 has almost no effect on quality.
		/// </summary>
		public int iterations = 2;

		/// <summary>
		/// Decomposer to use when regenerating terrain. Can be changed on the fly without consequence.
		/// Note: Some decomposerers are unstable.
		/// </summary>
		public TriangulationAlgorithm decomposer;

		/// <summary>
		/// Point cloud defining the terrain.
		/// </summary>
		sbyte[,] _terrainMap;

		/// <summary>
		/// Generated bodies.
		/// </summary>
		List<Body>[,] _bodyMap;

		float _localWidth;
		float _localHeight;
		int _xnum;
		int _ynum;
		AABB _dirtyArea;
		Vector2 _topLeft;

		#endregion


		/// <summary>
		/// Creates a new terrain.
		/// </summary>
		/// <param name="world">The World</param>
		/// <param name="area">The area of the terrain.</param>
		public Terrain( World world, AABB area )
		{
			this.world = world;
			width = area.width;
			height = area.height;
			center = area.center;
		}

		/// <summary>
		/// Creates a new terrain
		/// </summary>
		/// <param name="world">The World</param>
		/// <param name="position">The position (center) of the terrain.</param>
		/// <param name="width">The width of the terrain.</param>
		/// <param name="height">The height of the terrain.</param>
		public Terrain( World world, Vector2 position, float width, float height )
		{
			this.world = world;
			this.width = width;
			this.height = height;
			center = position;
		}

		/// <summary>
		/// Initialize the terrain for use.
		/// </summary>
		public void initialize()
		{
			// find top left of terrain in world space
			_topLeft = new Vector2( center.X - ( width * 0.5f ), center.Y - ( -height * 0.5f ) );

			// convert the terrains size to a point cloud size
			_localWidth = width * pointsPerUnit;
			_localHeight = height * pointsPerUnit;

			_terrainMap = new sbyte[(int)_localWidth + 1, (int)_localHeight + 1];

			for( int x = 0; x < _localWidth; x++ )
			{
				for( int y = 0; y < _localHeight; y++ )
				{
					_terrainMap[x, y] = 1;
				}
			}

			_xnum = (int)( _localWidth / cellSize );
			_ynum = (int)( _localHeight / cellSize );
			_bodyMap = new List<Body>[_xnum, _ynum];

			// make sure to mark the dirty area to an infinitely small box
			_dirtyArea = new AABB( new Vector2( float.MaxValue, float.MaxValue ), new Vector2( float.MinValue, float.MinValue ) );
		}

		/// <summary>
		/// Apply the specified texture data to the terrain.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		public void applyData( sbyte[,] data, Vector2 offset = default( Vector2 ) )
		{
			for( int x = 0; x < data.GetUpperBound( 0 ); x++ )
			{
				for( int y = 0; y < data.GetUpperBound( 1 ); y++ )
				{
					if( x + offset.X >= 0 && x + offset.X < _localWidth && y + offset.Y >= 0 && y + offset.Y < _localHeight )
					{
						_terrainMap[(int)( x + offset.X ), (int)( y + offset.Y )] = data[x, y];
					}
				}
			}

			removeOldData( 0, _xnum, 0, _ynum );
		}

		/// <summary>
		/// Modify a single point in the terrain.
		/// </summary>
		/// <param name="location">World location to modify. Automatically clipped.</param>
		/// <param name="value">-1 = inside terrain, 1 = outside terrain</param>
		public void modifyTerrain( Vector2 location, sbyte value )
		{
			// find local position
			// make position local to map space
			var p = location - _topLeft;

			// find map position for each axis
			p.X = p.X * _localWidth / width;
			p.Y = p.Y * -_localHeight / height;

			if( p.X >= 0 && p.X < _localWidth && p.Y >= 0 && p.Y < _localHeight )
			{
				_terrainMap[(int)p.X, (int)p.Y] = value;

				// expand dirty area
				if( p.X < _dirtyArea.lowerBound.X ) _dirtyArea.lowerBound.X = p.X;
				if( p.X > _dirtyArea.upperBound.X ) _dirtyArea.upperBound.X = p.X;

				if( p.Y < _dirtyArea.lowerBound.Y ) _dirtyArea.lowerBound.Y = p.Y;
				if( p.Y > _dirtyArea.upperBound.Y ) _dirtyArea.upperBound.Y = p.Y;
			}
		}

		/// <summary>
		/// Regenerate the terrain.
		/// </summary>
		public void regenerateTerrain()
		{
			//iterate effected cells
			int xStart = (int)( _dirtyArea.lowerBound.X / cellSize );
			if( xStart < 0 ) xStart = 0;

			int xEnd = (int)( _dirtyArea.upperBound.X / cellSize ) + 1;
			if( xEnd > _xnum ) xEnd = _xnum;

			int yStart = (int)( _dirtyArea.lowerBound.Y / cellSize );
			if( yStart < 0 ) yStart = 0;

			int yEnd = (int)( _dirtyArea.upperBound.Y / cellSize ) + 1;
			if( yEnd > _ynum ) yEnd = _ynum;

			removeOldData( xStart, xEnd, yStart, yEnd );

			_dirtyArea = new AABB( new Vector2( float.MaxValue, float.MaxValue ), new Vector2( float.MinValue, float.MinValue ) );
		}

		void removeOldData( int xStart, int xEnd, int yStart, int yEnd )
		{
			for( int x = xStart; x < xEnd; x++ )
			{
				for( int y = yStart; y < yEnd; y++ )
				{
					//remove old terrain object at grid cell
					if( _bodyMap[x, y] != null )
					{
						for( int i = 0; i < _bodyMap[x, y].Count; i++ )
						{
							world.removeBody( _bodyMap[x, y][i] );
						}
					}

					_bodyMap[x, y] = null;

					//generate new one
					generateTerrain( x, y );
				}
			}
		}

		void generateTerrain( int gx, int gy )
		{
			float ax = gx * cellSize;
			float ay = gy * cellSize;

			var polys = MarchingSquares.detectSquares( new AABB( new Vector2( ax, ay ), new Vector2( ax + cellSize, ay + cellSize ) ), subCellSize, subCellSize, _terrainMap, iterations, true );
			if( polys.Count == 0 ) return;

			_bodyMap[gx, gy] = new List<Body>();

			// create the scale vector
			var scale = new Vector2( 1f / pointsPerUnit, 1f / -pointsPerUnit );

			// create physics object for this grid cell
			foreach( Vertices item in polys )
			{
				// does this need to be negative?
				item.scale( ref scale );
				item.translate( ref _topLeft );
				var simplified = SimplifyTools.collinearSimplify( item );

				var decompPolys = Triangulate.convexPartition( simplified, decomposer );
				foreach( Vertices poly in decompPolys )
				{
					if( poly.Count > 2 )
						_bodyMap[gx, gy].Add( BodyFactory.createPolygon( world, poly, 1 ) );
				}
			}
		}
	
	}
}