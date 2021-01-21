using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Nez.Tools.Atlases
{
	public static class TxtMapExporter
	{
		public static string MapExtension => "txt";

		public static void Save(string filename, Dictionary<string, Rectangle> map, Dictionary<string, List<string>> animations, SpriteAtlasPacker.Config arguments )
		{
			// copy the files list and sort alphabetically
			var keys = new string[map.Count];
			map.Keys.CopyTo(keys, 0);
			List<string> outputFiles = new List<string>(keys);
			outputFiles.Sort();

			// compute the names of images to write
			IDictionary<string, string> imagesNames;
			if (arguments.WritePaths)
			{
				string commonPath = MiscHelper.GetCommonPath(outputFiles);
				imagesNames = outputFiles.ToDictionary(k => k, v => v.Substring(commonPath.Length));
			}
			else
			{
				imagesNames = outputFiles.ToDictionary(k => k, v => Path.GetFileNameWithoutExtension(v));
			}

			using (StreamWriter writer = new StreamWriter(filename))
			{
				writer.NewLine = arguments.LF ? "\n" : "\r\n";

				foreach (var image in outputFiles)
				{
					// get the destination rectangle
					Rectangle destination = map[image];

					// write out the destination rectangle for this bitmap
					writer.WriteLine(string.Format(
	                 	"{0} = {1} {2} {3} {4}", 
	                 	imagesNames[image], 
	                 	destination.X, 
	                 	destination.Y, 
	                 	destination.Width, 
	                 	destination.Height));
				}
			}
		}
	}
}