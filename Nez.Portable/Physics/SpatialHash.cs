using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez.Spatial
{
	public class SpatialHash
	{
		public Rectangle GridBounds = new Rectangle();


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
		Box _overlapTestBox = new Box(0f, 0f);

		/// <summary>
		/// cached circle used for overlap checks
		/// </summary>
		Circle _overlapTestCirce = new Circle(0f);

		/// <summary>
		/// the Dictionary that holds all of the data
		/// </summary>
		IntIntDictionary _cellDict = new IntIntDictionary();

		/// <summary>
		/// shared HashSet used to return collision info
		/// </summary>
		HashSet<Collider> _tempHashset = new HashSet<Collider>();


		public SpatialHash(int cellSize = 100)
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		Point CellCoords(int x, int y)
		{
			return new Point(Mathf.FloorToInt(x * _inverseCellSize), Mathf.FloorToInt(y * _inverseCellSize));
		}


		/// <summary>
		/// gets the cell x,y values for a world-space x,y value
		/// </summary>
		/// <returns>The coords.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		Point CellCoords(float x, float y)
		{
			return new Point(Mathf.FloorToInt(x * _inverseCellSize), Mathf.FloorToInt(y * _inverseCellSize));
		}


		/// <summary>
		/// gets the cell at the world-space x,y value. If the cell is empty and createCellIfEmpty is true a new cell will be created.
		/// </summary>
		/// <returns>The at position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="createCellIfEmpty">If set to <c>true</c> create cell if empty.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		List<Collider> CellAtPosition(int x, int y, bool createCellIfEmpty = false)
		{
			List<Collider> cell = null;
			if (!_cellDict.TryGetValue(x, y, out cell))
			{
				if (createCellIfEmpty)
				{
					cell = new List<Collider>();
					_cellDict.Add(x, y, cell);
				}
			}

			return cell;
		}


		/// <summary>
		/// adds the object to the SpatialHash
		/// </summary>
		/// <param name="collider">Object.</param>
		public void Register(Collider collider)
		{
			var bounds = collider.Bounds;
			collider.registeredPhysicsBounds = bounds;
			var p1 = CellCoords(bounds.X, bounds.Y);
			var p2 = CellCoords(bounds.Right, bounds.Bottom);

			// update our bounds to keep track of our grid size
			if (!GridBounds.Contains(p1))
				RectangleExt.Union(ref GridBounds, ref p1, out GridBounds);

			if (!GridBounds.Contains(p2))
				RectangleExt.Union(ref GridBounds, ref p2, out GridBounds);

			for (var x = p1.X; x <= p2.X; x++)
			{
				for (var y = p1.Y; y <= p2.Y; y++)
				{
					// we need to create the cell if there is none
					var c = CellAtPosition(x, y, true);
					c.Add(collider);
				}
			}
		}


		/// <summary>
		/// removes the object from the SpatialHash
		/// </summary>
		/// <param name="collider">Collider.</param>
		public void Remove(Collider collider)
		{
			var bounds = collider.registeredPhysicsBounds;
			var p1 = CellCoords(bounds.X, bounds.Y);
			var p2 = CellCoords(bounds.Right, bounds.Bottom);

			for (var x = p1.X; x <= p2.X; x++)
			{
				for (var y = p1.Y; y <= p2.Y; y++)
				{
					// the cell should always exist since this collider should be in all queryed cells
					var cell = CellAtPosition(x, y);
					Insist.IsNotNull(cell, "removing Collider [{0}] from a cell that it is not present in", collider);
					if (cell != null)
						cell.Remove(collider);
				}
			}
		}


		/// <summary>
		/// removes the object from the SpatialHash using a brute force approach
		/// </summary>
		/// <param name="obj">Object.</param>
		public void RemoveWithBruteForce(Collider obj)
		{
			_cellDict.Remove(obj);
		}


		public void Clear()
		{
			_cellDict.Clear();
		}


		/// <summary>
		/// debug draws the contents of the spatial hash. Note that Core.debugRenderEnabled must be true or nothing will be displayed.
		/// </summary>
		/// <param name="secondsToDisplay">Seconds to display.</param>
		/// <param name="textScale">Text scale.</param>
		public void DebugDraw(float secondsToDisplay, float textScale = 1f)
		{
			for (var x = GridBounds.X; x <= GridBounds.Right; x++)
			{
				for (var y = GridBounds.Y; y <= GridBounds.Bottom; y++)
				{
					var cell = CellAtPosition(x, y);
					if (cell != null && cell.Count > 0)
						DebugDrawCellDetails(x, y, cell.Count, secondsToDisplay, textScale);
				}
			}
		}


		void DebugDrawCellDetails(int x, int y, int cellCount, float secondsToDisplay = 0.5f, float textScale = 1f)
		{
			Debug.DrawHollowRect(new Rectangle(x * _cellSize, y * _cellSize, _cellSize, _cellSize), Color.Red,
				secondsToDisplay);

			if (cellCount > 0)
			{
				var textPosition = new Vector2((float) x * (float) _cellSize + 0.5f * _cellSize,
					(float) y * (float) _cellSize + 0.5f * _cellSize);
				Debug.DrawText(Graphics.Instance.BitmapFont, cellCount.ToString(), textPosition, Color.DarkGreen,
					secondsToDisplay, textScale);
			}
		}


		/// <summary>
		/// returns all the Colliders in the SpatialHash
		/// </summary>
		/// <returns>The all objects.</returns>
		public HashSet<Collider> GetAllObjects()
		{
			return _cellDict.GetAllObjects();
		}


		#region hash queries

		/// <summary>
		/// returns all objects in cells that the bounding box intersects
		/// </summary>
		/// <returns>The neighbors.</returns>
		/// <param name="bounds">Bounds.</param>
		/// <param name="layerMask">Layer mask.</param>
		public HashSet<Collider> AabbBroadphase(ref RectangleF bounds, Collider excludeCollider, int layerMask)
		{
			_tempHashset.Clear();

			var p1 = CellCoords(bounds.X, bounds.Y);
			var p2 = CellCoords(bounds.Right, bounds.Bottom);

			for (var x = p1.X; x <= p2.X; x++)
			{
				for (var y = p1.Y; y <= p2.Y; y++)
				{
					var cell = CellAtPosition(x, y);
					if (cell == null)
						continue;

					// we have a cell. loop through and fetch all the Colliders
					for (var i = 0; i < cell.Count; i++)
					{
						var collider = cell[i];

						// skip this collider if it is our excludeCollider or if it doesnt match our layerMask
						if (collider == excludeCollider || !Flags.IsFlagSet(layerMask, collider.PhysicsLayer))
							continue;

						if (bounds.Intersects(collider.Bounds))
							_tempHashset.Add(collider);
					}
				}
			}

			return _tempHashset;
		}


		/// <summary>
		/// casts a line through the spatial hash and fills the hits array up with any colliders that the line hits.
		/// https://github.com/francisengelmann/fast_voxel_traversal/blob/master/main.cpp
		/// http://www.cse.yorku.ca/~amana/research/grid.pdf
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="hits">Hits.</param>
		/// <param name="layerMask">Layer mask.</param>
		public int Linecast(Vector2 start, Vector2 end, RaycastHit[] hits, int layerMask)
		{
			var ray = new Ray2D(start, end);
			_raycastParser.Start(ref ray, hits, layerMask);

			// get our start/end position in the same space as our grid
			var currentCell = CellCoords(start.X, start.Y);
			var lastCell = CellCoords(end.X, end.Y);

			// what direction are we incrementing the cell checks?
			var stepX = Math.Sign(ray.Direction.X);
			var stepY = Math.Sign(ray.Direction.Y);

			// we make sure that if we're on the same line or row we don't step in the unneeded direction
			if (currentCell.X == lastCell.X) stepX = 0;
			if (currentCell.Y == lastCell.Y) stepY = 0;

			// Calculate cell boundaries. when the step is positive, the next cell is after this one meaning we add 1.
			// If negative, cell is before this one in which case dont add to boundary
			var xStep = stepX < 0 ? 0f : (float) stepX;
			var yStep = stepY < 0 ? 0f : (float) stepY;
			var nextBoundaryX = ((float) currentCell.X + xStep) * _cellSize;
			var nextBoundaryY = ((float) currentCell.Y + yStep) * _cellSize;

			// determine the value of t at which the ray crosses the first vertical voxel boundary. same for y/horizontal.
			// The minimum of these two values will indicate how much we can travel along the ray and still remain in the current voxel
			// may be infinite for near vertical/horizontal rays
			var tMaxX = ray.Direction.X != 0 ? (nextBoundaryX - ray.Start.X) / ray.Direction.X : float.MaxValue;
			var tMaxY = ray.Direction.Y != 0 ? (nextBoundaryY - ray.Start.Y) / ray.Direction.Y : float.MaxValue;

			// how far do we have to walk before crossing a cell from a cell boundary. may be infinite for near vertical/horizontal rays
			var tDeltaX = ray.Direction.X != 0 ? _cellSize / (ray.Direction.X * stepX) : float.MaxValue;
			var tDeltaY = ray.Direction.Y != 0 ? _cellSize / (ray.Direction.Y * stepY) : float.MaxValue;

			// start walking and returning the intersecting cells.
			var cell = CellAtPosition(currentCell.X, currentCell.Y);

			// Debug.log( $"cell: {currentCell.X}, {currentCell.Y}" );
			// debugDrawCellDetails( intX, intY, cell != null ? cell.Count : 10 );
			if (cell != null && _raycastParser.CheckRayIntersection(currentCell.X, currentCell.Y, cell))
			{
				_raycastParser.Reset();
				return _raycastParser.HitCounter;
			}

			while (currentCell.X != lastCell.X || currentCell.Y != lastCell.Y)
			{
				if (tMaxX < tMaxY)
				{
					// HACK: ensures we never overshoot our values
					currentCell.X = (int) Mathf.Approach(currentCell.X, lastCell.X, Math.Abs(stepX));

					// currentCell.X += stepX;
					tMaxX += tDeltaX;
				}
				else
				{
					currentCell.Y = (int) Mathf.Approach(currentCell.Y, lastCell.Y, Math.Abs(stepY));

					// currentCell.Y += stepY;
					tMaxY += tDeltaY;
				}

				// Debug.log( $"cell: {currentCell.X}, {currentCell.Y}" );
				cell = CellAtPosition(currentCell.X, currentCell.Y);
				if (cell != null && _raycastParser.CheckRayIntersection(currentCell.X, currentCell.Y, cell))
				{
					_raycastParser.Reset();
					return _raycastParser.HitCounter;
				}
			}

			// make sure we are reset
			_raycastParser.Reset();
			return _raycastParser.HitCounter;
		}


		/// <summary>
		/// gets all the colliders that fall within the specified rect
		/// </summary>
		/// <returns>the number of Colliders returned</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="results">Results.</param>
		/// <param name="layerMask">Layer mask.</param>
		public int OverlapRectangle(ref RectangleF rect, Collider[] results, int layerMask)
		{
			_overlapTestBox.UpdateBox(rect.Width, rect.Height);
			_overlapTestBox.Position = rect.Location;

			var resultCounter = 0;
			var potentials = AabbBroadphase(ref rect, null, layerMask);
			foreach (var collider in potentials)
			{
				if (collider is BoxCollider)
				{
					results[resultCounter] = collider;
					resultCounter++;
				}
				else if (collider is CircleCollider)
				{
					if (Collisions.RectToCircle(ref rect, collider.Bounds.Center, collider.Bounds.Width * 0.5f))
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else if (collider is PolygonCollider)
				{
					if (collider.Shape.Overlaps(_overlapTestBox))
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else
				{
					throw new NotImplementedException(
						"overlapRectangle against this collider type is not implemented!");
				}

				// if our results array is full return
				if (resultCounter == results.Length)
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
		public int OverlapCircle(Vector2 circleCenter, float radius, Collider[] results, int layerMask)
		{
			var bounds = new RectangleF(circleCenter.X - radius, circleCenter.Y - radius, radius * 2f, radius * 2f);

			_overlapTestCirce.Radius = radius;
			_overlapTestCirce.Position = circleCenter;

			var resultCounter = 0;
			var potentials = AabbBroadphase(ref bounds, null, layerMask);
			foreach (var collider in potentials)
			{
				if (collider is BoxCollider)
				{
					if (collider.Shape.Overlaps(_overlapTestCirce))
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else if (collider is CircleCollider)
				{
					if (collider.Shape.Overlaps(_overlapTestCirce))
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else if (collider is PolygonCollider)
				{
					if (collider.Shape.Overlaps(_overlapTestCirce))
					{
						results[resultCounter] = collider;
						resultCounter++;
					}
				}
				else
				{
					throw new NotImplementedException("overlapCircle against this collider type is not implemented!");
				}

				// if our results array is full return
				if (resultCounter == results.Length)
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
		Dictionary<long, List<Collider>> _store = new Dictionary<long, List<Collider>>();


		/// <summary>
		/// computes and returns a hash key based on the x and y value. basically just packs the 2 ints into a long.
		/// </summary>
		/// <returns>The key.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long GetKey(int x, int y)
		{
			return unchecked((long) x << 32 | (uint) y);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(int x, int y, List<Collider> list)
		{
			_store.Add(GetKey(x, y), list);
		}


		/// <summary>
		/// removes the collider from the Lists the Dictionary stores using a brute force approach
		/// </summary>
		/// <param name="obj">Object.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(Collider obj)
		{
			foreach (var list in _store.Values)
			{
				if (list.Contains(obj))
					list.Remove(obj);
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(int x, int y, out List<Collider> list)
		{
			return _store.TryGetValue(GetKey(x, y), out list);
		}


		/// <summary>
		/// gets all the Colliders currently in the dictionary
		/// </summary>
		/// <returns>The all objects.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSet<Collider> GetAllObjects()
		{
			var set = new HashSet<Collider>();

			foreach (var list in _store.Values)
				set.UnionWith(list);

			return set;
		}


		/// <summary>
		/// clears the backing dictionary
		/// </summary>
		public void Clear()
		{
			_store.Clear();
		}
	}


	class RaycastResultParser
	{
		public int HitCounter;

		static Comparison<RaycastHit> compareRaycastHits = (a, b) => { return a.Distance.CompareTo(b.Distance); };

		//int _cellSize;
		//Rectangle _hitTesterRect; see note in checkRayIntersection
		RaycastHit[] _hits;
		RaycastHit _tempHit;
		List<Collider> _checkedColliders = new List<Collider>();
		List<RaycastHit> _cellHits = new List<RaycastHit>();
		Ray2D _ray;
		int _layerMask;


		public void Start(ref Ray2D ray, RaycastHit[] hits, int layerMask)
		{
			_ray = ray;
			_hits = hits;
			_layerMask = layerMask;
			HitCounter = 0;
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CheckRayIntersection(int cellX, int cellY, List<Collider> cell)
		{
			float fraction;
			for (var i = 0; i < cell.Count; i++)
			{
				var potential = cell[i];

				// manage which colliders we already processed
				if (_checkedColliders.Contains(potential))
					continue;

				_checkedColliders.Add(potential);

				// only hit triggers if we are set to do so
				if (potential.IsTrigger && !Physics.RaycastsHitTriggers)
					continue;

				// make sure the Collider is on the layerMask
				if (!Flags.IsFlagSet(_layerMask, potential.PhysicsLayer))
					continue;

				// TODO: is rayIntersects performant enough? profile it. Collisions.rectToLine might be faster
				// TODO: if the bounds check returned more data we wouldnt have to do any more for a BoxCollider check
				// first a bounds check before doing a shape test
				var colliderBounds = potential.Bounds;
				if (colliderBounds.RayIntersects(ref _ray, out fraction) && fraction <= 1.0f)
				{
					if (potential.Shape.CollidesWithLine(_ray.Start, _ray.End, out _tempHit))
					{
						// check to see if the raycast started inside the collider if we should excluded those rays
						if (!Physics.RaycastsStartInColliders && potential.Shape.ContainsPoint(_ray.Start))
							continue;

						// TODO: make sure the collision point is in the current cell and if it isnt store it off for later evaluation
						// this would be for giant objects with odd shapes that bleed into adjacent cells
						//_hitTesterRect.X = cellX * _cellSize;
						//_hitTesterRect.Y = cellY * _cellSize;
						//if( !_hitTesterRect.Contains( _tempHit.point ) )

						_tempHit.Collider = potential;
						_cellHits.Add(_tempHit);
					}
				}
			}

			if (_cellHits.Count == 0)
				return false;

			// all done processing the cell. sort the results and pack the hits into the result array
			_cellHits.Sort(compareRaycastHits);
			for (var i = 0; i < _cellHits.Count; i++)
			{
				_hits[HitCounter] = _cellHits[i];

				// increment the hit counter and if it has reached the array size limit we are done
				HitCounter++;
				if (HitCounter == _hits.Length)
					return true;
			}

			return false;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_hits = null;
			_checkedColliders.Clear();
			_cellHits.Clear();
		}
	}
}