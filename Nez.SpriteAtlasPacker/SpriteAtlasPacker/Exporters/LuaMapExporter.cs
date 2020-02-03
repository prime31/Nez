using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Nez.Tools.Atlases
{
	public static class LuaMapExporter
	{
		public static string MapExtension => "atlas";

		public static void Save(string filename, Dictionary<string, Rectangle> map, Dictionary<string, List<string>> animations, int atlasWidth, int atlasHeight, SpriteAtlasPacker.Config arguments)
		{
			var images = ImagesNotInAnimations(map.Keys.ToArray(), animations);
			using (var writer = new StreamWriter(filename))
			{
				writer.WriteLine("return {");

				writer.WriteLine($"\ttexture = love.graphics.newImage('{arguments.AtlasOutputFile}'),");

				if(images.Length > 0)
				{
					writer.WriteLine("\timages = {");
					foreach( var image in images )
					{
						// get the destination rectangle
						var rect = map[image];
						writer.WriteLine($"\t\t['{Path.GetFileNameWithoutExtension(image)}'] = love.graphics.newQuad({rect.X}, {rect.Y}, {rect.Width}, {rect.Height}, {atlasWidth}, {atlasHeight}),");
					}
					writer.WriteLine("\t},");
				}

				if (animations.Count > 0)
				{
					writer.WriteLine("\tanimations = {");

					foreach (var kvPair in animations)
					{
						writer.WriteLine($"\t\t['{kvPair.Key}'] = {{");
						foreach (var image in kvPair.Value)
						{
							var rect = map[image];
							writer.WriteLine($"\t\t\tlove.graphics.newQuad({rect.X}, {rect.Y}, {rect.Width}, {rect.Height}, {atlasWidth}, {atlasHeight})," );
						}
						writer.WriteLine("\t\t},");
					}

					writer.WriteLine("\t}");
				}
				writer.WriteLine("}");
			}
		}

		static string[] ImagesNotInAnimations(string[] allImages, Dictionary<string, List<string>> animations)
		{
			var animationImages = new List<string>();
			foreach( var kv in animations )
				animationImages.AddRange( kv.Value );

			return allImages.Where(i => !animationImages.Contains(i)).ToArray();
		}

	}
}