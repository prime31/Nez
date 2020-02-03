using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	/// <summary>
	/// Convert units between display and simulation units
	/// </summary>
	public static class FSConvert
	{
		/// <summary>
		/// converts simulation (meters) to display (pixels)
		/// </summary>
		public static float SimToDisplay = 100f;

		/// <summary>
		/// converts display (pixels) to simulation (meters)
		/// </summary>
		public static float DisplayToSim = 1 / SimToDisplay;


		public static void SetDisplayUnitToSimUnitRatio(float displayUnitsPerSimUnit)
		{
			SimToDisplay = displayUnitsPerSimUnit;
			DisplayToSim = 1 / displayUnitsPerSimUnit;
		}

		public static float ToDisplayUnits(float simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static float ToDisplayUnits(int simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static Vector2 ToDisplayUnits(Vector2 simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static void ToDisplayUnits(ref Vector2 simUnits, out Vector2 displayUnits)
		{
			Vector2.Multiply(ref simUnits, SimToDisplay, out displayUnits);
		}

		public static Vector3 ToDisplayUnits(Vector3 simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static Vector2 ToDisplayUnits(float x, float y)
		{
			return new Vector2(x, y) * SimToDisplay;
		}

		public static void ToDisplayUnits(float x, float y, out Vector2 displayUnits)
		{
			displayUnits = Vector2.Zero;
			displayUnits.X = x * SimToDisplay;
			displayUnits.Y = y * SimToDisplay;
		}

		public static float ToSimUnits(float displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static float ToSimUnits(double displayUnits)
		{
			return (float) displayUnits * DisplayToSim;
		}

		public static float ToSimUnits(int displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static Vector2 ToSimUnits(Vector2 displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static Vector3 ToSimUnits(Vector3 displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static void ToSimUnits(ref Vector2 displayUnits, out Vector2 simUnits)
		{
			Vector2.Multiply(ref displayUnits, DisplayToSim, out simUnits);
		}

		public static Vector2 ToSimUnits(float x, float y)
		{
			return new Vector2(x, y) * DisplayToSim;
		}

		public static Vector2 ToSimUnits(double x, double y)
		{
			return new Vector2((float) x, (float) y) * DisplayToSim;
		}

		public static void ToSimUnits(float x, float y, out Vector2 simUnits)
		{
			simUnits = Vector2.Zero;
			simUnits.X = x * DisplayToSim;
			simUnits.Y = y * DisplayToSim;
		}
	}
}