using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;


namespace Nez
{
	public static class GestureSampleExt
	{
		public static Vector2 ScaledPosition(this GestureSample gestureSample)
		{
			return Input.ScaledPosition(gestureSample.Position);
		}

		public static Vector2 ScaledPosition2(this GestureSample gestureSample)
		{
			return Input.ScaledPosition(gestureSample.Position2);
		}
	}
}