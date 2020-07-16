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


		public static string GetContents()
		{
			if (_instance == null)
				_instance = new Clipboard();
			return _instance.GetContents();
		}


		public static void SetContents(string text)
		{
			if (_instance == null)
				_instance = new Clipboard();
			_instance.SetContents(text);
		}


		#region IClipboard implementation

		string IClipboard.GetContents()
		{
			return SDL_GetClipboardText();
		}


		void IClipboard.SetContents(string text)
		{
			SDL_SetClipboardText(text);
		}

		#endregion
	}
}