using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace Nez.Tools.Atlases
{
	public static class AtlasMapOriginsImporter
	{

		/// <summary>
		/// parses a .atlas file and extract sprite origins
		/// </summary>
		public static Dictionary<string, Vector2> ImportFromFile(string dataFile)
		{
			var origins = new Dictionary<string, Vector2>();

			var commaSplitter = new char[] { ',' };

			string line = null;
			using (var streamFile = File.OpenRead(dataFile))
			{
				using (var stream = new StreamReader(streamFile))
				{
					while ((line = stream.ReadLine()) != null)
					{
						// once we hit an empty line we are done parsing sprites and origins
						if (string.IsNullOrWhiteSpace(line))
							break;

						var name = line;

						// source rect
						line = stream.ReadLine();

						// origin
						line = stream.ReadLine();
						var lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);
						var origin = new Vector2(float.Parse(lineParts[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(lineParts[1], System.Globalization.CultureInfo.InvariantCulture));

						origins[name] = origin;
					}
				}
			}
			return origins;
		}
	}
}
