using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public abstract class GeometricPrimitive3D : Renderable3D, IDisposable
	{
		protected List<VertexPositionColorNormal> _vertices = new List<VertexPositionColorNormal>();
		protected List<ushort> _indices = new List<ushort>();

		VertexBuffer _vertexBuffer;
		IndexBuffer _indexBuffer;
		BasicEffect _basicEffect;


		#region Initialization and configuration

		protected void addVertex( Vector3 position, Color color, Vector3 normal )
		{
			_vertices.Add( new VertexPositionColorNormal( position, color, normal ) );
		}


		protected void addIndex( int index )
		{
			_indices.Add( (ushort)index );
		}


		/// <summary>
		/// Once all the geometry has been specified by calling AddVertex and AddIndex, this method copies the vertex and index data into
		/// GPU format buffers, ready for efficient rendering.
		protected void initializePrimitive()
		{
			// create a vertex buffer, and copy our vertex data into it.
			_vertexBuffer = new VertexBuffer( Core.graphicsDevice, typeof( VertexPositionColorNormal ), _vertices.Count, BufferUsage.None );
			_vertexBuffer.SetData( _vertices.ToArray() );

			// create an index buffer, and copy our index data into it.
			_indexBuffer = new IndexBuffer( Core.graphicsDevice, typeof( ushort ), _indices.Count, BufferUsage.None );
			_indexBuffer.SetData( _indices.ToArray() );
		}


		public override void onAddedToEntity()
		{
			base.onAddedToEntity();

			_basicEffect = entity.scene.content.loadMonoGameEffect<BasicEffect>();
			_basicEffect.VertexColorEnabled = true;
			_basicEffect.EnableDefaultLighting();
		}

		#endregion


		#region IDisposable

		~GeometricPrimitive3D()
		{
			Dispose( false );
		}


		/// <summary>
		/// frees resources used by this object.
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}


		/// <summary>
		/// frees resources used by this object.
		/// </summary>
		protected virtual void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( _vertexBuffer != null )
					_vertexBuffer.Dispose();

				if( _indexBuffer != null )
					_indexBuffer.Dispose();

				if( _basicEffect != null )
					_basicEffect.Dispose();
			}
		}

		#endregion


		public override void render( Graphics graphics, Camera camera )
		{
			// flush the 2D batch so we render appropriately depth-wise
			graphics.batcher.flushBatch();

			Core.graphicsDevice.BlendState = BlendState.Opaque;
			Core.graphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Set BasicEffect parameters.
			_basicEffect.World = worldMatrix;
			_basicEffect.View = camera.viewMatrix3D;
			_basicEffect.Projection = camera.projectionMatrix3D;
			_basicEffect.DiffuseColor = color.ToVector3();

			// Set our vertex declaration, vertex buffer, and index buffer.
			Core.graphicsDevice.SetVertexBuffer( _vertexBuffer );
			Core.graphicsDevice.Indices = _indexBuffer;

			_basicEffect.CurrentTechnique.Passes[0].Apply();
			var primitiveCount = _indices.Count / 3;
			Core.graphicsDevice.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0, primitiveCount );
		}

	}
}
