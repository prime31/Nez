using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class PointExt
	{
		#if FNA
		/// <summary>
		/// Gets a <see cref="Vector2"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Vector2"/> representation for this object.</returns>
		public static Vector2 ToVector2( this Point self )
		{
			return new Vector2( (float)self.X, (float)self.Y );
		}
		#endif
	}
}

