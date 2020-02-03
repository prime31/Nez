using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Sphere3D : GeometricPrimitive3D
	{
		public Sphere3D() : this(5, Color.Red)
		{
		}

		public Sphere3D(int tessellation, Color color)
		{
			var radius = 0.5f;

			var latitudeBands = tessellation;
			var longitudeBands = tessellation * 2;
			var normal = new Vector3();
			var uv = new Vector3();

			for (var latNumber = 0; latNumber <= latitudeBands; latNumber++)
			{
				var theta = latNumber * (float) Math.PI / latitudeBands;
				var sinTheta = (float) Math.Sin(theta);
				var cosTheta = (float) Math.Cos(theta);

				for (var longNumber = 0; longNumber <= longitudeBands; longNumber++)
				{
					var phi = longNumber * 2.0f * (float) Math.PI / longitudeBands;
					var sinPhi = (float) Math.Sin(phi);
					var cosPhi = (float) Math.Cos(phi);

					normal.X = cosPhi * sinTheta;
					normal.Y = cosTheta;
					normal.Z = sinPhi * sinTheta;
					uv.X = 1.0f - ((float) longNumber / (float) longitudeBands);
					uv.Y = ((float) latNumber / (float) latitudeBands);

					_vertices.Add(new VertexPositionColorNormal(normal * radius, color, normal));
				}
			}

			for (var latNumber = 0; latNumber < latitudeBands; latNumber++)
			{
				for (var longNumber = 0; longNumber < longitudeBands; longNumber++)
				{
					var first = (latNumber * (longitudeBands + 1)) + longNumber;
					var second = first + longitudeBands + 1;

					AddIndex(first);
					AddIndex(second);
					AddIndex(first + 1);

					AddIndex(second);
					AddIndex(second + 1);
					AddIndex(first + 1);
				}
			}

			InitializePrimitive();
		}
	}
}