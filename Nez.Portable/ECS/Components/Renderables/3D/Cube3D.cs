using Microsoft.Xna.Framework;


namespace Nez
{
	public class Cube3D : GeometricPrimitive3D
	{
		public Cube3D()
		{
			Vector3[] normals =
			{
				new Vector3(0, 0, 1),
				new Vector3(0, 0, -1),
				new Vector3(1, 0, 0),
				new Vector3(-1, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(0, -1, 0),
			};

			Color[] colors =
			{
				Color.Red,
				Color.Yellow,
				Color.Blue,
				Color.Violet,
				Color.Green,
				Color.Orange
			};

			for (var i = 0; i < normals.Length; i++)
			{
				var vertColor = colors[i];
				var normal = normals[i];
				var side1 = new Vector3(normal.Y, normal.Z, normal.X);
				var side2 = Vector3.Cross(normal, side1);

				AddIndex(_vertices.Count + 0);
				AddIndex(_vertices.Count + 1);
				AddIndex(_vertices.Count + 2);

				AddIndex(_vertices.Count + 0);
				AddIndex(_vertices.Count + 2);
				AddIndex(_vertices.Count + 3);

				AddVertex((normal - side1 - side2) / 2, vertColor, normal);
				AddVertex((normal - side1 + side2) / 2, vertColor, normal);
				AddVertex((normal + side1 + side2) / 2, vertColor, normal);
				AddVertex((normal + side1 - side2) / 2, vertColor, normal);
			}

			InitializePrimitive();
		}
	}
}