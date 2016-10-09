using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// we store all the default colors for various systems here such as collider debug rendering, Debug.drawText and others. The naming
	/// convention is CLASS-THING where possible to make it clear where it is used.
	/// </summary>
	public static class DefaultColors
	{
		public static Color debugText = Color.White;

		public static Color colliderBounds = Color.White * 0.3f;
		public static Color colliderEdge = Color.DarkRed;
		public static Color colliderPosition = Color.Yellow;
		public static Color colliderCenter = Color.Red;

		public static Color verletParticle = new Color( 220, 52, 94 );
		public static Color verletConstraintEdge = new Color( 67, 62, 54 );

	}
}
