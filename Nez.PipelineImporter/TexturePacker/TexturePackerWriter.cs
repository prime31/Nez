using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace Nez.TexturePackerImporter
{
	[ContentTypeWriter]
	public class TexturePackerWriter : ContentTypeWriter<TexturePackerFile>
	{
		protected override void Write(ContentWriter writer, TexturePackerFile data)
		{
			var assetName = Path.GetFileNameWithoutExtension(data.Metadata.Image);

			writer.Write(assetName);
			writer.Write(data.Regions.Count);

			foreach (var region in data.Regions)
			{
				var regionName = region.Filename.Replace(Path.GetExtension(region.Filename), string.Empty);
				TexturePackerProcessor.Logger.LogMessage("writing region: {0}", regionName);

				writer.Write(regionName);
				writer.Write(region.Frame.X);
				writer.Write(region.Frame.Y);
				writer.Write(region.Frame.Width);
				writer.Write(region.Frame.Height);

				// no use to write as double, since Subtexture.center holds floats
				writer.Write((float) region.PivotPoint.X);
				writer.Write((float) region.PivotPoint.Y);
			}

			writer.WriteObject(data.SpriteAnimationDetails);
		}


		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(Nez.TextureAtlases.TexturePackerAtlas).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(Nez.TextureAtlases.TexturePackerAtlasReader).AssemblyQualifiedName;
		}
	}
}