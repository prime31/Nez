using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// while technically not a ray (rays are just start and direction) it does double duty as both a line and a ray.
	/// </summary>
	public struct Ray2D
	{
		public Vector2 Start;
		public Vector2 End;
		public Vector2 Direction;


		public Ray2D(Vector2 position, Vector2 end)
		{
			Start = position;
			End = end;
			Direction = end - Start;
		}
	}
}