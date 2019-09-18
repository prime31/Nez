using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using static Nez.Tools.Atlases.SpriteAtlasPacker;

namespace Nez.Tools.Atlases
{
	public class ImagePacker
	{
		// various properties of the resulting image
		private bool requirePow2, requireSquare;
		private int padding;
		private int outputWidth, outputHeight;

		// the input list of image files
		private List<string> files;

		// some dictionaries to hold the image sizes and destination rectangles
		private readonly Dictionary<string, Size> imageSizes = new Dictionary<string, Size>();
		private readonly Dictionary<string, Rectangle> imagePlacement = new Dictionary<string, Rectangle>();

		/// <summary>
		/// Packs a collection of images into a single image.
		/// </summary>
		/// <param name="imageFiles">The list of file paths of the images to be combined.</param>
		/// <param name="requirePowerOfTwo">Whether or not the output image must have a power of two size.</param>
		/// <param name="requireSquareImage">Whether or not the output image must be a square.</param>
		/// <param name="maximumWidth">The maximum width of the output image.</param>
		/// <param name="maximumHeight">The maximum height of the output image.</param>
		/// <param name="imagePadding">The amount of blank space to insert in between individual images.</param>
		/// <param name="outputImage">The resulting output image.</param>
		/// <param name="outputMap">The resulting output map of placement rectangles for the images.</param>
		/// <returns>0 if the packing was successful, error code otherwise.</returns>
		public int PackImage(
			IEnumerable<string> imageFiles, 
			bool requirePowerOfTwo, 
			bool requireSquareImage, 
			int maximumWidth,
			int maximumHeight,
			int imagePadding,
			out Bitmap outputImage, 
			out Dictionary<string, Rectangle> outputMap)
		{
			files = new List<string>(imageFiles);
			requirePow2 = requirePowerOfTwo;
			requireSquare = requireSquareImage;
			outputWidth = maximumWidth;
			outputHeight = maximumHeight;
			padding = imagePadding;

			outputImage = null;
			outputMap = null;

			// make sure our dictionaries are cleared before starting
			imageSizes.Clear();
			imagePlacement.Clear();

			// get the sizes of all the images
			foreach (var image in files)
			{
				var bitmap = Bitmap.FromFile(image) as Bitmap;
				if (bitmap == null)
					return (int)FailCode.FailedToLoadImage;
				imageSizes.Add(image, bitmap.Size);
			}

			// sort our files by file size so we place large sprites first
			files.Sort(
				(f1, f2) =>
				{
					Size b1 = imageSizes[f1];
					Size b2 = imageSizes[f2];

					int c = -b1.Width.CompareTo(b2.Width);
					if (c != 0)
						return c;

					c = -b1.Height.CompareTo(b2.Height);
					if (c != 0)
						return c;

					return f1.CompareTo(f2);
				});

			// try to pack the images
			if (!PackImageRectangles())
				return (int)FailCode.FailedToPackImage;

			// make our output image
			outputImage = CreateOutputImage();
			if (outputImage == null)
				return (int)FailCode.FailedToSaveImage;
				
			// go through our image placements and replace the width/height found in there with
			// each image's actual width/height (since the ones in imagePlacement will have padding)
			var keys = new string[imagePlacement.Keys.Count];
			imagePlacement.Keys.CopyTo(keys, 0);
			foreach (var k in keys)
			{
				// get the actual size
				var s = imageSizes[k];

				// get the placement rectangle
				var r = imagePlacement[k];

				// set the proper size
				r.Width = s.Width;
				r.Height = s.Height;

				// insert back into the dictionary
				imagePlacement[k] = r;
			}

			// copy the placement dictionary to the output
			outputMap = new Dictionary<string, Rectangle>();
			foreach (var pair in imagePlacement)
			{
				outputMap.Add(pair.Key, pair.Value);
			}

			// clear our dictionaries just to free up some memory
			imageSizes.Clear();
			imagePlacement.Clear();

			return 0;
		}

