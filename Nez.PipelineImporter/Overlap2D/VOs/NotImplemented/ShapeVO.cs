using System;
using Microsoft.Xna.Framework;


namespace Nez.Overlap2D.Runtime
{
	public class ShapeVO
	{
		public Vector2[][] polygons;
		public Circle[] circles;


		public static ShapeVO createRect( float width, float height )
		{
			ShapeVO vo = new ShapeVO();
			vo.polygons = new Vector2[1][];

			vo.polygons[0] = new Vector2[4];
			vo.polygons[0][0] = new Vector2( 0, 0 );
			vo.polygons[0][1] = new Vector2( 0, height );
			vo.polygons[0][2] = new Vector2( width, height );
			vo.polygons[0][3] = new Vector2( width, 0 );

			return vo;
		}
	}
}

