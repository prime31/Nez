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
			var assetName = Path.GetFileNameWithoutExtension( data.metadata.image );

			writer.Write( assetName );
			writer.Write( data.regions.Count );

			foreach( var region in data.regions )
			{
				var regionName = region.filename.Replace( Path.GetExtension( region.filename ), string.Empty );
				TexturePackerProcessor.logger.LogMessage( "writing region: {0}", regionName );

				writer.Write( regionName );
				writer.Write( region.frame.x );
				writer.Write( region.frame.y );
				writer.Write( region.frame.width );
				writer.Write( region.frame.height );

				// no use to write as double, since Subtexture.center holds floats
				writer.Write( (float)region.pivotPoint.x );
				writer.Write( (float)region.pivotPoint.y );
			}

			writer.WriteObject( data.spriteAnimationDetails );
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