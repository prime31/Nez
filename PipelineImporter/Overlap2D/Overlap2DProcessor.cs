using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.Overlap2D.Runtime;
using System.IO;
using Newtonsoft.Json;


namespace Nez.Overlap2D
{
	[ContentProcessor( DisplayName = "Overlap2D Processor" )]
	public class Overlap2DProcessor : ContentProcessor<SceneVO, Overlap2DProcessorResult>
	{
		public override Overlap2DProcessorResult Process( SceneVO scene, ContentProcessorContext context )
		{
			//context.Logger.LogMessage( "scene: {0}", scene );
			return new Overlap2DProcessorResult( scene );
		}
	}
}
