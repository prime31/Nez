using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Torus3D : GeometricPrimitive3D
	{
		public Torus3D( float thickness, int tessellation, Color color )
		{
			var diameter = 1f;

			// First we loop around the main ring of the torus. 
			for( int i = 0; i < tessellation; i++ )
			{
				float outerAngle = i * MathHelper.TwoPi / tessellation;

				// Create a transform matrix that will align geometry to slice perpendicularly though the current ring position. 
				var vertTransform = Matrix.CreateTranslation( diameter / 2, 0, 0 ) * Matrix.CreateRotationY( outerAngle );

				// Now we loop along the other axis, around the side of the tube. 
				for( var j = 0; j < tessellation; j++ )
				{
					var innerAngle = j * MathHelper.TwoPi / tessellation;

					var dx = (float)Math.Cos( innerAngle );
					var dy = (float)Math.Sin( innerAngle );

					// Create a vertex. 
					var normal = new Vector3( dx, dy, 0 );
					var pos = normal * thickness / 2;

					pos = Vector3.Transform( pos, vertTransform );
					normal = Vector3.TransformNormal( normal, vertTransform );

					addVertex( pos, color, normal );

					// and create indices for two triangles. 
					int nextI = ( i + 1 ) % tessellation;
					int nextJ = ( j + 1 ) % tessellation;

					addIndex( i * tessellation + j );
					addIndex( i * tessellation + nextJ );
					addIndex( nextI * tessellation + j );

					addIndex( i * tessellation + nextJ );
					addIndex( nextI * tessellation + nextJ );
					addIndex( nextI * tessellation + j );
				}
			}

			initializePrimitive();
		}
	}
}
