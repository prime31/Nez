using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Nez.Tools.Atlases
{
	public static class AtlasMapExporter
	{
		public static string MapExtension => "lua";

		public static void Save(string filename, Dictionary<string, Rectangle> map, Dictionary<string, List<string>> animations, Dictionary<string, Vector2> origins, SpriteAtlasPacker.Config arguments)
		{
			var images = new List<string>(map.Keys);

			IDictionary<string, string> imagesNames;
			if (arguments.WritePaths)
			{
				string commonPath = MiscHelper.GetCommonPath(images);
				imagesNames = images.ToDictionary(k => k, v => v.Substring(commonPath.Length));
			}
			else
			{
				imagesNames = images.ToDictionary(k => k, v => Path.GetFileNameWithoutExtension(v));
			}

			using (var writer = new StreamWriter(filename))
			{
				writer.NewLine = arguments.LF ? "\n" : "\r\n";

				foreach (var image in images)
				{
					writer.WriteLine(imagesNames[image]);

					// write out the destination rectangle for this bitmap
					var destination = map[image];
					writer.WriteLine("\t{0},{1},{2},{3}",
						destination.X,
						destination.Y,
						destination.Width,
						destination.Height);

					// write images origins
					if (!arguments.NoOrigins)
					{
						Vector2 origin = new Vector2(arguments.OriginX, arguments.OriginY);
						if (origins != null)
						{
							var name = imagesNames[image];
							if (origins.ContainsKey(name))
								origin = origins[name];
						}
						writer.WriteLine("\t{0},{1}",
							origin.X.ToString(System.Globalization.CultureInfo.InvariantCulture),
							origin.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
					}
				}

				if (animations.Count > 0)
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