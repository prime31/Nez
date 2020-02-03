using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// builds a Polygon from the passed in verts. Verts should be relative to 0,0 and contain the outer perimeter of the polygon. A center
	/// vert will be added and used to triangulate the polygon. If you need a transform matrix for the Polygon set the position/scale and
	/// then fetch the transformMatrix property.
	/// </summary>
	public class PolygonMesh : IDisposable
	{
		VertexBuffer _vertexBuffer;
		IndexBuffer _indexBuffer;
		int _primitiveCount;


		#region static creation helpers

		public static PolygonMesh CreateRectangle()
		{
			var points = new Vector2[]
			{
				new Vector2(1, 1),
				new Vector2(0, 1),
				new Vector2(0, 0),
				new Vector2(1, 0)
			};
			return new PolygonMesh(points);
		}


		/// <summary>
		/// creates a circular polygon
		/// </summary>
		/// <returns>The symmetrical polygon.</returns>
		/// <param name="vertCount">Vert count.</param>
		/// <param name="radius">Radius.</param>
		public static PolygonMesh CreateSymmetricalPolygon(int vertCount, float radius)
		{
			return new PolygonMesh(PhysicsShapes.Polygon.BuildSymmetricalPolygon(vertCount, radius));
		}


		/// <summary>
		/// creates a circular polygon
		/// </summary>
		/// <returns>The symmetrical polygon.</returns>
		/// <param name="vertCount">Vert count.</param>
		public static PolygonMesh CreateSymmetricalPolygon(int vertCount) => CreateSymmetricalPolygon(vertCount, 1);

		#endregion


		public PolygonMesh(Vector2[] points)
		{
			var verts = GenerateVerts(points);

			// each point needs 3 verts
			var indices = new short[points.Length * 3];
			for (short i = 0; i < points.Length; i++)
			{
				indices[i * 3] = 0;
				indices[(i * 3) + 1] = (short) (i + 1);
				indices[(i * 3) + 2] = (short) (i + 2);
			}

			indices[(points.Length * 3) - 1] = 1;

			_vertexBuffer = new VertexBuffer(Core.GraphicsDevice, VertexPosition.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
			_vertexBuffer.SetData(verts);
			_indexBuffer = new IndexBuffer(Core.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
			_indexBuffer.SetData(indices);
			_primitiveCount = points.Length;
		}


		VertexPosition[] GenerateVerts(Vector2[] points)
		{
			// we need to make tris from the points. all points will be shared with the center (0,0)
			var verts = new VertexPosition[points.Length + 1];

			// the first point is the center so we start at 1
			for (var i = 1; i <= points.Length; i++)
			{
				verts[i].Position.X = points[i - 1].X;
				verts[i].Position.Y = points[i - 1].Y;
			}

			return verts;
		}


		public void Render()
		{
			Core.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
			Core.GraphicsDevice.Indices = _indexBuffer;
			Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
		}


		void IDisposable.Dispose()
		{
			_vertexBuffer.Dispose();
			_indexBuffer.Dispose();
		}
	}
}