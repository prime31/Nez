using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace Nez.LibGdxAtlases
{
	[ContentTypeWriter]
	public class LibGdxAtlasDWriter : ContentTypeWriter<LibGdxAtlasProcessorResult>
	{
		protected override void Write(ContentWriter writer, LibGdxAtlasProcessorResult result)
		{
			var data = result.Data;

			LibGdxAtlasProcessor.Logger.LogMessage("Writing {0} pages", data.Pages.Count);
			writer.Write(data.Pages.Count);
			foreach (var page in data.Pages)
			{
				LibGdxAtlasProcessor.Logger.LogMessage("Writing page: {0}", page.TextureFile);
				writer.Write(Path.GetFileNameWithoutExtension(page.TextureFile));
				int count = 0;
				foreach (var region in data.Regions)
				{
					if (region.Page == page.TextureFile)
						count++;
				}

				LibGdxAtlasProcessor.Logger.LogMessage("Writing {0} regions", count);
				writer.Write(count);
				foreach (var region in data.Regions)
				{
					if (region.Page == page.TextureFile)
					{
						LibGdxAtlasProcessor.Logger.LogMessage("Writing region: {0} {1}", region.Name,
							region.Index == -1 ? string.Empty : $"(index {region.Index})");
						writer.Write(region.Name);
						writer.Write(region.SourceRectangle.X);
						writer.Write(region.SourceRectangle.Y);
						writer.Write(region.SourceRectangle.W);
						writer.Write(region.SourceRectangle.H);

						if (region.Splits == null)
						{
							writer.Write(false);
						}
						else
						{
							writer.Write(true);
							writer.Write(region.Splits[0]);
							writer.Write(region.Splits[1]);
							writer.Write(region.Splits[2]);
							writer.Write(region.Splits[3]);
							LibGdxAtlasProcessor.Logger.LogMessage("Writing splits for region: {0}", region.Name);
						}

						if (region.Pads == null)
						{
							writer.Write(false);
						}
						else
						{
							writer.Write(true);
							writer.Write(region.Pads[0]);
							writer.Write(region.Pads[1]);
							writer.Write(region.Pads[2]);
							writer.Write(region.Pads[3]);
							LibGdxAtlasProcessor.Logger.LogMessage("Writing pads for region: {0}", region.Name);
						}

						writer.Write(region.Index);
					}
				}
			}
		}


		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(Nez.LibGdxAtlases.LibGdxAtlas).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(Nez.LibGdxAtlases.LibGdxAtlasReader).AssemblyQualifiedName;
		}
	}
}