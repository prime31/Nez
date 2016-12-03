using Microsoft.Xna.Framework;


namespace Nez
{
	public static partial class Debug
	{
		/// <summary>
		/// we store all the default colors for various systems here such as collider debug rendering, Debug.drawText and others. The naming
		/// convention is CLASS-THING where possible to make it clear where it is used.
		/// </summary>
		public static class Colors
		{
			public static Color debugText = Color.White;

			public static Color colliderBounds = Color.White * 0.3f;
			public static Color colliderEdge = Color.DarkRed;
			public static Color colliderPosition = Color.Yellow;
			public static Color colliderCenter = Color.Red;

			public static Color renderableBounds = Color.Yellow;
			public static Color renderableCenter = Color.DarkOrchid;

			public static Color verletParticle = new Color( 220, 52, 94 );
			public static Color verletConstraintEdge = new Color( 67, 62, 54 );

		}


		public static class Size
		{
			public static int lineSizeMultiplier
			{
				get
				{
					return System.Math.Max( Mathf.ceilToInt( (float)Core.scene.sceneRenderTargetSize.X / Screen.width ), 1 );
				}
			}
		}
	}
}
