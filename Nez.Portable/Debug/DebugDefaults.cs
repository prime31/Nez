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
			public static Color DebugText = Color.White;

			public static Color ColliderBounds = Color.White * 0.3f;
			public static Color ColliderEdge = Color.DarkRed;
			public static Color ColliderPosition = Color.Yellow;
			public static Color ColliderCenter = Color.Red;

			public static Color RenderableBounds = Color.Yellow;
			public static Color RenderableCenter = Color.DarkOrchid;

			public static Color VerletParticle = new Color(220, 52, 94);
			public static Color VerletConstraintEdge = new Color(67, 62, 54);
		}


		public static class Size
		{
			public static int LineSizeMultiplier =>
				System.Math.Max(Mathf.CeilToInt((float) Core.Scene.SceneRenderTargetSize.X / Screen.Width),
					1);
		}
	}
}