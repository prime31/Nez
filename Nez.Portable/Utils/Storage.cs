using System;
using System.IO;
using System.Runtime.InteropServices;


namespace Nez
{
	public static class Storage
	{
		/// <summary>
		/// attempts to come up with a root folder you can use to save your data accross all (non-console) platforms
		/// </summary>
		/// <returns>The storage root.</returns>
		public static string GetStorageRoot()
		{
			// Generate the path of the game's savefolder
			var exeName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName).Replace(".vshost", "");

			// we need to check for macOS in a different manner since Environment.OSVersion.Platform is hardcoded to return Unix
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				var osConfigDir = Environment.GetEnvironmentVariable("HOME");
				if (string.IsNullOrEmpty(osConfigDir))
					return "."; // Oh well.

				return Path.Combine(osConfigDir, "Library/Application Support", exeName);
			}

			// resort to using the PlatformId
			var platformId = Environment.OSVersion.Platform;

			// Get the OS save folder, append the EXE name
			if (platformId == PlatformID.Unix)
			{
				// Assuming a non-macOS Unix platform will follow the XDG. Which it should.
				var osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (string.IsNullOrEmpty(osConfigDir))
				{
					osConfigDir = Environment.GetEnvironmentVariable("HOME");
					if (string.IsNullOrEmpty(osConfigDir))
						return "."; // Oh well.

					osConfigDir += "/.local/share";
				}

				return Path.Combine(osConfigDir, exeName);
			}

			// windows
			if (platformId == PlatformID.Win32NT || platformId == PlatformID.Win32S ||
			    platformId == PlatformID.Win32Windows || platformId == PlatformID.WinCE)
			{
				var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				return Path.Combine(docs, "SavedGames", exeName);
			}

#if FNA
			return SDL2.SDL.SDL_GetPrefPath( null, exeName );
#else
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), exeName);
#endif
		}
	}
}