using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	/// <summary>
	/// base class for all of the different SVG path types. Note that arcs are not supported at this time.
	/// </summary>
	public abstract class SvgPathSegment
	{
		public Vector2 start, end;


		protected SvgPathSegment()
		{}


		protected SvgPathSegment( Vector2 start, Vector2 end )
		{
			this.start = start;
			this.end = end;
		}


		protected string toSvgString( Vector2 point )
		{
			return string.Format( "{0} {1}", point.X, point.Y );
		}
	}


	public sealed class SvgMoveToSegment : SvgPathSegment
	{
		public SvgMoveToSegment( Vector2 position )
		{
			start = position;
			end = position;
		}


		public override string ToString()
		{
			return "M" + toSvgString( start );
		}
	}


	public sealed class SvgLineSegment : SvgPathSegment
	{
		public SvgLineSegment( Vector2 start, Vector2 end )
		{
			this.start = start;
			this.end = end;
		}


		public override string ToString()
		{
			return "L" + toSvgString( end );
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
		public Vector2 controlPoint;

		public Vector2 firstCtrlPoint
		{
			get
			{
				var x1 = start.X + ( controlPoint.X - start.X ) * 2 / 3;
				var y1 = start.Y + ( controlPoint.Y - start.Y ) * 2 / 3;

				return new Vector2( x1, y1 );
			}
		}

		public Vector2 secondCtrlPoint
		{
			get
			{
				var x2 = controlPoint.X + ( end.X - controlPoint.X ) / 3;
				var y2 = controlPoint.Y + ( end.Y - controlPoint.Y ) / 3;

				return new Vector2( x2, y2 );
			}
		}

		public SvgQuadraticCurveSegment( Vector2 start, Vector2 controlPoint, Vector2 end )
		{
			this.start = start;
			this.controlPoint = controlPoint;
			this.end = end;
		}


		public override string ToString()
		{
			return "Q" + toSvgString( controlPoint ) + " " + toSvgString( end );
		}

	}


	public sealed class SvgCubicCurveSegment : SvgPathSegment
	{
		public Vector2 firstCtrlPoint;
		public Vector2 secondCtrlPoint;


		public SvgCubicCurveSegment( Vector2 start, Vector2 firstCtrlPoint, Vector2 secondCtrlPoint, Vector2 end )
		{
			this.start = start;
			this.end = end;
			this.firstCtrlPoint = firstCtrlPoint;
			this.secondCtrlPoint = secondCtrlPoint;
		}


		public override string ToString()
		{
			return "C" + toSvgString( firstCtrlPoint ) + " " + toSvgString( secondCtrlPoint ) + " " + toSvgString( end );
		}
	}

}
