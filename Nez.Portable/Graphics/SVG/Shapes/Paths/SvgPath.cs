using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Nez.Splines;


namespace Nez.Svg
{
	/// <summary>
	/// representation of a path element. Note that the best way to get points from the path is to use the getTransformedPoints method. It uses
	/// System.Drawing to generate the path points from any type of path. The getOptimized*DrawingPoints methods only work for cubic bezier
	/// curves.
	/// 
	/// SvgPath can be instantiated directly. You can then just set the 'd' property to have the path parsed.
	/// </summary>
	public class SvgPath : SvgElement
	{
		[XmlAttribute("d")]
		public string D
		{
			get => null;
			set => Segments = SvgPathParser.Parse(value);
		}

		public List<SvgPathSegment> Segments;


		/// <summary>
		/// gets the points that make up the path with any transforms present applied. The points can be used to approximate the path by
		/// drawing lines between them.
		/// 
		/// Important notes: ISvgPathBuilder is a faux interface that is required because PCLs cannot access System.Drawing which is used
		/// to get the drawing points. In order to use this method you need to put the SvgPathBuilder in your main project and then pass in
		/// an SvgPathBuilder object to this method.
		/// </summary>
		/// <returns>The transformed drawing points.</returns>
		/// <param name="pathBuilder">Path builder.</param>
		/// <param name="flatness">Flatness.</param>
		public Vector2[] GetTransformedDrawingPoints(ISvgPathBuilder pathBuilder, float flatness = 3)
		{
			var pts = pathBuilder.GetDrawingPoints(Segments, flatness);
			var mat = GetCombinedMatrix();
			Vector2Ext.Transform(pts, ref mat, pts);

			return pts;
		}


		/// <summary>
		/// returns true if all the segments are cubic curves
		/// </summary>
		/// <returns><c>true</c>, if path bezier was ised, <c>false</c> otherwise.</returns>
		public bool IsPathCubicBezier()
		{
			for (var i = 1; i < Segments.Count; i++)
			{
				if (!(Segments[i] is SvgCubicCurveSegment))
					return false;
			}

			return true;
		}


		/// <summary>
		/// gets a BezierSpline for the SvgPath
		/// </summary>
		/// <returns>The bezier spline for path.</returns>
		public BezierSpline GetBezierSplineForPath()
		{
			Insist.IsTrue(IsPathCubicBezier(), "SvgPath is not a cubic bezier");

			var bezier = new BezierSpline();
			for (var i = 1; i < Segments.Count; i++)
			{
				var cub = Segments[i] as SvgCubicCurveSegment;
				bezier.AddCurve(cub.Start, cub.FirstCtrlPoint, cub.SecondCtrlPoint, cub.End);
			}

			return bezier;
		}


		/// <summary>
		/// gets optimized drawing points with extra points in curves and less in straight lines. Returns a pooled list that should be returned to the ListPool when done.
		/// </summary>
		/// <returns>The optimized drawing points.</returns>
		/// <param name="distanceTolerance">Distance tolerance.</param>
		public List<Vector2> GetOptimizedDrawingPoints(float distanceTolerance = 2f)
		{
			Insist.IsTrue(IsPathCubicBezier(), "SvgPath is not a cubic bezier");

			var points = ListPool<Vector2>.Obtain();
			for (var i = 1; i < Segments.Count; i++)
			{
				var cub = Segments[i] as SvgCubicCurveSegment;
				var pts = Bezier.GetOptimizedDrawingPoints(cub.Start, cub.FirstCtrlPoint, cub.SecondCtrlPoint, cub.End,
					distanceTolerance);

				// as long as this isnt the first segment, we can remove the first drawing point since it will be identical to the last one
				if (i != 1)
					pts.RemoveAt(0);
				points.AddRange(pts);
				ListPool<Vector2>.Free(pts);
			}

			return points;
		}


		/// <summary>
		/// gets optimized drawing points with extra points in curves and less in straight lines with any transforms present applied
		/// </summary>
		/// <returns>The optimized drawing points.</returns>
		/// <param name="distanceTolerance">Distance tolerance.</param>
		public Vector2[] GetOptimizedTransformedDrawingPoints(float distanceTolerance = 2f)
		{
			var pointList = GetOptimizedDrawingPoints(distanceTolerance);
			var points = pointList.ToArray();
			ListPool<Vector2>.Free(pointList);

			var mat = GetCombinedMatrix();
			Vector2Ext.Transform(points, ref mat, points);

			return points;
		}
	}
}