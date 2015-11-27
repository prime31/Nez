using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace Nez.TexturePackerImporter
{
	[ContentTypeWriter]
	public class TexturePackerWriter : ContentTypeWriter<TexturePackerProcessorResult>
	{
		protected override void Write( ContentWriter writer, TexturePackerProcessorResult result )
		{
			var data = result.Data;
			var metadata = data.Metadata;

			var assetName = Path.GetFileNameWithoutExtension( metadata.image );

			writer.Write( assetName );
			writer.Write( data.Regions.Count );

			foreach( var region in data.Regions )
			{
				var regionName = Path.GetFileNameWithoutExtension( region.Filename );

				writer.Write( regionName );
				writer.Write( region.Frame.X );
				writer.Write( region.Frame.Y );
				writer.Write( region.Frame.Width );
				writer.Write( region.Frame.Height );
			}
		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( Nez.TextureAtlases.TexturePackerAtlas ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( Nez.TextureAtlases.TexturePackerAtlasReader ).AssemblyQualifiedName;
		}
	}
}