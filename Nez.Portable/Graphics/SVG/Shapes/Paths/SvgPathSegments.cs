using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	/// <summary>
	/// base class for all of the different SVG path types. Note that arcs are not supported at this time.
	/// </summary>
	public abstract class SvgPathSegment
	{
		public Vector2 Start, End;


		protected SvgPathSegment()
		{
		}


		protected SvgPathSegment(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}


		protected string ToSvgString(Vector2 point)
		{
			return string.Format("{0} {1}", point.X, point.Y);
		}
	}


	public sealed class SvgMoveToSegment : SvgPathSegment
	{
		public SvgMoveToSegment(Vector2 position)
		{
			Start = position;
			End = position;
		}


		public override string ToString()
		{
			return "M" + ToSvgString(Start);
		}
	}


	public sealed class SvgLineSegment : SvgPathSegment
	{
		public SvgLineSegment(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}


		public override string ToString()
		{
			return "L" + ToSvgString(End);
		}
	}


	public sealed class SvgClosePathSegment : SvgPathSegment
	{
		public override string ToString()
		{
			return "z";
		}
	}


	public sealed class SvgQuadraticCurveSegment : SvgPathSegment
	{
		public Vector2 ControlPoint;

		public Vector2 FirstCtrlPoint
		{
			get
			{
				var x1 = Start.X + (ControlPoint.X - Start.X) * 2 / 3;
				var y1 = Start.Y + (ControlPoint.Y - Start.Y) * 2 / 3;

				return new Vector2(x1, y1);
			}
		}

		public Vector2 SecondCtrlPoint
		{
			get
			{
				var x2 = ControlPoint.X + (End.X - ControlPoint.X) / 3;
				var y2 = ControlPoint.Y + (End.Y - ControlPoint.Y) / 3;

				return new Vector2(x2, y2);
			}
		}

		public SvgQuadraticCurveSegment(Vector2 start, Vector2 controlPoint, Vector2 end)
		{
			Start = start;
			ControlPoint = controlPoint;
			End = end;
		}


		public override string ToString()
		{
			return "Q" + ToSvgString(ControlPoint) + " " + ToSvgString(End);
		}
	}


	public sealed class SvgCubicCurveSegment : SvgPathSegment
	{
		public Vector2 FirstCtrlPoint;
		public Vector2 SecondCtrlPoint;


		public SvgCubicCurveSegment(Vector2 start, Vector2 firstCtrlPoint, Vector2 secondCtrlPoint, Vector2 end)
		{
			Start = start;
			End = end;
			FirstCtrlPoint = firstCtrlPoint;
			SecondCtrlPoint = secondCtrlPoint;
		}


		public override string ToString()
		{
			return "C" + ToSvgString(FirstCtrlPoint) + " " + ToSvgString(SecondCtrlPoint) + " " + ToSvgString(End);
		}
	}
}