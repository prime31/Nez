using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// renders a basic, CCW, convex polygon
	/// </summary>
	public class PolygonMesh : Mesh
	{
		public PolygonMesh(Vector2[] points, bool arePointsCCW = true)
		{
			var triangulator = new Triangulator();
			triangulator.Triangulate(points, arePointsCCW);

			SetVertPositions(points);
			SetTriangles(triangulator.TriangleIndices.ToArray());
			RecalculateBounds(true);
		}
	}
}