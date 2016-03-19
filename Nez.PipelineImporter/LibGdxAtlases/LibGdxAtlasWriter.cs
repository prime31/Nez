using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace Nez.LibGdxAtlases
{
	[ContentTypeWriter]
	public class LibGdxAtlasDWriter : ContentTypeWriter<LibGdxAtlasProcessorResult>
	{
		protected override void Write( ContentWriter writer, LibGdxAtlasProcessorResult result )
		{
			var data = result.data;

			LibGdxAtlasProcessor.logger.LogMessage( "Writing {0} pages", data.pages.Count );
			writer.Write( data.pages.Count );
			foreach( var page in data.pages )
			{
				LibGdxAtlasProcessor.logger.LogMessage( "Writing page: {0}", page.textureFile );
				writer.Write( Path.GetFileNameWithoutExtension( page.textureFile ) );
				int count = 0;
				foreach( var region in data.regions )
				{
					if( region.page == page.textureFile )
						count++;
				}

				LibGdxAtlasProcessor.logger.LogMessage( "Writing {0} regions", count );
				writer.Write( count );
				foreach( var region in data.regions )
				{
					if( region.page == page.textureFile )
					{
						LibGdxAtlasProcessor.logger.LogMessage( "Writing region: {0}", region.name );
						writer.Write( region.name );
						writer.Write( region.sourceRectangle.x );
						writer.Write( region.sourceRectangle.y );
						writer.Write( region.sourceRectangle.w );
						writer.Write( region.sourceRectangle.h );

						if( region.splits == null )
						{
							writer.Write( false );
						}
						else
						{
							writer.Write( true );
							writer.Write( region.splits[0] );
							writer.Write( region.splits[1] );
							writer.Write( region.splits[2] );
							writer.Write( region.splits[3] );
							LibGdxAtlasProcessor.logger.LogMessage( "Writing splits for region: {0}", region.name );
						}
					}
				}
			}
		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( Nez.LibGdxAtlases.LibGdxAtlas ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( Nez.LibGdxAtlases.LibGdxAtlasReader ).AssemblyQualifiedName;
		}
	}
}

