using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// simple ear clipping triangulator. the final triangles will be present in the triangleIndices list
	/// </summary>
	public class Triangulator
	{
		/// <summary>
		/// The indexes of triangle list entries for the list of points used in the last triangulate call.
		/// </summary>
		public List<int> TriangleIndices = new List<int>();

		int[] _triPrev = new int[12];
		int[] _triNext = new int[12];


		/// <summary>
		/// Computes a triangle list that fully covers the area enclosed by the given set of points. If points are not CCW, pass false for
		/// the arePointsCCW parameter
		/// </summary>
		/// <param name="points">A list of points that defines an enclosing path.</param>
		/// <param name="count">The number of points in the path.</param>
		public void Triangulate(Vector2[] points, bool arePointsCCW = true)
		{
			var count = points.Length;

			// set up previous and next links to effectively from a double-linked vert list
			Initialize(count);

			// loop breaker for polys that are not triangulatable
			var iterations = 0;

			// start at vert 0
			var index = 0;

			// keep removing verts until just a triangle is left
			while (count > 3 && iterations < 500)
			{
				iterations++;

				// test if current vert is an ear
				var isEar = true;

				var a = points[_triPrev[index]];
				var b = points[index];
				var c = points[_triNext[index]];

				// an ear must be convex (here counterclockwise)
				if (Vector2Ext.IsTriangleCCW(a, b, c))
				{
					// loop over all verts not part of the tentative ear
					var k = _triNext[_triNext[index]];
					do
					{
						// if vert k is inside the ear triangle, then this is not an ear
						if (TestPointTriangle(points[k], a, b, c))
						{
							isEar = false;
							break;
						}

						k = _triNext[k];
					} while (k != _triPrev[index]);
				}
				else
				{
					// the ear triangle is clockwise so points[i] is not an ear
					isEar = false;
				}

				// if current vert is an ear, delete it and visit the previous vert
				if (isEar)
				{
					// triangle is an ear
					TriangleIndices.Add(_triPrev[index]);
					TriangleIndices.Add(index);
					TriangleIndices.Add(_triNext[index]);

					// delete vert by redirecting next and previous links of neighboring verts past it
					// decrement vertext count
					_triNext[_triPrev[index]] = _triNext[index];
					_triPrev[_triNext[index]] = _triPrev[index];
					count--;

					// visit the previous vert next
					index = _triPrev[index];
				}
				else
				{
					// current vert is not an ear. visit the next vert
					index = _triNext[index];
				}
			}

			// output the final triangle
			TriangleIndices.Add(_triPrev[index]);
			TriangleIndices.Add(index);
			TriangleIndices.Add(_triNext[index]);

			if (!arePointsCCW)
				TriangleIndices.Reverse();
		}


		void Initialize(int count)
		{
			TriangleIndices.Clear();

			if (_triNext.Length < count)
				Array.Resize(ref _triNext, Math.Max(_triNext.Length * 2, count));
			if (_triPrev.Length < count)
				Array.Resize(ref _triPrev, Math.Max(_triPrev.Length * 2, count));

			for (var i = 0; i < count; i++)
			{
				_triPrev[i] = i - 1;
				_triNext[i] = i + 1;
			}

			_triPrev[0] = count - 1;
			_triNext[count - 1] = 0;
		}


		public static bool TestPointTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
		{
			// if point to the right of AB then outside triangle
			if (Vector2Ext.Cross(point - a, b - a) < 0f)
				return false;

			// if point to the right of BC then outside of triangle
			if (Vector2Ext.Cross(point - b, c - b) < 0f)
				return false;

			// if point to the right of ca then outside of triangle
			if (Vector2Ext.Cross(point - c, a - c) < 0f)
				return false;

			// point is in or on triangle
			return true;
		}
	}
}