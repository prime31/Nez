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
		public static float simToDisplay = 100f;

		/// <summary>
		/// converts display (pixels) to simulation (meters)
		/// </summary>
		public static float displayToSim = 1 / simToDisplay;


		public static void setDisplayUnitToSimUnitRatio( float displayUnitsPerSimUnit )
		{
			simToDisplay = displayUnitsPerSimUnit;
			displayToSim = 1 / displayUnitsPerSimUnit;
		}

		public static float toDisplayUnits( float simUnits )
		{
			return simUnits * simToDisplay;
		}

		public static float toDisplayUnits( int simUnits )
		{
			return simUnits * simToDisplay;
		}

		public static Vector2 toDisplayUnits( Vector2 simUnits )
		{
			return simUnits * simToDisplay;
		}

		public static void toDisplayUnits( ref Vector2 simUnits, out Vector2 displayUnits )
		{
			Vector2.Multiply( ref simUnits, simToDisplay, out displayUnits );
		}

		public static Vector3 toDisplayUnits( Vector3 simUnits )
		{
			return simUnits * simToDisplay;
		}

		public static Vector2 toDisplayUnits( float x, float y )
		{
			return new Vector2( x, y ) * simToDisplay;
		}

		public static void toDisplayUnits( float x, float y, out Vector2 displayUnits )
		{
			displayUnits = Vector2.Zero;
			displayUnits.X = x * simToDisplay;
			displayUnits.Y = y * simToDisplay;
		}

		public static float toSimUnits( float displayUnits )
		{
			return displayUnits * displayToSim;
		}

		public static float toSimUnits( double displayUnits )
		{
			return (float)displayUnits * displayToSim;
		}

		public static float toSimUnits( int displayUnits )
		{
			return displayUnits * displayToSim;
		}

		public static Vector2 toSimUnits( Vector2 displayUnits )
		{
			return displayUnits * displayToSim;
		}

		public static Vector3 toSimUnits( Vector3 displayUnits )
		{
			return displayUnits * displayToSim;
		}

		public static void toSimUnits( ref Vector2 displayUnits, out Vector2 simUnits )
		{
			Vector2.Multiply( ref displayUnits, displayToSim, out simUnits );
		}

		public static Vector2 toSimUnits( float x, float y )
		{
			return new Vector2( x, y ) * displayToSim;
		}

		public static Vector2 toSimUnits( double x, double y )
		{
			return new Vector2( (float)x, (float)y ) * displayToSim;
		}

		public static void toSimUnits( float x, float y, out Vector2 simUnits )
		{
			simUnits = Vector2.Zero;
			simUnits.X = x * displayToSim;
			simUnits.Y = y * displayToSim;
		}

	}
}