using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// batcher that draws vertex colored triangles
	/// </summary>
	public class PrimitiveBatch : IDisposable
	{
		BasicEffect _basicEffect;

		// hasBegun is flipped to true once Begin is called, and is used to make
		// sure users don't call End before Begin is called.
		bool _hasBegun;

		bool _isDisposed;
		VertexPositionColor[] _lineVertices;
		int _lineVertsCount;
		VertexPositionColor[] _triangleVertices;
		int _triangleVertsCount;
		Vector2[] _rectangleVerts;

		public PrimitiveBatch(int bufferSize = 500)
		{
			_triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
			_lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];
			_rectangleVerts = new Vector2[4];

			// set up a new basic effect, and enable vertex colors.
			_basicEffect = new BasicEffect(Core.GraphicsDevice);
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}

		public void Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				if (_basicEffect != null)
					_basicEffect.Dispose();

				_isDisposed = true;
			}
		}

		/// <summary>
		/// draws directly in screen space at full viewport size
		/// </summary>
		public void Begin()
		{
			var projection = Matrix.CreateOrthographicOffCenter(0, Core.GraphicsDevice.Viewport.Width,
				Core.GraphicsDevice.Viewport.Height, 0, 0, -1);
			var view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

			Begin(ref projection, ref view);
		}

		/// <summary>
		/// Begin is called to tell the PrimitiveBatch what kind of primitives will be drawn, and to prepare the graphics card to render those primitives.
		/// Use camera.projectionMatrix and camera.transformMatrix if the batch should be in camera space.
		/// </summary>
		/// <param name="projection">The projection.</param>
		/// <param name="view">The view.</param>
		public void Begin(ref Matrix projection, ref Matrix view)
		{
			Insist.IsFalse(_hasBegun, "Invalid state. End must be called before Begin can be called again.");

			// tell our basic effect to begin.
			_basicEffect.Projection = projection;
			_basicEffect.View = view;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			// flip the error checking boolean. It's now ok to call AddVertex, Flush, and End.
			_hasBegun = true;
		}

		/// <summary>
		/// Begin is called to tell the PrimitiveBatch what kind of primitives will be drawn, and to prepare the graphics card to render those primitives.
		/// Use camera.projectionMatrix and camera.transformMatrix if the batch should be in camera space.
		/// </summary>
		/// <param name="projection">The projection.</param>
		/// <param name="view">The view.</param>
		public void Begin(Matrix projection, Matrix view) => Begin(ref projection, ref view);

		/// <summary>
		/// End is called once all the primitives have been drawn using AddVertex.
		/// it will call Flush to actually submit the draw call to the graphics card, and
		/// then tell the basic effect to end.
		/// </summary>
		public void End()
		{
			if (!_hasBegun)
				throw new InvalidOperationException("Begin must be called before End can be called.");

			// Draw whatever the user wanted us to draw
			FlushTriangles();
			FlushLines();
			_hasBegun = false;
		}

		public void AddVertex(Vector2 vertex, Color color, PrimitiveType primitiveType)
		{
			Insist.IsTrue(_hasBegun, "Invalid state. Begin must be called before AddVertex can be called.");
			Insist.IsFalse(primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip,
				"The specified primitiveType is not supported by PrimitiveBatch");

			if (primitiveType == PrimitiveType.TriangleList)
			{
				if (_triangleVertsCount >= _triangleVertices.Length)
					FlushTriangles();

				_triangleVertices[_triangleVertsCount].Position = new Vector3(vertex, 0);
				_triangleVertices[_triangleVertsCount].Color = color;
				_triangleVertsCount++;
			}

			if (primitiveType == PrimitiveType.LineList)
			{
				if (_lineVertsCount >= _lineVertices.Length)
					FlushLines();

				_lineVertices[_lineVertsCount].Position = new Vector3(vertex, 0);
				_lineVertices[_lineVertsCount].Color = color;
				_lineVertsCount++;
			}
		}

		void FlushTriangles()
		{
			if (!_hasBegun)
				throw new InvalidOperationException("Begin must be called before Flush can be called.");

			if (_triangleVertsCount >= 3)
			{
				var primitiveCount = _triangleVertsCount / 3;

				// submit the draw call to the graphics card
				Core.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
				Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _triangleVertices, 0, primitiveCount);
				_triangleVertsCount -= primitiveCount * 3;
			}
		}

		void FlushLines()
		{
			if (!_hasBegun)
				throw new InvalidOperationException("Begin must be called before Flush can be called.");

			if (_lineVertsCount >= 2)
			{
				int primitiveCount = _lineVertsCount / 2;

				// submit the draw call to the graphics card
				Core.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
				Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, primitiveCount);
				_lineVertsCount -= primitiveCount * 2;
			}
		}

		#region Drawing methods

		public void DrawRectangle(float x, float y, float width, float height, Color color)
		{
			_rectangleVerts[2] = new Vector2(x + width, y + height);
			_rectangleVerts[1] = new Vector2(x + width, y);
			_rectangleVerts[0] = new Vector2(x, y);
			_rectangleVerts[3] = new Vector2(x, y + height);

			DrawPolygon(_rectangleVerts, 4, color);
		}

		public void DrawRectangle(ref Rectangle rect, Color color)
		{
			_rectangleVerts[0] = new Vector2(rect.Left, rect.Top);
			_rectangleVerts[1] = new Vector2(rect.Right, rect.Top);
			_rectangleVerts[2] = new Vector2(rect.Right, rect.Bottom);
			_rectangleVerts[3] = new Vector2(rect.Left, rect.Bottom);

			DrawPolygon(_rectangleVerts, 4, color);
		}

		public void DrawPolygon(Vector2[] vertices, int count, Color color)
		{
			for (int i = 1; i < count - 1; i++)
			{
				AddVertex(vertices[0], color, PrimitiveType.TriangleList);
				AddVertex(vertices[i], color, PrimitiveType.TriangleList);
				AddVertex(vertices[i + 1], color, PrimitiveType.TriangleList);
			}
		}

		public void DrawPolygon(Vector2 position, Vector2[] vertices, Color color)
		{
			var v0 = vertices[0];
			var v1 = v0;

			for (var i = 1; i < vertices.Length; i++)
			{
				var v2 = vertices[i];

				AddVertex(position + v1, color, PrimitiveType.TriangleList);
				AddVertex(position + v2, color, PrimitiveType.TriangleList);
				AddVertex(position, color, PrimitiveType.TriangleList);

				v1 = v2;
			}

			AddVertex(position + v1, color, PrimitiveType.TriangleList);
			AddVertex(position + v0, color, PrimitiveType.TriangleList);
			AddVertex(position, color, PrimitiveType.TriangleList);
		}

		public void DrawCircle(Vector2 center, float radius, Color color, int circleSegments = 32)
		{
			var increment = MathHelper.Pi * 2.0f / circleSegments;
			var theta = 0.0f;

			var v0 = center + radius * new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
			theta += increment;

			for (int i = 1; i < circleSegments - 1; i++)
			{
				var v1 = center + radius * new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
				var v2 = center + radius * new Vector2((float) Math.Cos(theta + increment),
					         (float) Math.Sin(theta + increment));

				AddVertex(v0, color, PrimitiveType.TriangleList);
				AddVertex(v1, color, PrimitiveType.TriangleList);
				AddVertex(v2, color, PrimitiveType.TriangleList);

				theta += increment;
			}
		}

		public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color)
		{
			// Draw connection segment between start- and end-point
			//drawLine( start, end, color );

			// Precalculate halfwidth
			var halfWidth = width / 2;

			// Create directional reference
			Vector2 rotation = (start - end);
			rotation.Normalize();

			// Calculate angle of directional vector
			float angle = (float) Math.Atan2(rotation.X, -rotation.Y);

			// Create matrix for rotation
			Matrix2D rotMatrix = Matrix2D.CreateRotation(angle);

			// Create translation matrix for end-point
			Matrix2D endMatrix = Matrix2D.CreateTranslation(end.X, end.Y);

			// Setup arrow end shape
			Vector2[] verts = new Vector2[3];
			verts[0] = new Vector2(0, 0);
			verts[1] = new Vector2(-halfWidth, -length);
			verts[2] = new Vector2(halfWidth, -length);

			// Rotate end shape
			Vector2Ext.Transform(verts, ref rotMatrix, verts);

			// Translate end shape
			Vector2Ext.Transform(verts, ref endMatrix, verts);

			// Draw arrow end shape
			DrawPolygon(verts, 3, color);

			if (drawStartIndicator)
			{
				// Create translation matrix for start
				Matrix2D startMatrix = Matrix2D.CreateTranslation(start.X, start.Y);

				// Setup arrow start shape
				Vector2[] baseVerts = new Vector2[4];
				baseVerts[0] = new Vector2(-halfWidth, length / 4);
				baseVerts[1] = new Vector2(halfWidth, length / 4);
				baseVerts[2] = new Vector2(halfWidth, 0);
				baseVerts[3] = new Vector2(-halfWidth, 0);

				// Rotate start shape
				Vector2Ext.Transform(baseVerts, ref rotMatrix, baseVerts);

				// Translate start shape
				Vector2Ext.Transform(baseVerts, ref startMatrix, baseVerts);

				// Draw start shape
				DrawPolygon(baseVerts, 4, color);
			}
		}

		#endregion
	}
}