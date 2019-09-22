using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nez.Tools.Atlases
{
	public static class AtlasMapExporter
	{
		public static string MapExtension => "lua";

		public static void Save(string filename, Dictionary<string, Rectangle> map, Dictionary<string, List<string>> animations, SpriteAtlasPacker.Config arguments )
		{
			var images = new List<string>( map.Keys);
			using (var writer = new StreamWriter(filename))
			{
				foreach (var image in images)
				{
					// get the destination rectangle
					var destination = map[image];

					// write out the destination rectangle for this bitmap
					writer.WriteLine("{0}\n\t{1},{2},{3},{4}\n\t{5},{6}", 
	                 	Path.GetFileNameWithoutExtension(image), 
	                 	destination.X, 
	                 	destination.Y, 
	                 	destination.Width, 
	                 	destination.Height,
                        arguments.OriginX.ToString(System.Globalization.CultureInfo.InvariantCulture), arguments.OriginY.ToString(System.Globalization.CultureInfo.InvariantCulture));
				}

				if ( animations.Count > 0)
				{
					writer.WriteLine();

					foreach (var kvPair in animations)
					{
						writer.WriteLine("{0}", kvPair.Key);
						writer.WriteLine("\t{0}", arguments.FrameRate);

						var indexes = new List<int>();
						foreach (var image in kvPair.Value)
						{
							indexes.Add(images.IndexOf(image));
						}
						writer.WriteLine("\t{0}", string.Join(",", indexes));
					}
				}
			}
		}
	
	}
}