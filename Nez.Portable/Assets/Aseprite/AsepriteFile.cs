using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents the contents loaded from an Aseprite file.
	/// </summary>
	public sealed class AsepriteFile
	{
		/// <summary>
		/// The width, in pixels, defined for the canvas of the Aseprite image.
		/// </summary>
		/// <remarks>
		/// This is also the width of every frame.
		/// </remarks>
		public readonly int CanvasWidth;

		/// <summary>
		/// The height, in pixels, defined for the canvas of the Aseprite image.
		/// </summary>
		/// <remarks>
		/// This is also the height of every frame.
		/// </remarks>
		public readonly int CanvasHeight;

		/// <summary>
		/// The color depth mode used for the image in Aseprite which defines the total number of bits per pixel.
		/// </summary>
		public readonly AsepriteColorDepth ColorDepth;

		/// <summary>
		/// A collection of all frame elements in the Aseprite file.  Order of elements is from first-to-last.
		/// </summary>
		public readonly List<AsepriteFrame> Frames;

		/// <summary>
		/// A collection of all layer elements in the Aseprite file.  Order of elements is from bottom-to-top.
		/// </summary>
		public readonly List<AsepriteLayer> Layers;

		/// <summary>
		/// A collection of all tag elements from the Aseprite file.  Order of elements is as defined in the Aseprite UI
		/// from left-to-right.
		/// </summary>
		public readonly List<AsepriteTag> Tags;

		/// <summary>
		/// A collection of all slice elements from the Aseprite file.  Order of elements is in the order they were
		/// created in Aseprite.
		/// </summary>
		public readonly List<AsepriteSlice> Slices;

		/// <summary>
		/// A collection of any warnings issued when parsing the Aseprite file.  You can use this to see if there were
		/// any non-fatal errors that occurred while parsing the file.
		/// </summary>
		public readonly List<string> Warnings;

		/// <summary>
		/// The palette data from the Aseprite file containing the palette information and colors.
		/// </summary>
		public readonly AsepritePalette Palette;

		/// <summary>
		/// The custom user data that was set in the sprite properties in Aseprite.
		/// </summary>
		public AsepriteUserData UserData { get; }

		/// <summary>
		/// The name of this Aseprite file (without extension)
		/// </summary>
		public readonly string Name;

		internal AsepriteFile(string name, AsepritePalette palette, int width, int height, AsepriteColorDepth colorDepth, List<AsepriteFrame> frames, List<AsepriteLayer> layers, List<AsepriteTag> tags, List<AsepriteSlice> slices, List<string> warnings)
		{
			Name = name;
			CanvasWidth = width;
			CanvasHeight = height;
			ColorDepth = colorDepth;
			Frames = frames;
			Tags = tags;
			Slices = slices;
			Warnings = warnings;
			Palette = palette;
		}

		/// <summary>
		/// Translates the data in this aseprite file to a sprite atlas that can be used in a sprite animator component.
		/// </summary>
		/// <param name="onlyVisibleLayers">
		/// Indicates whether only layers that are visible in the Aseprite file should be included when generating the 
		/// texture.
		/// </param>
		/// <param name="borderPadding">
		/// Indicates the amount of padding, in transparent pixels, to add to the edge of the generated texture.
		/// </param>
		/// <param name="spacing">
		/// Indicates the amount of padding, in transparent pixels, to add between each frame in the generated texture.
		/// </param>
		/// <param name="innerPadding">
		/// indicates the amount of padding, in transparent pixels, to add around the edges of each frame in the
		/// generated texture.
		/// </param>
		/// <returns>
		/// A new instance of hte <see cref="SpriteAtlas"/> class initialized with the data generated from this Aseprite
		/// file.
		/// </returns>
		public SpriteAtlas ToSpriteAtlas(bool onlyVisibleLayers = true, int borderPadding = 0, int spacing = 0, int innerPadding = 0)
		{
			SpriteAtlas atlas = new SpriteAtlas
			{
				Names = new string[Frames.Count],
				Sprites = new Sprite[Frames.Count],
				SpriteAnimations = new SpriteAnimation[Tags.Count],
				AnimationNames = new string[Tags.Count]
			};

			Color[][] flattenedFrames = new Color[Frames.Count][];

			for (int i = 0; i < Frames.Count; i++)
			{
				flattenedFrames[i] = Frames[i].FlattenFrame(onlyVisibleLayers);
			}

			double sqrt = Math.Sqrt(Frames.Count);
			int columns = (int)Math.Ceiling(sqrt);
			int rows = (Frames.Count + columns - 1) / columns;

			int imageWidth = (columns * CanvasWidth)
							 + (borderPadding * 2)
							 + (spacing * (columns - 1))
							 + (innerPadding * 2 * columns);

			int imageHeight = (rows * CanvasHeight)
							  + (borderPadding * 2)
							  + (spacing * (rows - 1))
							  + (innerPadding * 2 * rows);

			Color[] imagePixels = new Color[imageWidth * imageHeight];
			Rectangle[] regions = new Rectangle[Frames.Count];

			for (int i = 0; i < flattenedFrames.GetLength(0); i++)
			{
				int column = i % columns;
				int row = i / columns;
				Color[] frame = flattenedFrames[i];

				int x = (column * CanvasWidth)
						+ borderPadding
						+ (spacing * column)
						+ (innerPadding * (column + column + 1));

				int y = (row * CanvasHeight)
						 + borderPadding
						 + (spacing * row)
						 + (innerPadding * (row + row + 1));

				for (int p = 0; p < frame.Length; p++)
				{
					int px = (p % CanvasWidth) + x;
					int py = (p / CanvasWidth) + y;

					int index = py * imageWidth + px;
					imagePixels[index] = frame[p];

				}

				regions[i] = new Rectangle(x, y, CanvasWidth, CanvasHeight);
			}

			Texture2D texture = new Texture2D(Core.GraphicsDevice, imageWidth, imageHeight);
			texture.SetData<Color>(imagePixels);

			for (int i = 0; i < Frames.Count; i++)
			{
				atlas.Sprites[i] = new Sprite(texture, regions[i]);
			}

			for (int tagNum = 0; tagNum < Tags.Count; tagNum++)
			{
				AsepriteTag tag = Tags[tagNum];
				Sprite[] sprites = new Sprite[tag.To - tag.From + 1];
				float[] durations = new float[sprites.Length];

				for (int spriteIndex = 0, lookupIndex = tag.From; spriteIndex < sprites.Length; spriteIndex++, lookupIndex++)
				{
					sprites[spriteIndex] = atlas.Sprites[lookupIndex];
					durations[spriteIndex] = 1.0f / (Frames[lookupIndex].Duration / 1000.0f);
				}

				atlas.SpriteAnimations[tagNum] = new SpriteAnimation(sprites, durations);
				atlas.AnimationNames[tagNum] = tag.Name;
			}

			return atlas;

			// Dictionary<int, Color[]> frameColorLookup = new Dictionary<int, Color[]>();

			// for (int frameNum = 0; frameNum < Frames.Count; frameNum++)
			// {
			// 	frameColorLookup.Add(frameNum, Frames[frameNum].FlattenFrame(onlyVisibleLayers));
			// }

			// int columns, rows;
			// int width, height;
			// int totalFrames = frameColorLookup.Count;
			// atlas.Sprites = new Sprite[totalFrames];

			// double sqrt = Math.Sqrt(totalFrames);
			// columns = (int)Math.Floor(sqrt);
			// if (Math.Abs(sqrt % 1) >= double.Epsilon)
			// {
			// 	columns++;
			// }

			// rows = totalFrames / columns;
			// if (totalFrames % columns != 0)
			// {
			// 	rows++;
			// }

			// width = (columns * CanvasWidth) +
			// 		(borderPadding * 2) +
			// 		(spacing * (columns - 1)) +
			// 		(innerPadding * 2 * columns);

			// height = (rows * CanvasHeight) +
			// 		 (borderPadding * 2) +
			// 		 (spacing + (rows - 1)) +
			// 		 (innerPadding * 2 * rows);

			// Color[] texturePixels = new Color[width * height];

			// int frameOffset = 0;

			// Rectangle[] sourceRects = new Rectangle[Frames.Count];

			// for (int frameNum = 0; frameNum < Frames.Count; frameNum++)
			// {
			// 	//	Calculate the x and y position of the frame's top-left pixel relative to the top-left of the
			// 	//	final texture
			// 	int frameCol = (frameNum - frameOffset) % columns;
			// 	int frameRow = (frameNum - frameOffset) / columns;

			// 	//	Inject the pixel color data from teh frame into the final texture color data array
			// 	Color[] pixels = frameColorLookup[frameNum];

			// 	for (int pixelNum = 0; pixelNum < pixels.Length; pixelNum++)
			// 	{
			// 		int x = (pixelNum % CanvasWidth) + (frameCol * CanvasWidth);
			// 		int y = (pixelNum / CanvasWidth) + (frameRow * CanvasHeight);

			// 		//	Adjust for padding/spacing
			// 		x += borderPadding;
			// 		y += borderPadding;

			// 		if (spacing > 0)
			// 		{
			// 			if (frameCol > 0)
			// 			{
			// 				x += spacing * frameCol;
			// 			}

			// 			if (frameRow > 0)
			// 			{
			// 				y += spacing * frameRow;
			// 			}
			// 		}

			// 		if (innerPadding > 0)
			// 		{
			// 			x += innerPadding * (frameCol + 1);
			// 			y += innerPadding * (frameRow + 1);

			// 			if (frameCol > 0)
			// 			{
			// 				x += innerPadding * frameCol;
			// 			}

			// 			if (frameRow > 0)
			// 			{
			// 				y += innerPadding * frameRow;
			// 			}
			// 		}

			// 		int index = y * width + x;
			// 		texturePixels[index] = pixels[pixelNum];
			// 	}


			// 	//	Now create the source rectangle
			// 	Rectangle sourceRectangle = new Rectangle(0, 0, CanvasWidth, CanvasHeight);
			// 	sourceRectangle.X += borderPadding;
			// 	sourceRectangle.Y += borderPadding;

			// 	if (spacing > 0)
			// 	{
			// 		if (frameCol > 0)
			// 		{
			// 			sourceRectangle.X += spacing * frameCol;
			// 		}

			// 		if (frameRow > 0)
			// 		{
			// 			sourceRectangle.Y += spacing * frameRow;
			// 		}
			// 	}

			// 	if (innerPadding > 0)
			// 	{
			// 		sourceRectangle.X += innerPadding * (frameCol + 1);
			// 		sourceRectangle.Y += innerPadding * (frameRow + 1);

			// 		if (frameCol > 0)
			// 		{
			// 			sourceRectangle.X += innerPadding * frameCol;
			// 		}

			// 		if (frameRow > 0)
			// 		{
			// 			sourceRectangle.Y += innerPadding * frameRow;
			// 		}
			// 	}

			// 	sourceRects[frameNum] = sourceRectangle;
			// }

			// Texture2D texture = new Texture2D(Core.GraphicsDevice, width, height);
			// texture.SetData<Color>(texturePixels);

			// for (int frameNum = 0; frameNum < Frames.Count; frameNum++)
			// {
			// 	atlas.Sprites[frameNum] = new Sprite(texture, sourceRects[frameNum]);
			// 	atlas.Names[frameNum] = $"{Name}_{frameNum}";
			// }

			// for (int tagNum = 0; tagNum < Tags.Count; tagNum++)
			// {
			// 	AsepriteTag tag = Tags[tagNum];
			// 	Sprite[] sprites = new Sprite[tag.To - tag.From + 1];
			// 	float[] durations = new float[sprites.Length];

			// 	for (int spriteIndex = 0, lookupIndex = tag.From; spriteIndex < sprites.Length; spriteIndex++, lookupIndex++)
			// 	{
			// 		sprites[spriteIndex] = atlas.Sprites[lookupIndex];
			// 		durations[spriteIndex] = Frames[lookupIndex].Duration;
			// 	}

			// 	atlas.SpriteAnimations[tagNum] = new SpriteAnimation(sprites, durations);
			// 	atlas.AnimationNames[tagNum] = tag.Name;
			// }

			// return atlas;
		}
	}
}