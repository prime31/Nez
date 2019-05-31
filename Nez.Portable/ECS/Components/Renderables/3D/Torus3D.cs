using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Torus3D : GeometricPrimitive3D
	{
		[Range( 0.01f, 1f )]
		public float thickness
		{
			get => _thickness;
			set => generateTorusGeometry( value, _tessellation, _color );
		}

		[Range( 5, 300 )]
		public int tessellation
		{
			get => _tessellation;
			set => generateTorusGeometry( _thickness, value, _color );
		}

		float _thickness;
		int _tessellation;
		Color _color;


		public Torus3D() : this( 0.5f, 50, Color.Red )
		{}

		public Torus3D( float thickness, int tessellation, Color color )
		{
			generateTorusGeometry( thickness, tessellation, color );
		}

		void generateTorusGeometry( float torusThickness, int torusTessellation, Color torusColor )
		{
			_thickness = torusThickness;
			_tessellation = torusTessellation;
			_color = torusColor;

			_vertices.Clear();
			_indices.Clear();

			var diameter = 1f;

			// First we loop around the main ring of the torus. 
			for( int i = 0; i < torusTessellation; i++ )
			{
				var outerAngle = i * MathHelper.TwoPi / torusTessellation;

				// Create a transform matrix that will align geometry to slice perpendicularly though the current ring position. 
				var vertTransform = Matrix.CreateTranslation( diameter / 2, 0, 0 ) * Matrix.CreateRotationY( outerAngle );

				// Now we loop along the other axis, around the side of the tube. 
				for( var j = 0; j < torusTessellation; j++ )
				{
					var innerAngle = j * MathHelper.TwoPi / torusTessellation;

					var dx = (float)Math.Cos( innerAngle );
					var dy = (float)Math.Sin( innerAngle );

					// Create a vertex
					var normal = new Vector3( dx, dy, 0 );
					var pos = normal * torusThickness / 2;

					pos = Vector3.Transform( pos, vertTransform );
					normal = Vector3.TransformNormal( normal, vertTransform );

					addVertex( pos, torusColor, normal );

					// and create indices for two triangles. 
					int nextI = ( i + 1 ) % torusTessellation;
					int nextJ = ( j + 1 ) % torusTessellation;

					addIndex( i * torusTessellation + j );
					addIndex( i * torusTessellation + nextJ );
					addIndex( nextI * torusTessellation + j );

					addIndex( i * torusTessellation + nextJ );
					addIndex( nextI * torusTessellation + nextJ );
					addIndex( nextI * torusTessellation + j );
				}
			}

			initializePrimitive();
		}
	
	}
}