		// This method does some trickery type stuff where we perform the TestPackingImages method over and over, 
		// trying to reduce the image size until we have found the smallest possible image we can fit.
		private bool PackImageRectangles()
		{
			// create a dictionary for our test image placements
			Dictionary<string, Rectangle> testImagePlacement = new Dictionary<string, Rectangle>();

			// get the size of our smallest image
			int smallestWidth = int.MaxValue;
			int smallestHeight = int.MaxValue;
			foreach (var size in imageSizes)
			{
				smallestWidth = Math.Min(smallestWidth, size.Value.Width);
				smallestHeight = Math.Min(smallestHeight, size.Value.Height);
			}

			// we need a couple values for testing
			int testWidth = outputWidth;
			int testHeight = outputHeight;

			bool shrinkVertical = false;

			// just keep looping...
			while (true)
			{
				// make sure our test dictionary is empty
				testImagePlacement.Clear();

				// try to pack the images into our current test size
				if (!TestPackingImages(testWidth, testHeight, testImagePlacement))
				{
					// if that failed...

					// if we have no images in imagePlacement, i.e. we've never succeeded at PackImages,
					// show an error and return false since there is no way to fit the images into our
					// maximum size texture
					if (imagePlacement.Count == 0)
						return false;

					// otherwise return true to use our last good results
					if (shrinkVertical)
						return true;

					shrinkVertical = true;
					testWidth += smallestWidth + padding + padding;
					testHeight += smallestHeight + padding + padding;
					continue;
				}

				// clear the imagePlacement dictionary and add our test results in
				imagePlacement.Clear();
				foreach (var pair in testImagePlacement)
					imagePlacement.Add(pair.Key, pair.Value);

				// figure out the smallest bitmap that will hold all the images
				testWidth = testHeight = 0;
				foreach (var pair in imagePlacement)
				{
					testWidth = Math.Max(testWidth, pair.Value.Right);
					testHeight = Math.Max(testHeight, pair.Value.Bottom);
				}

				// subtract the extra padding on the right and bottom
				if (!shrinkVertical)
					testWidth -= padding;
				testHeight -= padding;

				// if we require a power of two texture, find the next power of two that can fit this image
				if (requirePow2)
				{
					testWidth = MiscHelper.FindNextPowerOfTwo(testWidth);
					testHeight = MiscHelper.FindNextPowerOfTwo(testHeight);
				}

				// if we require a square texture, set the width and height to the larger of the two
				if (requireSquare)
				{
					int max = Math.Max(testWidth, testHeight);
					testWidth = testHeight = max;
				}

				// if the test results are the same as our last output results, we've reached an optimal size,
				// so we can just be done
				if (testWidth == outputWidth && testHeight == outputHeight)
				{
					if (shrinkVertical)
						return true;

					shrinkVertical = true;
				}

				// save the test results as our last known good results
				outputWidth = testWidth;
				outputHeight = testHeight;

				// subtract the smallest image size out for the next test iteration
				if (!shrinkVertical)
					testWidth -= smallestWidth;
				testHeight -= smallestHeight;
			}
		}

		private bool TestPackingImages(int testWidth, int testHeight, Dictionary<string, Rectangle> testImagePlacement)
		{
			// create the rectangle packer
			ArevaloRectanglePacker rectanglePacker = new ArevaloRectanglePacker(testWidth, testHeight);

			foreach (var image in files)
			{
				// get the bitmap for this file
				Size size = imageSizes[image];

				// pack the image
				Point origin;
				if (!rectanglePacker.TryPack(size.Width + padding, size.Height + padding, out origin))
				{
					return false;
				}

				// add the destination rectangle to our dictionary
				testImagePlacement.Add(image, new Rectangle(origin.X, origin.Y, size.Width + padding, size.Height + padding));
			}

			return true;
		}

		private Bitmap CreateOutputImage()
		{
			try
			{
				var outputImage = new Bitmap(outputWidth, outputHeight, PixelFormat.Format32bppArgb);

				// draw all the images into the output image
				foreach (var image in files)
				{
					var location = imagePlacement[image];
					var bitmap = Bitmap.FromFile(image) as Bitmap;
					if (bitmap == null)
						return null;

					// copy pixels over to avoid antialiasing or any other side effects of drawing
					// the subimages to the output image using Graphics
					for (int x = 0; x < bitmap.Width; x++)
						for (int y = 0; y < bitmap.Height; y++)
							outputImage.SetPixel(location.X + x, location.Y + y, bitmap.GetPixel(x, y));
				}

				return outputImage;
			}
			catch
			{
				return null;
			}
		}
	}
}