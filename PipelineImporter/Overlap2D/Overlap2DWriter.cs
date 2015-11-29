using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.Overlap2D.Runtime;


namespace Nez.Overlap2D
{
	[ContentTypeWriter]
	public class Overlap2DWriter : ContentTypeWriter<Overlap2DProcessorResult>
	{
		protected override void Write( ContentWriter writer, Overlap2DProcessorResult result )
		{
			Overlap2DProcessor.logger.LogMessage( "qui" );
			var scene = result.scene;
			Overlap2DProcessor.logger.LogMessage( "{0}", scene.sceneName );
			writer.Write( scene.sceneName );
			Overlap2DProcessor.logger.LogMessage( "prova" );
			Overlap2DProcessor.logger.LogMessage( "" + scene.composite.sImages );
			Overlap2DProcessor.logger.LogMessage( "Processing {0} simple images", scene.composite.sImages.Count );
			writer.Write( scene.composite.sImages.Count );
			foreach( var si in scene.composite.sImages )
			{
				Overlap2DProcessor.logger.LogMessage( "Processing image {0}", si.imageName );
				writer.Write( si.uniqueId );
				writer.Write( si.itemIdentifier );
				writer.Write( si.itemName );
				writer.Write( si.x );
				writer.Write( si.y );
				writer.Write( si.scaleX );
				writer.Write( si.scaleY );
				writer.Write( si.originX );
				writer.Write( si.originY );
				writer.Write( si.rotation );
				writer.Write( si.zIndex );
				writer.Write( si.layerName );
				writer.Write( si.imageName );
			}

		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( Nez.Overlap2D.O2DScene ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( Nez.Overlap2D.O2DSceneReader ).AssemblyQualifiedName;
		}
	}
}

