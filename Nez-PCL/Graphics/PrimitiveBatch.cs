using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


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
		VertexPositionColor[] _triangleVertices;
		int _triangleVertsCount;


		public PrimitiveBatch( int bufferSize = 500 )
		{
			_triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];

			// set up a new basic effect, and enable vertex colors.
			_basicEffect = new BasicEffect( Core.graphicsDevice );
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}
			

		public void Dispose()
		{
			Dispose( true );
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
			var projection = Matrix.CreateOrthographicOffCenter( 0, Core.graphicsDevice.Viewport.Width, Core.graphicsDevice.Viewport.Height, 0, 0, 1 );
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
			Assert.isFalse( _hasBegun, "Invalid state. End must be called before Begin can be called again." );

			// tell our basic effect to begin.
			_basicEffect.Projection = projection;
			_basicEffect.View = view;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			// flip the error checking boolean. It's now ok to call AddVertex, Flush, and End.
			_hasBegun = true;
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
			_hasBegun = false;
		}


		public void addVertex( Vector2 vertex, Color color, PrimitiveType primitiveType )
		{
			Assert.isTrue( _hasBegun, "Invalid state. Begin must be called before AddVertex can be called." );
			Assert.isFalse( primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip || primitiveType == PrimitiveType.LineList, "The specified primitiveType is not supported by PrimitiveBatch" );

			if( _triangleVertsCount >= _triangleVertices.Length )
				flushTriangles();

			_triangleVertices[_triangleVertsCount].Position = new Vector3( vertex, -0.1f );
			_triangleVertices[_triangleVertsCount].Color = color;
			_triangleVertsCount++;
		}


		void flushTriangles()
		{
			if( !_hasBegun )
				throw new InvalidOperationException( "Begin must be called before Flush can be called." );
			
			if( _triangleVertsCount >= 3 )
			{
				var primitiveCount = _triangleVertsCount / 3;
				// submit the draw call to the graphics card
				Core.graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
				Core.graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _triangleVertices, 0, primitiveCount );
				_triangleVertsCount -= primitiveCount * 3;
			}
		}


		#region Drawing methods

		public void drawRectangle( float x, float y, float width, float height, Color color )
		{
			var verts = new Vector2[4];
			verts[2] = new Vector2( x + width, y + height );
			verts[1] = new Vector2( x + width, y );
			verts[0] = new Vector2( x, y );
			verts[3] = new Vector2( x, y + height );

			drawPolygon( verts, 4, color );
		}


		public void drawRectangle( ref Rectangle rect, Color color )
		{
			var verts = new Vector2[4];
			verts[0] = new Vector2( rect.Left, rect.Top );
			verts[1] = new Vector2( rect.Right, rect.Top );
			verts[2] = new Vector2( rect.Right, rect.Bottom );
			verts[3] = new Vector2( rect.Left, rect.Bottom );

			drawPolygon( verts, 4, color );
		}


		public void drawPolygon( Vector2[] vertices, int count, Color color )
		{
			for( int i = 1; i < count - 1; i++ )
			{
				addVertex( vertices[0], color, PrimitiveType.TriangleList );
				addVertex( vertices[i], color, PrimitiveType.TriangleList );
				addVertex( vertices[i + 1], color, PrimitiveType.TriangleList );
			}
		}


		public void drawPolygon( Vector2 position, Vector2[] vertices, Color color )
		{
			var v0 = vertices[0];
			var v1 = v0;

			for( var i = 1; i < vertices.Length; i++ )
			{
				var v2 = vertices[i];

				addVertex( position + v1, color, PrimitiveType.TriangleList );
				addVertex( position + v2, color, PrimitiveType.TriangleList );
				addVertex( position, color, PrimitiveType.TriangleList );

				v1 = v2;
			}

			addVertex( position + v1, color, PrimitiveType.TriangleList );
			addVertex( position + v0, color, PrimitiveType.TriangleList );
			addVertex( position, color, PrimitiveType.TriangleList );
		}


		public void drawCircle( Vector2 center, float radius, Color color, int circleSegments = 32 )
		{
			var increment = MathHelper.Pi * 2.0f / circleSegments;
			var theta = 0.0f;

			var v0 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
			theta += increment;

			for( int i = 1; i < circleSegments - 1; i++ )
			{
				var v1 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
				var v2 = center + radius * new Vector2( (float)Math.Cos( theta + increment ), (float)Math.Sin( theta + increment ) );

				addVertex( v0, color, PrimitiveType.TriangleList );
				addVertex( v1, color, PrimitiveType.TriangleList );
				addVertex( v2, color, PrimitiveType.TriangleList );

				theta += increment;
			}
		}


		public void drawArrow( Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color )
		{
			// Draw connection segment between start- and end-point
			//drawLine( start, end, color );

			// Precalculate halfwidth
			var halfWidth = width / 2;

			// Create directional reference
			Vector2 rotation = ( start - end );
			rotation.Normalize();

			// Calculate angle of directional vector
			float angle = (float)Math.Atan2( rotation.X, -rotation.Y );
			// Create matrix for rotation
			Matrix rotMatrix = Matrix.CreateRotationZ( angle );
			// Create translation matrix for end-point
			Matrix endMatrix = Matrix.CreateTranslation( end.X, end.Y, 0 );

			// Setup arrow end shape
			Vector2[] verts = new Vector2[3];
			verts[0] = new Vector2( 0, 0 );
			verts[1] = new Vector2( -halfWidth, -length );
			verts[2] = new Vector2( halfWidth, -length );

			// Rotate end shape
			Vector2.Transform( verts, ref rotMatrix, verts );
			// Translate end shape
			Vector2.Transform( verts, ref endMatrix, verts );

			// Draw arrow end shape
			drawPolygon( verts, 3, color );

			if( drawStartIndicator )
			{
				// Create translation matrix for start
				Matrix startMatrix = Matrix.CreateTranslation( start.X, start.Y, 0 );
				// Setup arrow start shape
				Vector2[] baseVerts = new Vector2[4];
				baseVerts[0] = new Vector2( -halfWidth, length / 4 );
				baseVerts[1] = new Vector2( halfWidth, length / 4 );
				baseVerts[2] = new Vector2( halfWidth, 0 );
				baseVerts[3] = new Vector2( -halfWidth, 0 );

				// Rotate start shape
				Vector2.Transform( baseVerts, ref rotMatrix, baseVerts );
				// Translate start shape
				Vector2.Transform( baseVerts, ref startMatrix, baseVerts );
				// Draw start shape
				drawPolygon( baseVerts, 4, color );
			}
		}
			
		#endregion

	}
}