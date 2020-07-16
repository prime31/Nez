using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Vector3Ext
	{
		/// <summary>
		/// returns a Vector2 ignoring the z component
		/// </summary>
		/// <returns>The vector2.</returns>
		/// <param name="vec">Vec.</param>
		public static Vector2 ToVector2(this Vector3 vec)
		{
			return new Vector2(vec.X, vec.Y);
		}
	}
}