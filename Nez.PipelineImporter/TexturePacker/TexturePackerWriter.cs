using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace Nez.TexturePackerImporter
{
	[ContentTypeWriter]
	public class TexturePackerWriter : ContentTypeWriter<TexturePackerFile>
	{
		protected override void Write( ContentWriter writer, TexturePackerFile data )
		{
			var metadata = data.metadata;

			var assetName = Path.GetFileNameWithoutExtension( metadata.image );

			writer.Write( assetName );
			writer.Write( data.regions.Count );

			foreach( var region in data.regions )
			{
				var regionName = Path.GetFileNameWithoutExtension( region.filename );

				writer.Write( regionName );
				writer.Write( region.frame.x );
				writer.Write( region.frame.y );
				writer.Write( region.frame.width );
				writer.Write( region.frame.height );
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