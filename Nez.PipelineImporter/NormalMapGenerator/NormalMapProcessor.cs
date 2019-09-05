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
	public enum BlurType
	{
		None,
		Color,
		Grayscale
	}

	[ContentProcessor(DisplayName = "Normal Map Generator")]
	public class NormalMapProcessor : TextureProcessor
	{
		#region Processor properties

		[DefaultValueAttribute(false)] public virtual bool FlattenImage { get; set; }

		[DefaultValueAttribute(typeof(Color), "255,255,255,255")]
		public Color OpaqueColor { get; set; } = Color.White;

		[DefaultValueAttribute(typeof(Color), "0,0,0,255")]
		public Color TransparentColor { get; set; } = Color.Black;

		[DefaultValueAttribute(typeof(BlurType), "BlurType.Grayscale")]
		public BlurType BlurType { get; set; } = BlurType.Grayscale;

		public float BlurDeviation { get; set; } = 0.5f;

		[DefaultValueAttribute(typeof(TextureUtils.EdgeDetectionFilter), "TextureUtils.EdgeDetectionFilter.Sobel")]
		public TextureUtils.EdgeDetectionFilter EdgeDetectionFilter { get; set; }

		public float NormalStrength { get; set; } = 1f;

		[DefaultValueAttribute(false)] public bool InvertX { get; set; }

		[DefaultValueAttribute(false)] public bool InvertY { get; set; }

		#endregion


		public static ContentBuildLogger Logger;


		public override TextureContent Process(TextureContent input, ContentProcessorContext context)
		{
			Logger = context.Logger;
			Logger.LogMessage("sending texture to base TextureProcessor for initial processing");

			var textureContent = base.Process(input, context);
			var bmp = (PixelBitmapContent<Color>) textureContent.Faces[0][0];
			var destData = bmp.GetData();

			// process the data
			if (FlattenImage)
			{
				Logger.LogMessage("flattening image");
				destData = TextureUtils.CreateFlatHeightmap(destData, OpaqueColor, TransparentColor);
			}

			if (BlurType != BlurType.None)
			{
				Logger.LogMessage("blurring image width blurDeviation: {0}", BlurDeviation);
				if (BlurType == BlurType.Color)
					destData = TextureUtils.CreateBlurredTexture(destData, bmp.Width, bmp.Height,
						(double) BlurDeviation);
				else
					destData = TextureUtils.CreateBlurredGrayscaleTexture(destData, bmp.Width, bmp.Height,
						(double) BlurDeviation);
			}

			Logger.LogMessage("generating normal map with {0}", EdgeDetectionFilter);
			destData = TextureUtils.CreateNormalMap(destData, EdgeDetectionFilter, bmp.Width, bmp.Height,
				NormalStrength, InvertX, InvertY);

			bmp.SetData(destData);

			return textureContent;
		}
	}
}