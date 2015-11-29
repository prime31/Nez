using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.Overlap2D.Runtime;

namespace Nez.Overlap2D
{
	[ContentProcessor( DisplayName = "Overlap2D Processor" )]
	public class Overlap2DProcessor : ContentProcessor<SceneVO, Overlap2DProcessorResult>
	{
		public static ContentBuildLogger logger;

		public override Overlap2DProcessorResult Process( SceneVO input, ContentProcessorContext context )
		{
			logger = context.Logger;
			try
			{
				var output = new Overlap2DProcessorResult { scene = input };
				return output;
			}
			catch( Exception ex )
			{
				context.Logger.LogMessage( "Error {0}", ex );
				throw;
			}
		}
	}
}
