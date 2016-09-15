using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;
using System.ComponentModel;


namespace Nez.TexturePackerImporter
{
	[ContentProcessor( DisplayName = "TexturePacker Processor" )]
	public class TexturePackerProcessor : ContentProcessor<TexturePackerFile, TexturePackerFile>
	{
		public static ContentBuildLogger logger;

		[Description( "Enable this to automatically create animations based on texturepacker regions filenames metadata" )]
		[DefaultValue( false )]
		public bool parseAnimations { get; set; } = false;

		public override TexturePackerFile Process( TexturePackerFile input, ContentProcessorContext context )
		{
			logger = context.Logger;

			if( parseAnimations )
				processAnimations( input );

			return input;
		}


		void processAnimations( TexturePackerFile input )
		{
			input.spriteAnimationDetails = new Dictionary<string, List<int>>();

			for( var i = 0; i < input.regions.Count; i++ )
			{
				var region = input.regions[i];
				var rawFileName = region.filename.Split( '.' )[0];

				// texturepacker always ends the filename's frame with 4 digits
				var animationName = rawFileName.Substring( 0, rawFileName.Length - 4 );
				if( input.spriteAnimationDetails.ContainsKey( animationName ) == false )
				{
					logger.LogMessage( "found new animation [{0}]", animationName );
					input.spriteAnimationDetails.Add( animationName, new List<int>() );
				}

				logger.LogMessage( "adding frame {0} to animation {1} ", i, animationName );
				input.spriteAnimationDetails[animationName].Add( i );

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