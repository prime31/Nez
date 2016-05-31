using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Textures
{
	public static class TextureUtils
	{
		/// <summary>
		/// processes each pixel of the passed in Texture and in the output texture transparent pixels will be transparentColor and opaque pixels
		/// will be opaqueColor. This is useful for creating normal maps for rim lighting by applying a grayscale blur then using createNormalMap*
		/// by doing something like the following. The first step is used only for making rim lighting normal maps:
		/// - var maskTex = createFlatHeightmap( tex, Color.White, Color.Black )
		/// - var blurredTex = createBlurredGrayscaleTexture( maskTex, 1 )
		/// - createNormalMap( blurredTex, 50f )
		/// </summary>
		/// <returns>The flat heightmap.</returns>
		/// <param name="image">Image.</param>
		/// <param name="opaqueColor">Opaque color.</param>
		/// <param name="transparentColor">Transparent color.</param>
		public static Texture2D createFlatHeightmap( Texture2D image, Color opaqueColor, Color transparentColor )
		{
			var resultTex = new Texture2D( Core.graphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color );

			var srcData = new Color[image.Width * image.Height];
			image.GetData<Color>( srcData );

			var destData = createFlatHeightmap( srcData, opaqueColor, transparentColor );

			resultTex.SetData( destData );

			return resultTex;
		}


		public static Color[] createFlatHeightmap( Color[] srcData, Color opaqueColor, Color transparentColor )
		{
			var destData = new Color[srcData.Length];

			for( var i = 0; i < srcData.Length; i++ )
			{
				var pixel = srcData[i];

				if( pixel.A == 0 )
					destData[i] = transparentColor;
				else
					destData[i] = opaqueColor;
			}

			return destData;
		}

		
		/// <summary>
		/// creates a new texture that is a gaussian blurred version of the original in grayscale
		/// </summary>
		/// <returns>The blurred texture.</returns>
		/// <param name="image">Image.</param>
		/// <param name="deviation">Deviation.</param>
		public static Texture2D createBlurredGrayscaleTexture( Texture2D image, double deviation = 1 )
		{
			return GaussianBlur.createBlurredGrayscaleTexture( image, deviation );
		}


		public static Color[] createBlurredTexture( Color[] srcData, int width, int height, double deviation = 1 )
		{
			return GaussianBlur.createBlurredTexture( srcData, width, height, deviation );
		}


		/// <summary>
		/// creates a new texture that is a gaussian blurred version of the original
		/// </summary>
		/// <returns>The blurred texture.</returns>
		/// <param name="image">Image.</param>
		/// <param name="deviation">Deviation.</param>
		public static Texture2D createBlurredTexture( Texture2D image, double deviation = 1 )
		{
			return GaussianBlur.createBlurredTexture( image, deviation );
		}


		public static Color[] createBlurredGrayscaleTexture( Color[] srcData, int width, int height, double deviation = 1 )
		{
			return GaussianBlur.createBlurredGrayscaleTexture( srcData, width, height, deviation );
		}


		/// <summary>
		/// generates a normal map from a height map calculating it with a sobel filter
		/// </summary>
		/// <returns>The sobel filter.</returns>
		/// <param name="image">Image.</param>
		/// <param name="normalStrength">Normal strength.</param>
		public static Texture2D createNormalMapWithSobelFilter( Texture2D image, float normalStrength = 1f, bool invertX = false, bool invertY = false )
		{
			var resultTex = new Texture2D( Core.graphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color );

			var srcData = new Color[image.Width * image.Height];
			image.GetData<Color>( srcData );

			var destData = createNormalMapWithSobelFilter( srcData, image.Width, image.Height, normalStrength, invertX, invertY );
			resultTex.SetData( destData );

			return resultTex;
		}


		public static Color[] createNormalMapWithSobelFilter( Color[] srcData, int width, int height, float normalStrength = 1f, bool invertX = false, bool invertY = false )
		{
			// x/r is inverted here due to the sobel kernel used so we use the opposite to keep it the same as createNormalMap
			var invertR = invertX ? 1f : -1f;
			var invertG = invertY ? -1f : 1f;
			var destData = new Color[width * height];

			for( var i = 1; i < width - 1; i++ )
			{
				for( var j = 1; j < height - 1; j++ )
				{
					var cr = srcData[i + 1 + j * width].grayscale().B / 255f;
					var cl = srcData[i - 1 + j * width].grayscale().B / 255f;
					var cu = srcData[i + ( j - 1 ) * width].grayscale().B / 255f;
					var cd = srcData[i + ( j + 1 ) * width].grayscale().B / 255f;
					var cld = srcData[i - 1 + ( j + 1 ) * width].grayscale().B / 255f;
					var clu = srcData[i - 1 + ( j - 1 ) * width].grayscale().B / 255f;
					var crd = srcData[i + 1 + ( j + 1 ) * width].grayscale().B / 255f;
					var cru = srcData[i + 1 + ( j - 1 ) * width].grayscale().B / 255f;

					// Compute dx using Sobel:
					//           -1 0 1 
					//           -2 0 2
					//           -1 0 1
					var dX = cru + 2 * cr + crd - clu - 2 * cl - cld;

					// Compute dy using Sobel:
					//           -1 -2 -1 
					//            0  0  0
					//            1  2  1
					var dY = cld + 2 * cd + crd - clu - 2 * cu - cru;

					var normal = Vector3.Normalize( new Vector3( dX * invertR, dY * invertG, 1f / normalStrength ) );
					normal = normal * 0.5f + new Vector3( 0.5f );
					destData[i + j * width] = new Color( normal.X, normal.Y, normal.Z, 1f );
				}
			}

			return destData;
		}


		/// <summary>
		/// generates a normal map from a height map calculating it with just the 4 surrounding pixels
		/// </summary>
		/// <returns>The normal map.</returns>
		/// <param name="image">Image.</param>
		/// <param name="bias">Bias.</param>
		/// <param name="invertRed">If set to <c>true</c> invert red.</param>
		/// <param name="invertGreen">If set to <c>true</c> invert green.</param>
		public static Texture2D createNormalMap( Texture2D image, float bias = 50f, bool invertX = false, bool invertY = false )
		{
			var resultTex = new Texture2D( Core.graphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color );

			var srcData = new Color[image.Width * image.Height];
			image.GetData<Color>( srcData );

			var destData = createNormalMap( srcData, image.Width, image.Height, bias, invertX, invertY );
			resultTex.SetData( destData );

			return resultTex;
		}


		public static Color[] createNormalMap( Color[] srcData, int width, int height, float bias = 50f, bool invertX = false, bool invertY = false )
		{
			var invertR = invertX ? -1f : 1f;
			var invertG = invertY ? -1f : 1f;
			var destData = new Color[width * height];

			for( var i = 1; i < width - 1; i++ )
			{
				for( var j = 1; j < height - 1; j++ )
				{
					var d0 = srcData[i + j * width].grayscale().B / 255f;
					var d1 = srcData[i + 1 + j * width].grayscale().B / 255f;
					var d2 = srcData[i - 1 + j * width].grayscale().B / 255f;
					var d3 = srcData[i + ( j - 1 ) * width].grayscale().B / 255f;
					var d4 = srcData[i + ( j + 1 ) * width].grayscale().B / 255f;

					var dx = ( ( d2 - d0 ) + ( d0 - d1 ) ) * 0.5f;
					var dy = ( ( d4 - d0 ) + ( d0 - d3 ) ) * 0.5f;

					var normal = new Vector3( dx * invertR, dy * invertG, 1.0f - ( ( bias - 0.1f ) / 100.0f ) );
					normal.Normalize();
					normal = normal * 0.5f + new Vector3( 0.5f );
					destData[i + j * width] = new Color( normal.X, normal.Y, normal.Z, 1f );
				}
			}

			return destData;
		}

	}
}

