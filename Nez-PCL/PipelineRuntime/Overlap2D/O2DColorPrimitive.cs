using System;
using Microsoft.Xna.Framework;


namespace Nez.Overlap2D
{
	public class O2DColorPrimitive : O2DMainItem
	{
		public Vector2[] polygon;


		public Vector3[] getPolygon3D()
		{
			var poly3d = new Vector3[polygon.Length + 1];

			for( var i = 0; i < polygon.Length; i++ )
				poly3d[i] = new Vector3( polygon[i], 0 );

			poly3d[polygon.Length] = new Vector3( polygon[0], 0 );

			return poly3d;
		}
	}
}

