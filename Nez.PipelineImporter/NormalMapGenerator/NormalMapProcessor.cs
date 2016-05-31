using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Nez.PipelineImporter;
using System.ComponentModel;
using Nez.Textures;


namespace Nez.NormalMapGenerator
{
	[ContentProcessor( DisplayName = "Normal Map Generator" )]
	public class NormalMapProcessor : TextureProcessor
	{
		public enum BlurType
		{
			None,
			Color,
			Grayscale
		}


		#region Processor properties

		[DefaultValueAttribute( false )]
		public virtual bool flattenImage { get; set; }

		[DefaultValueAttribute( typeof( Color ), "255,255,255,255" )]
		public Color opaqueColor { get; set; } = Color.White;

		[DefaultValueAttribute( typeof( Color ), "0,0,0,255" )]
		public Color transparentColor { get; set; } = Color.Black;

		[DefaultValueAttribute( typeof( BlurType ), "BlurType.Grayscale" )]
		public BlurType blurType { get; set; } = BlurType.Grayscale;

		public float blurDeviation { get; set; } = 0.5f;

		[DefaultValueAttribute( typeof( TextureUtils.EdgeDetectionFilter ), "TextureUtils.EdgeDetectionFilter.Sobel" )]
		public TextureUtils.EdgeDetectionFilter edgeDetectionFilter { get; set; }

		public float normalStrength { get; set; } = 1f;

		[DefaultValueAttribute( false )]
		public bool invertX { get; set; }

		[DefaultValueAttribute( false )]
		public bool invertY { get; set; }

		#endregion


		public static ContentBuildLogger logger;


		public override TextureContent Process( TextureContent input, ContentProcessorContext context )
		{
			logger = context.Logger;
			logger.LogMessage( "sending texture to base TextureProcessor for initial processing" );

			var textureContent = base.Process( input, context );
			var bmp = (PixelBitmapContent<Color>)textureContent.Faces[0][0];
			var destData = bmp.getData();

			// process the data
			if( flattenImage )
			{
				logger.LogMessage( "flattening image" );
				destData = TextureUtils.createFlatHeightmap( destData, opaqueColor, transparentColor );
			}

			if( blurType != BlurType.None )
			{
				logger.LogMessage( "blurring image width blurDeviation: {0}", blurDeviation );
				if( blurType == BlurType.Color )
					destData = TextureUtils.createBlurredTexture( destData, bmp.Width, bmp.Height, (double)blurDeviation );
				else
					destData = TextureUtils.createBlurredGrayscaleTexture( destData, bmp.Width, bmp.Height, (double)blurDeviation );
			}

			logger.LogMessage( "generating normal map with {0}", edgeDetectionFilter );
			destData = TextureUtils.createNormalMap( destData, edgeDetectionFilter, bmp.Width, bmp.Height, normalStrength, invertX, invertY );

			bmp.setData( destData );

			return textureContent;
		}
	}
}

