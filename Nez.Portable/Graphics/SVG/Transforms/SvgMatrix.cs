using System.Collections.Generic;
using System.Globalization;


namespace Nez.Svg
{
	public class SvgMatrix : SvgTransform
	{
		List<float> _points;


		public SvgMatrix(List<float> points)
		{
			_points = points;
			Matrix = new Matrix2D(
				_points[0],
				_points[1],
				_points[2],
				_points[3],
				_points[4],
				_points[5]
			);
		}


		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "matrix({0}, {1}, {2}, {3}, {4}, {5})",
				_points[0], _points[1], _points[2], _points[3], _points[4], _points[5]);
		}
	}
}