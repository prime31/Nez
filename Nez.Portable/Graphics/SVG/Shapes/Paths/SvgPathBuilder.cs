using System.Collections.Generic;
using Microsoft.Xna.Framework;


// IMPORTANT NOTE! THIS CLASS IS NOT COMPILED INTO THE NEZ PCL! YOU MUST ADD THIS CLASS MANUALLY TO YOUR MAIN PROJECT TO USE IT.


namespace Nez.Svg
{
	/// <summary>
	/// helper class used to parse paths and also fetch the drawing points from a series of SvgPathSegments
	/// </summary>
	public class SvgPathBuilder : ISvgPathBuilder
	{
		/// <summary>
		/// helper to convert a Vector2 into a Point
		/// </summary>
		/// <returns>The draw point.</returns>
		/// <param name="vec">Vec.</param>
		static System.Drawing.Point ToDrawPoint(Vector2 vec)
		{
			return new System.Drawing.Point((int) vec.X, (int) vec.Y);
		}


		/// <summary>
		/// takes in a parsed path and returns a list of points that can be used to draw the path
		/// </summary>
		/// <returns>The drawing points.</returns>
		/// <param name="segments">Segments.</param>
		public Vector2[] GetDrawingPoints(List<SvgPathSegment> segments, float flatness = 3)
		{
			var path = new System.Drawing.Drawing2D.GraphicsPath();
			for (var j = 0; j < segments.Count; j++)
			{
				var segment = segments[j];
				if (segment is SvgMoveToSegment)
				{
					path.StartFigure();
				}
				else if (segment is SvgCubicCurveSegment)
				{
					var cubicSegment = segment as SvgCubicCurveSegment;
					path.AddBezier(ToDrawPoint(segment.Start), ToDrawPoint(cubicSegment.FirstCtrlPoint),
						ToDrawPoint(cubicSegment.SecondCtrlPoint), ToDrawPoint(segment.End));
				}
				else if (segment is SvgClosePathSegment)
				{
					// important for custom line caps. Force the path the close with an explicit line, not just an implicit close of the figure.
					if (path.PointCount > 0 && !path.PathPoints[0].Equals(path.PathPoints[path.PathPoints.Length - 1]))
					{
						var i = path.PathTypes.Length - 1;
						while (i >= 0 && path.PathTypes[i] > 0)
							i--;
						if (i < 0)
							i = 0;
						path.AddLine(path.PathPoints[path.PathPoints.Length - 1], path.PathPoints[i]);
					}

					path.CloseFigure();
				}
				else if (segment is SvgLineSegment)
				{
					path.AddLine(ToDrawPoint(segment.Start), ToDrawPoint(segment.End));
				}
				else if (segment is SvgQuadraticCurveSegment)
				{
					var quadSegment = segment as SvgQuadraticCurveSegment;
					path.AddBezier(ToDrawPoint(segment.Start), ToDrawPoint(quadSegment.FirstCtrlPoint),
						ToDrawPoint(quadSegment.SecondCtrlPoint), ToDrawPoint(segment.End));
				}
				else
				{
					Debug.Warn("unknown type in getDrawingPoints");
				}
			}

			path.Flatten(new System.Drawing.Drawing2D.Matrix(), flatness);

			return System.Array.ConvertAll(path.PathPoints, i => new Vector2(i.X, i.Y));
		}
	}
}