using System;
using System.Runtime.InteropServices;

namespace Nez
{
	/// <summary>
	/// prep for a proper multi-platform clipboard system. For now it just mocks the clipboard and will only work in-app
	/// </summary>
	public class Clipboard : IClipboard
	{
		private static IClipboard _instance;

		[DllImport("SDL2.dll")]
		internal static extern void SDL_free(IntPtr memblock);

		// took this from c# sdl2 binding
		public static unsafe string UTF8_ToManaged(IntPtr s, bool freePtr = false)
		{
			if (s == IntPtr.Zero)
			{
				return null;
			}

			/* We get to do strlen ourselves! */
			byte* ptr = (byte*)s;
			while (*ptr != 0)
			{
				ptr++;
			}

			/* TODO: This #ifdef is only here because the equivalent
			 * .NET 2.0 constructor appears to be less efficient?
			 * Here's the pretty version, maybe steal this instead:
			 *
			string result = new string(
				(sbyte*) s, // Also, why sbyte???
				0,
				(int) (ptr - (byte*) s),
				System.Text.Encoding.UTF8
			);
			 * See the CoreCLR source for more info.
			 * -flibit
			 */
#if NETSTANDARD2_0
			/* Modern C# lets you just send the byte*, nice! */
			string result = System.Text.Encoding.UTF8.GetString(
				(byte*)s,
				(int)(ptr - (byte*)s)
			);
#else
			/* Old C# requires an extra memcpy, bleh! */
			int len = (int) (ptr - (byte*) s);
			if (len == 0)
			{
				return string.Empty;
			}
			char* chars = stackalloc char[len];
			int strLen = System.Text.Encoding.UTF8.GetChars((byte*) s, len, chars, len);
			string result = new string(chars, 0, strLen);
#endif

			/* Some SDL functions will malloc, we have to free! */
			if (freePtr)
			{
				SDL_free(s);
			}
			return result;
		}

		//The Monogame.Framework.dll.config maps SDL2.dll to platform specific libraries
		[DllImport("SDL2.dll")]
		private static extern int SDL_SetClipboardText(string text);

		[DllImport("SDL2.dll")]
		private static extern IntPtr SDL_GetClipboardText();

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
			return UTF8_ToManaged(SDL_GetClipboardText(), true);
		}

		void IClipboard.SetContents(string text)
		{
			SDL_SetClipboardText(text);
		}

		#endregion IClipboard implementation
	}
}