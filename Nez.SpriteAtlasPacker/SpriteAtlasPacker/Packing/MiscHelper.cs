using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nez.Tools.Atlases
{
	public static class MiscHelper
	{
		// the valid extensions for images
		public static readonly string[] AllowedImageExtensions = new[] { "png", "jpg", "bmp", "gif" };

		// determines if a file is an image we accept
		public static bool IsImageFile(string file)
		{
			if (!File.Exists(file))
				return false;

			// ToLower for string comparisons
			string fileLower = file.ToLower();

			// see if the file ends with one of our valid extensions
			foreach (var ext in AllowedImageExtensions)
				if (fileLower.EndsWith(ext))
					return true;
			return false;
		}

		// stolen from http://en.wikipedia.org/wiki/Power_of_two#Algorithm_to_find_the_next-highest_power_of_two
		public static int FindNextPowerOfTwo(int k)
		{
			k--;
			for (int i = 1; i < sizeof(int) * 8; i <<= 1)
				k = k | k >> i;
			return k + 1;
		}

		// common prefix path for all the given files, e.g. for
		// C:\dir1\file1.png
		// C:\dir1\file2.jpg
		// will return "C:\dir1\"
		public static string GetCommonPath(IEnumerable<string> files)
		{
			if (files == null || !files.Any())
				throw new ArgumentNullException(nameof(files),
					"No file is given. Provide at least one.");

			var orderedFiles = files.OrderBy(file => file.Length);
			var shortestFile = orderedFiles.First();
			int lastCommonDirectoryNameStart = 0;
			for (int i = 0; i < shortestFile.Length; i++)
			{
				char currentChar = shortestFile[i];
				bool isThisCharInOtherFiles = files.All(file => file[i] == currentChar);
				if (!isThisCharInOtherFiles)
					break;

				bool isPathSeparator = currentChar == '\\';
				if (isPathSeparator)
					lastCommonDirectoryNameStart = i;
			}
			return shortestFile.Substring(0, lastCommonDirectoryNameStart + 1);
		}
	}
}
