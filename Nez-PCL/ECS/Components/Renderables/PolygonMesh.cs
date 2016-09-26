using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// renders a basic, CCW, convex polygon
	/// </summary>
	public class PolygonMesh : Mesh
	{
		public PolygonMesh( Vector2[] points )
		{
			var triangulator = new Triangulator();
			triangulator.triangulate( points );

			setVertPositions( points );
			setTriangles( triangulator.triangleIndices.ToArray() );
			recalculateBounds( true );
		}
	}
}
