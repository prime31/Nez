using System;
using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Provides utility methods for working with colors in Aseprite.
	/// </summary>
	public static class AsepriteColorUtils
	{
		private const byte RGBA_R_SHIFT = 0;
		private const byte RGBA_G_SHIFT = 8;
		private const byte RGBA_B_SHIFT = 16;
		private const byte RGBA_A_SHIFT = 24;
		private const uint RGBA_R_MASK = 0x000000ff;
		private const uint RGBA_G_MASK = 0x0000ff00;
		private const uint RGBA_B_MASK = 0x00ff0000;
		private const uint RGBA_RGB_MASK = 0x00ffffff;
		private const uint RGBA_A_MASK = 0xff000000;

		/// <summary>
		/// Given two colors, blends them using the specified <see cref="AsepriteBlendMode"/>.  
		/// </summary>
		/// <remarks>
		///	The <paramref name="backdrop"/> is the color on the bottom layer and the <paramref name="source"/> is the
		///	color on the top layer that is blending down to the backdrop.
		/// </remarks>
		/// <param name="mode">The blend mode to use when blending the two colors.</param>
		/// <param name="backdrop">The color on the lowest layer when blending</param>
		/// <param name="source">The color on the highest layer when blending.</param>
		/// <param name="opacity">The opacity of the source layer (cel.Opacity * cel.Layer.Opacity)</param>
		/// <returns>
		/// A <see cref="Microsoft.Xna.Framework.Color"/> value that is the result of blending the two colors given.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the <paramref name="mode"/> parameter is an unknown <see cref="AsepriteBlendMode"/> value.
		/// </exception>
		public static Color Blend(AsepriteBlendMode mode, Color backdrop, Color source, int opacity)
		{
			if (backdrop.A == 0 && source.A == 0)
			{
				return Color.Transparent;
			}
			else if (backdrop.A == 0)
			{
				return source;
			}
			else if (source.A == 0)
			{
				return backdrop;
			}

			uint b = RGBA(backdrop.R, backdrop.G, backdrop.B, backdrop.A);
			uint s = RGBA(source.R, source.G, source.B, source.A);
			uint blended;
			switch (mode)
			{
				case AsepriteBlendMode.Normal:
					blended = Normal(b, s, opacity);
					break;
				case AsepriteBlendMode.Multiply:
					blended = Multiply(b, s, opacity);
					break;
				case AsepriteBlendMode.Screen:
					blended = Screen(b, s, opacity);
					break;
				case AsepriteBlendMode.Overlay:
					blended = Overlay(b, s, opacity);
					break;
				case AsepriteBlendMode.Darken:
					blended = Darken(b, s, opacity);
					break;
				case AsepriteBlendMode.Lighten:
					blended = Lighten(b, s, opacity);
					break;
				case AsepriteBlendMode.ColorDodge:
					blended = ColorDodge(b, s, opacity);
					break;
				case AsepriteBlendMode.ColorBurn:
					blended = ColorBurn(b, s, opacity);
					break;
				case AsepriteBlendMode.HardLight:
					blended = HardLight(b, s, opacity);
					break;
				case AsepriteBlendMode.SoftLight:
					blended = SoftLight(b, s, opacity);
					break;
				case AsepriteBlendMode.Difference:
					blended = Difference(b, s, opacity);
					break;
				case AsepriteBlendMode.Exclusion:
					blended = Exclusion(b, s, opacity);
					break;
				case AsepriteBlendMode.Hue:
					blended = HslHue(b, s, opacity);
					break;
				case AsepriteBlendMode.Saturation:
					blended = HslSaturation(b, s, opacity);
					break;
				case AsepriteBlendMode.Color:
					blended = HslColor(b, s, opacity);
					break;
				case AsepriteBlendMode.Luminosity:
					blended = HslLuminosity(b, s, opacity);
					break;
				case AsepriteBlendMode.Addition:
					blended = Addition(b, s, opacity);
					break;
				case AsepriteBlendMode.Subtract:
					blended = Subtract(b, s, opacity);
					break;
				case AsepriteBlendMode.Divide:
					blended = Divide(b, s, opacity);
					break;
				default:
					throw new InvalidOperationException($"Unknown blend mode '{mode}'");
			};

			byte red = GetR(blended);
			byte green = GetG(blended);
			byte blue = GetB(blended);
			byte alpha = GetA(blended);

			return new Color(red, green, blue, alpha);
		}

		private static uint RGBA(int red, int green, int blue, int alpha) => (uint)red << RGBA_R_SHIFT |
																			 (uint)green << RGBA_G_SHIFT |
																			 (uint)blue << RGBA_B_SHIFT |
																			 (uint)alpha << RGBA_A_SHIFT;

		private static byte GetR(uint value) => (byte)((value >> RGBA_R_SHIFT) & 0xFF);
		private static byte GetG(uint value) => (byte)((value >> RGBA_G_SHIFT) & 0xFF);
		private static byte GetB(uint value) => (byte)((value >> RGBA_B_SHIFT) & 0xFF);
		private static byte GetA(uint value) => (byte)((value >> RGBA_A_SHIFT) & 0xFF);

		private static double Sat(double red, double green, double blue) => Math.Max(red, Math.Max(green, blue)) - Math.Min(red, Math.Min(green, blue));

		private static double Lum(double red, double green, double blue) => 0.3 * red + 0.59 * green + 0.11 * blue;

		private static void SetSat(ref double red, ref double green, ref double blue, double saturation)
		{
			ref double MIN(ref double x, ref double y) => ref (x < y ? ref x : ref y);
			ref double MAX(ref double x, ref double y) => ref (x > y ? ref x : ref y);
			ref double MID(ref double x, ref double y, ref double z) =>
				ref (x > y ? ref (y > z ? ref y : ref (x > z ? ref z : ref x)) : ref (y > z ? ref (z > x ? ref z : ref x) : ref y));

			ref double min = ref MIN(ref red, ref MIN(ref green, ref blue));
			ref double mid = ref MID(ref red, ref green, ref blue);
			ref double max = ref MAX(ref red, ref MAX(ref green, ref blue));

			if (max > min)
			{
				mid = ((mid - min) * saturation) / (max - min);
				max = saturation;
			}
			else
			{
				mid = max = 0;
			}

			min = 0;
		}

		private static void SetLum(ref double red, ref double green, ref double blue, double luminosity)
		{
			double d = luminosity - Lum(red, green, blue);
			red += d;
			green += d;
			blue += d;
			ClipColor(ref red, ref green, ref blue);
		}

		private static void ClipColor(ref double red, ref double green, ref double blue)
		{
			double luminosity = Lum(red, green, blue);
			double min = Math.Min(red, Math.Min(green, blue));
			double max = Math.Max(red, Math.Max(green, blue));

			if (min < 0)
			{
				red = luminosity + (((red - luminosity) * luminosity) / (luminosity - min));
				green = luminosity + (((green - luminosity) * luminosity) / (luminosity - min));
				blue = luminosity + (((blue - luminosity) * luminosity) / (luminosity - min));
			}

			if (max > 1)
			{
				red = luminosity + (((red - luminosity) * (1 - luminosity)) / (max - luminosity));
				green = luminosity + (((green - luminosity) * (1 - luminosity)) / (max - luminosity));
				blue = luminosity + (((blue - luminosity) * (1 - luminosity)) / (max - luminosity));
			}
		}

		internal static byte MUL_UN8(int a, int b)
		{
			int t = (a * b) + 0x80;
			return (byte)(((t >> 8) + t) >> 8);
		}

		internal static byte DIV_UN8(int a, int b)
		{
			return (byte)(((ushort)a * 0xFF + (b / 2)) / b);
		}

		private static uint Normal(uint backdrop, uint src, int opacity)
		{
			if ((backdrop & RGBA_A_MASK) == 0)
			{
				int alpha = GetA(src);
				alpha = MUL_UN8(alpha, opacity);
				alpha <<= RGBA_A_SHIFT;
				return (uint)((src & RGBA_RGB_MASK) | (uint)alpha);
			}
			else if ((src & RGBA_A_MASK) == 0)
			{
				return backdrop;
			}

			int Br = GetR(backdrop);
			int Bg = GetG(backdrop);
			int Bb = GetB(backdrop);
			int Ba = GetA(backdrop);

			int Sr = GetR(src);
			int Sg = GetG(src);
			int Sb = GetB(src);
			int Sa = GetA(src);
			Sa = MUL_UN8(Sa, opacity);


			int Ra = Sa + Ba - MUL_UN8(Ba, Sa);

			int Rr = Br + (Sr - Br) * Sa / Ra;
			int Rg = Bg + (Sg - Bg) * Sa / Ra;
			int Rb = Bb + (Sb - Bb) * Sa / Ra;

			return RGBA(Rr, Rg, Rb, Ra);
		}

		private static uint Multiply(uint backdrop, uint source, int opacity)
		{
			int red = MUL_UN8(GetR(backdrop), GetR(source));
			int green = MUL_UN8(GetG(backdrop), GetG(source));
			int blue = MUL_UN8(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Screen(uint backdrop, uint source, int opacity)
		{
			int red = GetR(backdrop) + GetR(source) - MUL_UN8(GetR(backdrop), GetR(source));
			int green = GetG(backdrop) + GetG(source) - MUL_UN8(GetG(backdrop), GetG(source));
			int blue = GetB(backdrop) + GetB(source) - MUL_UN8(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Overlay(uint backdrop, uint source, int opacity)
		{
			int overlay(int b, int s)
			{
				if (b < 128)
				{
					b <<= 1;
					return MUL_UN8(s, b);
				}
				else
				{
					b = (b << 1) - 255;
					return s + b - MUL_UN8(s, b);
				}
			}

			int red = overlay(GetR(backdrop), GetR(source));
			int green = overlay(GetG(backdrop), GetG(source));
			int blue = overlay(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Darken(uint backdrop, uint source, int opacity)
		{
			int blend(int b, int s) => Math.Min(b, s);

			int red = blend(GetR(backdrop), GetR(source));
			int green = blend(GetG(backdrop), GetG(source));
			int blue = blend(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Lighten(uint backdrop, uint source, int opacity)
		{
			int lighten(int b, int s) => Math.Max(b, s);

			int red = lighten(GetR(backdrop), GetR(source));
			int green = lighten(GetG(backdrop), GetG(source));
			int blue = lighten(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint ColorDodge(uint backdrop, uint source, int opacity)
		{
			int dodge(int b, int s)
			{
				if (b == 0)
				{
					return 0;
				}

				s = 255 - s;

				if (b >= s)
				{
					return 255;
				}
				else
				{
					return DIV_UN8(b, s);
				}
			}

			int red = dodge(GetR(backdrop), GetR(source));
			int green = dodge(GetG(backdrop), GetG(source));
			int blue = dodge(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint ColorBurn(uint backdrop, uint source, int opacity)
		{
			int burn(int b, int s)
			{
				if (b == 255)
				{
					return 255;
				}

				b = (255 - b);

				if (b >= s)
				{
					return 0;
				}
				else
				{
					return 255 - DIV_UN8(b, s);
				}
			}

			int red = burn(GetR(backdrop), GetR(source));
			int green = burn(GetG(backdrop), GetG(source));
			int blue = burn(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		//  Not working
		private static uint HardLight(uint backdrop, uint source, int opacity)
		{
			int hardlight(int b, int s)
			{
				if (s < 128)
				{
					s <<= 1;
					return MUL_UN8(b, s);
				}
				else
				{
					s = (s << 1) - 255;
					return b + s - MUL_UN8(b, s);
				}
			}

			int red = hardlight(GetR(backdrop), GetR(source));
			int green = hardlight(GetG(backdrop), GetG(source));
			int blue = hardlight(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint SoftLight(uint backdrop, uint source, int opacity)
		{
			int softlight(int _b, int _s)
			{
				double b = _b / 255.0;
				double s = _s / 255.0;
				double r, d;

				if (b <= 0.25)
				{
					d = ((16 * b - 12) * b + 4) * b;
				}
				else
				{
					d = Math.Sqrt(b);
				}

				if (s <= 0.5)
				{
					r = b - (1.0 - 2.0 * s) * b * (1.0 - b);
				}
				else
				{
					r = b + (2.0 * s - 1.0) * (d - b);
				}

				return (int)(r * 255 + 0.5);
			}

			int red = softlight(GetR(backdrop), GetR(source));
			int green = softlight(GetG(backdrop), GetG(source));
			int blue = softlight(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Difference(uint backdrop, uint source, int opacity)
		{
			int difference(int b, int s)
			{
				return Math.Abs(b - s);
			}

			int red = difference(GetR(backdrop), GetR(source));
			int green = difference(GetG(backdrop), GetG(source));
			int blue = difference(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Exclusion(uint backdrop, uint source, int opacity)
		{
			int exclusion(int b, int s)
			{
				return b + s - 2 * MUL_UN8(b, s);
			}

			int red = exclusion(GetR(backdrop), GetR(source));
			int green = exclusion(GetG(backdrop), GetG(source));
			int blue = exclusion(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint HslHue(uint backdrop, uint source, int opacity)
		{
			double red = GetR(backdrop) / 255.0;
			double green = GetG(backdrop) / 255.0;
			double blue = GetB(backdrop) / 255.0;
			double saturation = Sat(red, green, blue);
			double luminosity = Lum(red, green, blue);

			red = GetR(source) / 255.0;
			green = GetG(source) / 255.0;
			blue = GetB(source) / 255.0;

			SetSat(ref red, ref green, ref blue, saturation);
			SetLum(ref red, ref green, ref blue, luminosity);

			uint src = RGBA((int)(255.0 * red), (int)(255.0 * green), (int)(255.0 * blue), 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint HslSaturation(uint backdrop, uint source, int opacity)
		{
			double red = GetR(source) / 255.0;
			double green = GetG(source) / 255.0;
			double blue = GetB(source) / 255.0;
			double saturation = Sat(red, green, blue);

			red = GetR(backdrop) / 255.0;
			green = GetG(backdrop) / 255.0;
			blue = GetB(backdrop) / 255.0;
			double l = Lum(red, green, blue);

			SetSat(ref red, ref green, ref blue, saturation);
			SetLum(ref red, ref green, ref blue, l);

			uint src = RGBA((int)(255.0 * red), (int)(255.0 * green), (int)(255.0 * blue), 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint HslColor(uint backdrop, uint source, int opacity)
		{
			double red = GetR(backdrop) / 255.0;
			double green = GetG(backdrop) / 255.0;
			double blue = GetB(backdrop) / 255.0;
			double luminosity = Lum(red, green, blue);

			red = GetR(source) / 255.0;
			green = GetG(source) / 255.0;
			blue = GetB(source) / 255.0;

			SetLum(ref red, ref green, ref blue, luminosity);

			uint src = RGBA((int)(255.0 * red), (int)(255.0 * green), (int)(255.0 * blue), 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint HslLuminosity(uint backdrop, uint source, int opacity)
		{
			double red = GetR(source) / 255.0;
			double green = GetG(source) / 255.0;
			double blue = GetB(source) / 255.0;
			double luminosity = Lum(red, green, blue);

			red = GetR(backdrop) / 255.0;
			green = GetG(backdrop) / 255.0;
			blue = GetB(backdrop) / 255.0;

			SetLum(ref red, ref green, ref blue, luminosity);

			uint src = RGBA((int)(255.0 * red), (int)(255.0 * green), (int)(255.0 * blue), 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Addition(uint backdrop, uint source, int opacity)
		{
			int red = GetR(backdrop) + GetR(source);
			int green = GetG(backdrop) + GetG(source);
			int blue = GetB(backdrop) + GetB(source);
			uint src = RGBA(Math.Min(red, 255),
							Math.Min(green, 255),
							Math.Min(blue, 255), 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Subtract(uint backdrop, uint source, int opacity)
		{
			int red = GetR(backdrop) - GetR(source);
			int green = GetG(backdrop) - GetG(source);
			int blue = GetB(backdrop) - GetB(source);
			uint src = RGBA(Math.Max(red, 0), Math.Max(green, 0), Math.Max(blue, 0), 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}

		private static uint Divide(uint backdrop, uint source, int opacity)
		{
			int divide(int b, int s)
			{
				if (b == 0)
				{
					return 0;
				}
				else if (b >= s)
				{
					return 255;
				}
				else
				{
					return DIV_UN8(b, s);
				}
			}
			int red = divide(GetR(backdrop), GetR(source));
			int green = divide(GetG(backdrop), GetG(source));
			int blue = divide(GetB(backdrop), GetB(source));
			uint src = RGBA(red, green, blue, 0) | (source & RGBA_A_MASK);
			return Normal(backdrop, src, opacity);
		}
	}
}