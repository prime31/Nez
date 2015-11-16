using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


// TODO: use a temp array of verts instead of creating new arrays for every call
using Nez.Physics;


namespace Nez
{
	public class GeometryBatch
	{
		public const int CircleSegments = 32;

		private PrimitiveBatch _primitiveBatch;
		private GraphicsDevice _device;


		public GeometryBatch( GraphicsDevice device )
		{
			_device = device;
			_primitiveBatch = new PrimitiveBatch( _device, 1000 );
		}


		// example: geometryBatch.beginCustomDraw( camera.getProjectionMatrix(), camera.transformMatrix );
		public void beginCustomDraw( Matrix projection, Matrix view )
		{
			beginCustomDraw( ref projection, ref view );
		}


		public void beginCustomDraw( ref Matrix projection, ref Matrix view )
		{
			_primitiveBatch.begin( ref projection, ref view );
		}


		public void endCustomDraw()
		{
			_primitiveBatch.end();
		}


		public void drawRectangle( float x, float y, float width, float height, Color color )
		{
			var verts = new Vector2[4];
			verts[0] = new Vector2( x, y + height );
			verts[1] = new Vector2( x + width, y + height );
			verts[2] = new Vector2( x + width, y );
			verts[3] = new Vector2( x, y );

			drawPolygon( verts, 4, color );
		}


		public void fillRectangle( float x, float y, float width, float height, Color color )
		{
			var verts = new Vector2[4];
			verts[2] = new Vector2( x + width, y + height );
			verts[1] = new Vector2( x + width, y );
			verts[0] = new Vector2( x, y );
			verts[3] = new Vector2( x, y + height );

			fillPolygon( verts, 4, color );
		}


		public void drawRectangle( Rectangle rect, Color color )
		{
			drawRectangle( ref rect, color );
		}

	
		public void drawRectangle( ref Rectangle rect, Color color )
		{
			var verts = new Vector2[4];
			verts[0] = new Vector2( rect.Left, rect.Bottom );
			verts[1] = new Vector2( rect.Right, rect.Bottom );
			verts[2] = new Vector2( rect.Right, rect.Top );
			verts[3] = new Vector2( rect.Left, rect.Top );

			drawPolygon( verts, 4, color );
		}


		public void fillRectangle( ref Rectangle rect, Color color )
		{
			var verts = new Vector2[4];
			verts[0] = new Vector2( rect.Left, rect.Top );
			verts[1] = new Vector2( rect.Right, rect.Top );
			verts[2] = new Vector2( rect.Right, rect.Bottom );
			verts[3] = new Vector2( rect.Left, rect.Bottom );

			fillPolygon( verts, 4, color );
		}


		public void drawPolygon( Vector2[] vertices, int count, Color color, bool closed = true )
		{
			for( int i = 0; i < count - 1; i++ )
			{
				_primitiveBatch.addVertex( vertices[i], color, PrimitiveType.LineList );
				_primitiveBatch.addVertex( vertices[i + 1], color, PrimitiveType.LineList );
			}
			if( closed )
			{
				_primitiveBatch.addVertex( vertices[count - 1], color, PrimitiveType.LineList );
				_primitiveBatch.addVertex( vertices[0], color, PrimitiveType.LineList );
			}
		}


		public void fillPolygon( Vector2[] vertices, int count, Color color )
		{
			if( count == 2 )
			{
				drawPolygon( vertices, count, color );
				return;
			}

			for( int i = 1; i < count - 1; i++ )
			{
				_primitiveBatch.addVertex( vertices[0], color, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( vertices[i], color, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( vertices[i + 1], color, PrimitiveType.TriangleList );
			}
		}
			

		public void drawCircle( Vector2 center, float radius, Color color )
		{
			const double increment = Math.PI * 2.0 / CircleSegments;
			double theta = 0.0;

			for( int i = 0; i < CircleSegments; i++ )
			{
				var v1 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
				var v2 = center + radius * new Vector2( (float)Math.Cos( theta + increment ), (float)Math.Sin( theta + increment ) );

				_primitiveBatch.addVertex( v1, color, PrimitiveType.LineList );
				_primitiveBatch.addVertex( v2, color, PrimitiveType.LineList );

				theta += increment;
			}
		}
			

		public void fillCircle( Vector2 center, float radius, Color color )
		{
			const double increment = Math.PI * 2.0 / CircleSegments;
			double theta = 0.0;

			Vector2 v0 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
			theta += increment;

			for( int i = 1; i < CircleSegments - 1; i++ )
			{
				var v1 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
				var v2 = center + radius * new Vector2( (float)Math.Cos( theta + increment ), (float)Math.Sin( theta + increment ) );

				_primitiveBatch.addVertex( v0, color, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( v1, color, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( v2, color, PrimitiveType.TriangleList );

				theta += increment;
			}
		}


		public void drawLine( Vector2 start, Vector2 end, Color color )
		{
			_primitiveBatch.addVertex( start, color, PrimitiveType.LineList );
			_primitiveBatch.addVertex( end, color, PrimitiveType.LineList );
		}
			

		public void drawArrow( Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color )
		{
			// Draw connection segment between start- and end-point
			drawLine( start, end, color );

			// Precalculate halfwidth
			float halfWidth = width / 2;

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
			fillPolygon( verts, 3, color );

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
				fillPolygon( baseVerts, 4, color );
			}
		}
			
	}
}