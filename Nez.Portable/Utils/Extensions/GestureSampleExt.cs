using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;


namespace Nez
{
	public static class GestureSampleExt
	{
		public static Vector2 scaledPosition( this GestureSample gestureSample )
		{
			return Input.scaledPosition( gestureSample.Position );
		}

		public static Vector2 scaledPosition2( this GestureSample gestureSample )
		{
			return Input.scaledPosition( gestureSample.Position2 );
		}
	}
}
