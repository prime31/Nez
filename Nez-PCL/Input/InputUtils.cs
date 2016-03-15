using System;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	public static class InputUtils
	{
		public static bool isMac;
		public static bool isWindows;
		public static bool isLinux;


		static InputUtils()
		{
			isMac = true;
		}


		public static bool shiftDown()
		{
			return Input.isKeyDown( Keys.LeftShift ) || Input.isKeyDown( Keys.RightShift );
		}


		public static bool altDown()
		{
			return Input.isKeyDown( Keys.LeftAlt ) || Input.isKeyDown( Keys.RightAlt );
		}


		public static bool controlDown()
		{
			if( isMac )
				return Input.isKeyDown( Keys.LeftWindows ) || Input.isKeyDown( Keys.RightWindows );
			
			return Input.isKeyDown( Keys.LeftControl ) || Input.isKeyDown( Keys.RightControl );
		}
	}
}

