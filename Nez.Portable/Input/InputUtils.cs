using Microsoft.Xna.Framework.Input;


namespace Nez
{
	public static class InputUtils
	{
		public static bool IsMac;
		public static bool IsWindows;
		public static bool IsLinux;


		static InputUtils()
		{
			IsMac = true;
		}


		public static bool IsShiftDown()
		{
			return Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
		}


		public static bool IsAltDown()
		{
			return Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);
		}


		public static bool IsControlDown()
		{
			if (IsMac)
				return Input.IsKeyDown(Keys.LeftWindows) || Input.IsKeyDown(Keys.RightWindows);

			return Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl);
		}
	}
}