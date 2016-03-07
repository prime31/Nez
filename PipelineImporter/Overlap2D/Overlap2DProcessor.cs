using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.Overlap2D.Runtime;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;


namespace Nez.Overlap2D
{
	[ContentProcessor( DisplayName = "Overlap2D Processor" )]
	public class Overlap2DProcessor : ContentProcessor<SceneVO,SceneVO>
	{
		public override SceneVO Process( SceneVO scene, ContentProcessorContext context )
		{
			// deal with converting renderLayer into a layerDepth. first we need to find the max zIndex
			var maxIndex = scene.composite.findMaxZindex() + 1;
			scene.composite.setLayerDepthRecursively( (float)maxIndex, null );

			context.Logger.LogMessage( "converting zIndex to layerDepth. Max zIndex: {0}", maxIndex );

			return scene;
		}

	}
}
