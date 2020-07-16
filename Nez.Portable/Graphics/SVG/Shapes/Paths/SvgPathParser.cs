using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public static class SvgPathParser
	{
		/// <summary>
		/// parses the 'd' element of an SVG file and returns the command series
		/// </summary>
		/// <param name="path">Path.</param>
		public static List<SvgPathSegment> Parse(string path)
		{
			var segments = new List<SvgPathSegment>();

			try
			{
				char command;
				bool isRelative;
				foreach (var commandSet in SplitCommands(path.TrimEnd(null)))
				{
					command = commandSet[0];
					isRelative = char.IsLower(command);

					// http://www.w3.org/TR/SVG11/paths.html#PathDataGeneralInformation

					// discard whitespace and comma but keep the -
					const string argSeparators = @"[\s,]|(?=-)";
					var remainingargs = commandSet.Substring(1);
					var splitArgs = Regex.Split(remainingargs, argSeparators)
						.Where(t => !string.IsNullOrEmpty(t));

					var floatArgs = splitArgs.Select(arg => float.Parse(arg)).ToArray();

					CreatePathSegment(command, segments, floatArgs, isRelative);
				}
			}
			catch (System.Exception exc)
			{
				Debug.Error("Error parsing path \"{0}\": {1}", path, exc.Message);
			}

			return segments;
		}


		/// <summary>
		/// creates an SvgPathSegment based on the command and coords passed in
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="segments">Segments.</param>
		/// <param name="coords">Coords.</param>
		/// <param name="isRelative">If set to <c>true</c> is relative.</param>
		static void CreatePathSegment(char command, List<SvgPathSegment> segments, float[] coords, bool isRelative)
		{
			switch (command)
			{
				case 'm': // relative moveto
				case 'M': // moveto
				{
					segments.Add(new SvgMoveToSegment(ToAbsolute(coords[0], coords[1], segments, isRelative)));

					var index = 2;
					while (index < coords.Length)
					{
						segments.Add(new SvgLineSegment(segments.LastItem().End,
							ToAbsolute(coords[index], coords[index + 1], segments, isRelative)));
						index += 2;
					}
				}
					break;

				case 'a': // relative arc
				case 'A': // arc
				{
					throw new System.NotImplementedException();
				}

				case 'l': // relative lineto
				case 'L': // lineto
				{
					var index = 0;
					while (index < coords.Length)
					{
						segments.Add(new SvgLineSegment(segments.LastItem().End,
							ToAbsolute(coords[index], coords[index + 1], segments, isRelative)));
						index += 2;
					}
				}
					break;

				case 'H': // horizontal lineto
				case 'h': // relative horizontal lineto
				{
					var index = 0;
					while (index < coords.Length)
					{
						segments.Add(new SvgLineSegment(segments.LastItem().End,
							ToAbsolute(coords[index], segments.LastItem().End.Y, segments, isRelative, false)));
						index += 1;
					}
				}
					break;

				case 'V': // vertical lineto
				case 'v': // relative vertical lineto
				{
					var index = 0;
					while (index < coords.Length)
					{
						segments.Add(new SvgLineSegment(segments.LastItem().End,
							ToAbsolute(coords[index], segments.LastItem().End.X, segments, false, isRelative)));
						index += 1;
					}
				}
					break;

				case 'Q': // curveto
				case 'q': // relative curveto
				{
					var index = 0;
					while (index < coords.Length)
					{
						var controlPoint = ToAbsolute(coords[index], coords[index + 1], segments, isRelative);
						var end = ToAbsolute(coords[index + 2], coords[index + 3], segments, isRelative);
						segments.Add(new SvgQuadraticCurveSegment(segments.LastItem().End, controlPoint, end));
						index += 4;
					}
				}
					break;

				case 'T': // shorthand/smooth curveto
				case 't': // relative shorthand/smooth curveto
				{
					var index = 0;
					while (index < coords.Length)
					{
						var lastQuadCurve = segments.LastItem() as SvgQuadraticCurveSegment;

						var controlPoint = lastQuadCurve != null
							? Reflect(lastQuadCurve.ControlPoint, segments.LastItem().End)
							: segments.LastItem().End;

						var end = ToAbsolute(coords[index], coords[index + 1], segments, isRelative);
						segments.Add(new SvgQuadraticCurveSegment(segments.LastItem().End, controlPoint, end));
						index += 2;
					}
				}
					break;

				case 'C': // curveto
				case 'c': // relative curveto
				{
					var index = 0;
					while (index < coords.Length)
					{
						var firstControlPoint = ToAbsolute(coords[index], coords[index + 1], segments, isRelative);
						var secondControlPoint = ToAbsolute(coords[index + 2], coords[index + 3], segments, isRelative);
						var end = ToAbsolute(coords[index + 4], coords[index + 5], segments, isRelative);
						segments.Add(new SvgCubicCurveSegment(segments.LastItem().End, firstControlPoint,
							secondControlPoint, end));
						index += 6;
					}
				}
					break;

				case 'S': // shorthand/smooth curveto
				case 's': // relative shorthand/smooth curveto
				{
					var index = 0;
					while (index < coords.Length)
					{
						var lastCubicCurve = segments.LastItem() as SvgCubicCurveSegment;

						var firstControlPoint = lastCubicCurve != null
							? Reflect(lastCubicCurve.SecondCtrlPoint, segments.LastItem().End)
							: segments.LastItem().End;

						var secondControlPoint = ToAbsolute(coords[index], coords[index + 1], segments, isRelative);
						var end = ToAbsolute(coords[index + 2], coords[index + 3], segments, isRelative);
						segments.Add(new SvgCubicCurveSegment(segments.LastItem().End, firstControlPoint,
							secondControlPoint, end));
						index += 4;
					}
				}
					break;

				case 'Z': // closepath
				case 'z': // relative closepath
				{
					segments.Add(new SvgClosePathSegment());
				}
					break;
			}
		}


		/// <summary>
		/// Creates point with absolute coordinates
		/// </summary>
		/// <returns>The absolute.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="segments">Segments.</param>
		/// <param name="areBothRelative">If set to <c>true</c> is relative both.</param>
		static Vector2 ToAbsolute(float x, float y, List<SvgPathSegment> segments, bool areBothRelative)
		{
			return ToAbsolute(x, y, segments, areBothRelative, areBothRelative);
		}


		/// <summary>
		/// Creates point with absolute coordinates
		/// </summary>
		/// <param name="x">Raw X-coordinate value.</param>
		/// <param name="y">Raw Y-coordinate value.</param>
		/// <param name="segments">Current path segments.</param>
		/// <param name="isRelativeX"><b>true</b> if <paramref name="x"/> contains relative coordinate value, otherwise <b>false</b>.</param>
		/// <param name="isRelativeY"><b>true</b> if <paramref name="y"/> contains relative coordinate value, otherwise <b>false</b>.</param>
		/// <returns><see cref="Vector2"/> that contains absolute coordinates.</returns>
		static Vector2 ToAbsolute(float x, float y, List<SvgPathSegment> segments, bool isRelativeX, bool isRelativeY)
		{
			var point = new Vector2(x, y);

			if ((isRelativeX || isRelativeY) && segments.Count > 0)
			{
				var lastSegment = segments.LastItem();

				// if the last element is a SvgClosePathSegment the position of the previous element should be used because the position of SvgClosePathSegment is 0,0
				if (lastSegment is SvgClosePathSegment)
					lastSegment = ((IList<SvgPathSegment>) segments).Reverse().OfType<SvgMoveToSegment>().First();

				if (isRelativeX)
					point.X += lastSegment.End.X;

				if (isRelativeY)
					point.Y += lastSegment.End.Y;
			}

			return point;
		}


		static Vector2 Reflect(Vector2 point, Vector2 mirror)
		{
			float x, y, dx, dy;
			dx = System.Math.Abs(mirror.X - point.X);
			dy = System.Math.Abs(mirror.Y - point.Y);

			if (mirror.X >= point.X)
				x = mirror.X + dx;
			else
				x = mirror.X - dx;

			if (mirror.Y >= point.Y)
				y = mirror.Y + dy;
			else
				y = mirror.Y - dy;

			return new Vector2(x, y);
		}


		static IEnumerable<string> SplitCommands(string path)
		{
			var commandStart = 0;

			for (var i = 0; i < path.Length; i++)
			{
				string command;
				if (char.IsLetter(path[i]) && path[i] != 'e') //e is used in scientific notiation. but not svg path
				{
					command = path.Substring(commandStart, i - commandStart).Trim();
					commandStart = i;

					if (!string.IsNullOrEmpty(command))
						yield return command;

					if (path.Length == i + 1)
						yield return path[i].ToString();
				}
				else if (path.Length == i + 1)
				{
					command = path.Substring(commandStart, i - commandStart + 1).Trim();

					if (!string.IsNullOrEmpty(command))
						yield return command;
				}
			}
		}
	}
}