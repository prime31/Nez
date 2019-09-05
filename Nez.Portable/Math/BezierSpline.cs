using Microsoft.Xna.Framework;


namespace Nez.Splines
{
	/// <summary>
	/// houses a series of cubic bezier points and provides helper methods to access the bezier
	/// </summary>
	public class BezierSpline
	{
		FastList<Vector2> _points = new FastList<Vector2>();
		int _curveCount;


		/// <summary>
		/// helper that gets the bezier point index at time t. t is modified in the process to be in the range of the curve segment.
		/// </summary>
		/// <returns>The index at time.</returns>
		/// <param name="t">T.</param>
		int PointIndexAtTime(ref float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = _points.Length - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * _curveCount;
				i = (int) t;
				t -= i;
				i *= 3;
			}

			return i;
		}


		/// <summary>
		/// sets a control point taking into account if this is a shared point and adjusting appropriately if it is
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="point">Point.</param>
		public void SetControlPoint(int index, Vector2 point)
		{
			if (index % 3 == 0)
			{
				var delta = point - _points[index];
				if (index > 0)
					_points.Buffer[index - 1] += delta;

				if (index + 1 < _points.Length)
					_points.Buffer[index + 1] += delta;
			}

			_points.Buffer[index] = point;
		}


		/// <summary>
		/// gets the point on the bezier at time t
		/// </summary>
		/// <returns>The point at time.</returns>
		/// <param name="t">T.</param>
		public Vector2 GetPointAtTime(float t)
		{
			var i = PointIndexAtTime(ref t);
			return Bezier.GetPoint(_points.Buffer[i], _points.Buffer[i + 1], _points.Buffer[i + 2],
				_points.Buffer[i + 3], t);
		}


		/// <summary>
		/// gets the velocity (first derivative) of the bezier at time t
		/// </summary>
		/// <returns>The velocity at time.</returns>
		/// <param name="t">T.</param>
		public Vector2 GetVelocityAtTime(float t)
		{
			var i = PointIndexAtTime(ref t);
			return Bezier.GetFirstDerivative(_points.Buffer[i], _points.Buffer[i + 1], _points.Buffer[i + 2],
				_points.Buffer[i + 3], t);
		}


		/// <summary>
		/// gets the direction (normalized first derivative) of the bezier at time t
		/// </summary>
		/// <returns>The direction at time.</returns>
		/// <param name="t">T.</param>
		public Vector2 GetDirectionAtTime(float t)
		{
			return Vector2.Normalize(GetVelocityAtTime(t));
		}


		/// <summary>
		/// adds a curve to the bezier
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="firstControlPoint">First control point.</param>
		/// <param name="secondControlPoint">Second control point.</param>
		public void AddCurve(Vector2 start, Vector2 firstControlPoint, Vector2 secondControlPoint, Vector2 end)
		{
			// we only add the start point if this is the first curve. For all other curves the previous end should equal the start of the new curve.
			if (_points.Length == 0)
				_points.Add(start);

			_points.Add(firstControlPoint);
			_points.Add(secondControlPoint);
			_points.Add(end);

			_curveCount = (_points.Length - 1) / 3;
		}


		/// <summary>
		/// resets the bezier removing all points
		/// </summary>
		public void Reset()
		{
			_points.Clear();
		}


		/// <summary>
		/// breaks up the spline into totalSegments parts and returns all the points required to draw using lines
		/// </summary>
		/// <returns>The drawing points.</returns>
		/// <param name="totalSegments">Total segments.</param>
		public Vector2[] GetDrawingPoints(int totalSegments)
		{
			var points = new Vector2[totalSegments];
			for (var i = 0; i < totalSegments; i++)
			{
				var t = i / (float) totalSegments;
				points[i] = GetPointAtTime(t);
			}

			return points;
		}
	}
}