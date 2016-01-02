using System;
using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	/// <summary>
	/// special case of a Polygon. When doing SAT collision checks we only need to check 2 axes instead of 8
	/// </summary>
	public class Box : Polygon
	{
		public float width;
		public float height;


		public Box( float width, float height ) : base( buildBox( width, height ) )
		{
			isBox = true;
			this.width = width;
			this.height = height;
		}


		public void updateBox( float width, float height )
		{
			points[0] = new Vector2( 0, 0 );
			points[1] = new Vector2( width, 0 );
			points[2] = new Vector2( width, height );
			points[3] = new Vector2( 0, height );
		}


		static Vector2[] buildBox( float width, float height )
		{
			var verts = new Vector2[4];

			verts[0] = new Vector2( 0, 0 );
			verts[1] = new Vector2( width, 0 );
			verts[2] = new Vector2( width, height );
			verts[3] = new Vector2( 0, height );

			return verts;
		}


		public override bool collidesWithShape( Shape other, out ShapeCollisionResult result )
		{
			// special, high-performance cases

			//if( other is Box )

			//if( other is Circle )

			// fallthrough to standard cases
			return base.collidesWithShape( other, out result );
		}

	}
}

