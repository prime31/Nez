using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


namespace Nez
{
	public class PrimitiveBatch : IDisposable
	{
		const int DefaultBufferSize = 500;
		BasicEffect _basicEffect;

		// the device that we will issue draw calls to.
		GraphicsDevice _device;

		// hasBegun is flipped to true once Begin is called, and is used to make
		// sure users don't call End before Begin is called.
		bool _hasBegun;

		bool _isDisposed;
		VertexPositionColor[] _lineVertices;
		int _lineVertsCount;
		VertexPositionColor[] _triangleVertices;
		int _triangleVertsCount;


		public PrimitiveBatch( GraphicsDevice graphicsDevice, int bufferSize = DefaultBufferSize )
		{
			Debug.assertIsNotNull( graphicsDevice, "missing graphicsDevice" );

			_device = graphicsDevice;

			_triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
			_lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

			// set up a new basic effect, and enable vertex colors.
			_basicEffect = new BasicEffect( graphicsDevice );
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}
			

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}


		protected virtual void Dispose( bool disposing )
		{
			if( disposing && !_isDisposed )
			{
				if( _basicEffect != null )
					_basicEffect.Dispose();

				_isDisposed = true;
			}
		}


		/// <summary>
		/// draws directly in screen space at full viewport size
		/// </summary>
		public void begin()
		{
			var projection = Matrix.CreateOrthographicOffCenter( 0, Core.graphicsDevice.Viewport.Width,
					Core.graphicsDevice.Viewport.Height, 0, 0, 1 );
			var view = Matrix.CreateLookAt( Vector3.Zero, Vector3.Forward, Vector3.Up );

			begin( ref projection, ref view );
		}


		/// <summary>
		/// Begin is called to tell the PrimitiveBatch what kind of primitives will be
		/// drawn, and to prepare the graphics card to render those primitives.
		/// </summary>
		/// <param name="projection">The projection.</param>
		/// <param name="view">The view.</param>
		public void begin( ref Matrix projection, ref Matrix view )
		{
			Debug.assertIsFalse( _hasBegun, "Invalid state. End must be called before Begin can be called again." );

			// tell our basic effect to begin.
			_basicEffect.Projection = projection;
			_basicEffect.View = view;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			// flip the error checking boolean. It's now ok to call AddVertex, Flush, and End.
			_hasBegun = true;
		}


		public void addVertex( Vector2 vertex, Color color, PrimitiveType primitiveType )
		{
			Debug.assertIsTrue( _hasBegun, "Invalid state. Begin must be called before AddVertex can be called." );
			Debug.assertIsFalse( primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip, "The specified primitiveType is not supported by PrimitiveBatch" );

			if( primitiveType == PrimitiveType.TriangleList )
			{
				if( _triangleVertsCount >= _triangleVertices.Length )
					flushTriangles();

				_triangleVertices[_triangleVertsCount].Position = new Vector3( vertex, -0.1f );
				_triangleVertices[_triangleVertsCount].Color = color;
				_triangleVertsCount++;
			}

			if( primitiveType == PrimitiveType.LineList )
			{
				if( _lineVertsCount >= _lineVertices.Length )
					flushLines();

				_lineVertices[_lineVertsCount].Position = new Vector3( vertex, 0f );
				_lineVertices[_lineVertsCount].Color = color;
				_lineVertsCount++;
			}
		}


		/// <summary>
		/// End is called once all the primitives have been drawn using AddVertex.
		/// it will call Flush to actually submit the draw call to the graphics card, and
		/// then tell the basic effect to end.
		/// </summary>
		public void end()
		{
			if( !_hasBegun )
				throw new InvalidOperationException( "Begin must be called before End can be called." );

			// Draw whatever the user wanted us to draw
			flushTriangles();
			flushLines();

			_hasBegun = false;
		}


		void flushTriangles()
		{
			if( !_hasBegun )
				throw new InvalidOperationException( "Begin must be called before Flush can be called." );
			
			if( _triangleVertsCount >= 3 )
			{
				int primitiveCount = _triangleVertsCount / 3;
				// submit the draw call to the graphics card
				_device.SamplerStates[0] = SamplerState.AnisotropicClamp;
				_device.DrawUserPrimitives( PrimitiveType.TriangleList, _triangleVertices, 0, primitiveCount );
				_triangleVertsCount -= primitiveCount * 3;
			}
		}


		void flushLines()
		{
			if( !_hasBegun )
				throw new InvalidOperationException( "Begin must be called before Flush can be called." );
			
			if( _lineVertsCount >= 2 )
			{
				int primitiveCount = _lineVertsCount / 2;
				// submit the draw call to the graphics card
				_device.SamplerStates[0] = SamplerState.AnisotropicClamp;
				_device.DrawUserPrimitives( PrimitiveType.LineList, _lineVertices, 0, primitiveCount );
				_lineVertsCount -= primitiveCount * 2;
			}
		}
	
	}
}