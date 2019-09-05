using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;
using System.ComponentModel;


namespace Nez.TexturePackerImporter
{
	[ContentProcessor(DisplayName = "TexturePacker Processor")]
	public class TexturePackerProcessor : ContentProcessor<TexturePackerFile, TexturePackerFile>
	{
		public static ContentBuildLogger Logger;

		[Description(
			"Enable this to automatically create animations based on texturepacker regions filenames metadata")]
		[DefaultValue(false)]
		public bool ParseAnimations { get; set; } = false;

		public override TexturePackerFile Process(TexturePackerFile input, ContentProcessorContext context)
		{
			Logger = context.Logger;

			if (ParseAnimations)
				ProcessAnimations(input);

			return input;
		}


		void ProcessAnimations(TexturePackerFile input)
		{
			input.SpriteAnimationDetails = new Dictionary<string, List<int>>();

			for (var i = 0; i < input.Regions.Count; i++)
			{
				var region = input.Regions[i];
				var rawFileName = region.Filename.Split('.')[0];

				// texturepacker always ends the filename's frame with 4 digits
				var animationName = rawFileName.Substring(0, rawFileName.Length - 4);
				if (input.SpriteAnimationDetails.ContainsKey(animationName) == false)
				{
					Logger.LogMessage("found new animation [{0}]", animationName);
					input.SpriteAnimationDetails.Add(animationName, new List<int>());
				}

				Logger.LogMessage("adding frame {0} to animation {1} ", i, animationName);
				input.SpriteAnimationDetails[animationName].Add(i);

				// TODO add support for "Detect identical sprites"

				#region WIP detect identical sprites

				/*
                string frameIDString = rawFileName.Substring( rawFileName.Length - 4, 4 );

                int frameID = -1;
                if ( int.TryParse( frameIDString, out frameID ) )
                {
                    logger.LogMessage( "adding frame {0} to animation {1} ", frameID, animationName );
                    input.spriteAnimationDetails[ animationName ].Add( frameID );
                }
                else
                {
                    logger.LogMessage( "animation {0} does not end in 4 digits, skipping animation parsing for this region" + animationName);
                }*/

				#endregion
			}
		}
	}
}