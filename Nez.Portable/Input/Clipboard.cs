using System;
using System.Runtime.InteropServices;


namespace Nez
{
	/// <summary>
	/// prep for a proper multi-platform clipboard system. For now it just mocks the clipboard and will only work in-app
	/// </summary>
	public class Clipboard : IClipboard
	{
		static IClipboard _instance;
		
		//The Monogame.Framework.dll.config maps SDL2.dll to platform specific libraries
		[DllImport("SDL2.dll")]
		private static extern int SDL_SetClipboardText(string text);
		[DllImport("SDL2.dll")]
		private static extern string SDL_GetClipboardText();


		public static string getContents()
		{
			if( _instance == null )
				_instance = new Clipboard();
			return _instance.getContents();
		}


		public static void setContents( string text )
		{
			if( _instance == null )
				_instance = new Clipboard();
			_instance.setContents( text );
		}


		
		#region IClipboard implementation

		string IClipboard.getContents()
		{
			return SDL_GetClipboardText();
		}


		void IClipboard.setContents( string text )
		{
			SDL_SetClipboardText(text);
		}

		#endregion


	}
}

