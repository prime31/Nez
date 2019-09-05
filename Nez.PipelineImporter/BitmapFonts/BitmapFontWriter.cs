using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using Nez.PipelineImporter;


namespace Nez.BitmapFontImporter
{
	[ContentTypeWriter]
	public class BitmapFontWriter : ContentTypeWriter<BitmapFontProcessorResult>
	{
		protected override void Write(ContentWriter writer, BitmapFontProcessorResult result)
		{
			writer.Write(result.PackTexturesIntoXnb);

			// write our textures if we should else write the texture names
			if (result.PackTexturesIntoXnb)
			{
				writer.Write(result.Textures.Count);
				foreach (var tex in result.Textures)
					writer.WriteObject(tex);
			}
			else
			{
				writer.Write(result.TextureNames.Count);
				for (var i = 0; i < result.TextureNames.Count; i++)
				{
					writer.Write(result.TextureNames[i]);
					writer.Write(result.TextureOrigins[i]);
				}
			}

			// write the font data
			var fontFile = result.FontFile;
			writer.Write(fontFile.Common.LineHeight);

			var padding = fontFile.Info.Padding.Split(new char[] {','});
			if (padding.Length != 4)
				throw new PipelineException("font padding is invalid! It should contain 4 values");

			writer.Write(int.Parse(padding[0])); // top
			writer.Write(int.Parse(padding[1])); // left
			writer.Write(int.Parse(padding[2])); // bottom
			writer.Write(int.Parse(padding[3])); // right

			writer.Write(GetDescent(fontFile, int.Parse(padding[2])));

			writer.Write(fontFile.Chars.Count);
			foreach (var c in fontFile.Chars)
			{
				writer.Write(c.Id);
				writer.Write(c.Page);
				writer.Write(c.X);
				writer.Write(c.Y);
				writer.Write(c.Width);
				writer.Write(c.Height);
				writer.Write(c.XOffset);
				writer.Write(c.YOffset);
				writer.Write(c.XAdvance);
			}
		}


		int GetDescent(BitmapFontFile fontFile, int padBottom)
		{
			var descent = 0;

			foreach (var c in fontFile.Chars)
			{
				if (c.Width > 0 && c.Height > 0)
					descent = Math.Min(fontFile.Common.Base_ + c.YOffset + fontFile.Info.OutLine, descent);
			}

			return descent + padBottom;
		}


		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(Nez.BitmapFonts.BitmapFont).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(Nez.BitmapFonts.BitmapFontReader).AssemblyQualifiedName;
		}
	}
}