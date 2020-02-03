using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.DeferredLighting
{
	public class QuadMesh : IDisposable
	{
		VertexBuffer _vertexBuffer;
		IndexBuffer _indexBuffer;


		public QuadMesh(GraphicsDevice device)
		{
			var verts = new VertexPositionTexture[]
			{
				new VertexPositionTexture(
					new Vector3(1, -1, 0),
					new Vector2(1, 1)),
				new VertexPositionTexture(
					new Vector3(-1, -1, 0),
					new Vector2(0, 1)),
				new VertexPositionTexture(
					new Vector3(-1, 1, 0),
					new Vector2(0, 0)),
				new VertexPositionTexture(
					new Vector3(1, 1, 0),
					new Vector2(1, 0))
			};

			var indices = new short[] {0, 1, 2, 2, 3, 0};

			_vertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
			_vertexBuffer.SetData(verts);
			_indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
			_indexBuffer.SetData(indices);
		}


		public void Render()
		{
			Core.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
			Core.GraphicsDevice.Indices = _indexBuffer;
			Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
		}


		void IDisposable.Dispose()
		{
			_vertexBuffer.Dispose();
			_indexBuffer.Dispose();
		}
	}
}