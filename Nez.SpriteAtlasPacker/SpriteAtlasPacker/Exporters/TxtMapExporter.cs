using System.Collections.Generic;
using System.Drawing;
using System.IO;

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

			using (StreamWriter writer = new StreamWriter(filename))
			{
				foreach (var image in outputFiles)
				{
					// get the destination rectangle
					Rectangle destination = map[image];

					// write out the destination rectangle for this bitmap
					writer.WriteLine(string.Format(
	                 	"{0} = {1} {2} {3} {4}", 
	                 	Path.GetFileNameWithoutExtension(image), 
	                 	destination.X, 
	                 	destination.Y, 
	                 	destination.Width, 
	                 	destination.Height));
				}
			}
		}
	}
}