using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	/// <summary>
	/// THIS IS A HORRIBLE ABOMINATION! PCLs dont have access to System.Drawing so this class is a wrapper for accessing the GraphicsPath class.
	/// It has the full public API that Nez needs for SVG files but the whole mess is all accessed via reflection. It is slow as all hell and not
	/// recommended for production use. It's only purpose is so that Nez works with SVG files out of the box to get up and running fast.
	/// </summary>
	public class FauxGraphicsPath
	{
		public int PointCount
		{
			get
			{
				var prop = ReflectionUtils.GetPropertyInfo(_graphicsPath, "PointCount");
				return (int) prop.GetValue(_graphicsPath);
			}
		}

		public System.Array PathPoints
		{
			get
			{
				var prop = ReflectionUtils.GetPropertyInfo(_graphicsPath, "PathPoints");
				return (System.Array) prop.GetValue(_graphicsPath);
			}
		}

		public int[] PathTypes
		{
			get
			{
				var prop = ReflectionUtils.GetPropertyInfo(_graphicsPath, "PathTypes");
				return (int[]) prop.GetValue(_graphicsPath);
			}
		}


		object _graphicsPath;


		public FauxGraphicsPath()
		{
			_graphicsPath =
				System.Activator.CreateInstance(
					System.Type.GetType("System.Drawing.Drawing2D.GraphicsPath, System.Drawing"));
		}


		public void StartFigure()
		{
			var method = ReflectionUtils.GetMethodInfo(_graphicsPath, "StartFigure");
			method.Invoke(_graphicsPath, new object[0]);
		}


		public void CloseFigure()
		{
			var method = ReflectionUtils.GetMethodInfo(_graphicsPath, "CloseFigure");
			method.Invoke(_graphicsPath, new object[0]);
		}


		public void AddBezier(object first, object second, object third, object fourth)
		{
			var method = ReflectionUtils.GetMethodInfo(_graphicsPath, "AddBezier");
			method.Invoke(_graphicsPath, new object[] {first, second, third, fourth});
		}


		public void AddLine(object first, object second)
		{
			var method = ReflectionUtils.GetMethodInfo(_graphicsPath, "AddLine");
			method.Invoke(_graphicsPath, new object[] {first, second});
		}


		public void Flatten(object matrix, float flatness)
		{
			var paramTypes = new System.Type[] {matrix.GetType(), flatness.GetType()};
			var method = ReflectionUtils.GetMethodInfo(_graphicsPath, "Flatten", paramTypes);
			method.Invoke(_graphicsPath, new object[] {matrix, flatness});
		}


		public Vector2[] PathPointsAsVectors()
		{
			var pathPoints = PathPoints;
			if (pathPoints.Length == 0)
				return new Vector2[0];

			var pts = new Vector2[pathPoints.Length];
			var getX = ReflectionUtils.GetPropertyInfo(pathPoints.GetValue(0), "X");
			var getY = ReflectionUtils.GetPropertyInfo(pathPoints.GetValue(0), "Y");

			for (var i = 0; i < pathPoints.Length; i++)
			{
				var obj = pathPoints.GetValue(i);
				pts[i] = new Vector2((float) getX.GetValue(obj), (float) getY.GetValue(obj));
			}

			return pts;
		}
	}
}